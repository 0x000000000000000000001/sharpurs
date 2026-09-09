module Sharpurs.ThunkKernel (ThunkModule, Helper, Worker, prepareModule, fromExpr) where

import Prelude

import Control.Alternative (guard)
import Data.Array as Array
import Data.Array.NonEmpty as NEA
import Data.Foldable (all, foldMap, foldr)
import Data.Map (Map)
import Data.Map as Map
import Data.Maybe (Maybe(..))
import Data.Newtype (unwrap)
import Data.String as String
import Data.String.Pattern (Pattern(..), Replacement(..))
import Data.Traversable (traverse)
import Data.Tuple (Tuple(..))
import PureScript.Backend.Optimizer.Convert (BackendModule)
import PureScript.Backend.Optimizer.CoreFn (Ann, ExprType, Ident, Module(..), Qualified(..))
import PureScript.Backend.Optimizer.CoreFn as C
import PureScript.Backend.Optimizer.Semantics (NeutralExpr(..))
import PureScript.Backend.Optimizer.Syntax (Level(..), Pair(..))
import PureScript.Backend.Optimizer.Syntax as S
import Sharpurs.AdtLayout (validIdentifier)
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), sanitizeName)
import Sharpurs.IntArithmetic as IntArithmetic

-- Public values keep their object ABI. These workers are usable only by the
-- closed call-site recognizer below: typed callbacks alone do not prove purity.
data Helper = Identity | Force

derive instance eqHelper :: Eq Helper

type Worker = { name :: Qualified Ident, nativeName :: String, args :: Array ExprType, result :: ExprType }
type Parameter = { level :: Level, type :: ExprType }
type Context = { worker :: Worker, helpers :: Map (Qualified Ident) Helper, locals :: Map Level ExprType }
type SourceBinding = { name :: Ident, expr :: C.Expr Ann, recursive :: Boolean, singleton :: Boolean }

type ThunkModule =
  { declarations :: Array FsDecl
  , nativeNames :: Array Ident
  , helpers :: Map (Qualified Ident) Helper
  , workers :: Map (Qualified Ident) Worker
  }

thunk :: ExprType
thunk = C.Func [ C.Unit ] C.Int

arrow :: Array ExprType -> ExprType -> ExprType
arrow args result = if Array.null args then result else case result of
  C.Func tail ret -> C.Func (args <> tail) ret
  _ -> C.Func args result

annotation :: C.Expr Ann -> Maybe ExprType
annotation expr = case C.exprAnn expr of
  C.Ann ann -> ann.type

pboAnnotation :: NeutralExpr -> Maybe ExprType
pboAnnotation (NeutralExpr (S.Typed ty _)) = Just ty
pboAnnotation _ = Nothing

strip :: NeutralExpr -> NeutralExpr
strip (NeutralExpr (S.Typed _ inner)) = strip inner
strip expr = expr

sourceGroup :: C.Bind Ann -> Array SourceBinding
sourceGroup = case _ of
  C.NonRec (C.Binding _ name expr) -> [ { name, expr, recursive: false, singleton: true } ]
  C.Rec [ C.Binding _ name expr ] -> [ { name, expr, recursive: true, singleton: true } ]
  C.Rec bindings -> map (\(C.Binding _ name expr) -> { name, expr, recursive: true, singleton: false }) bindings

lookupExpression :: Ident -> BackendModule -> Maybe NeutralExpr
lookupExpression name backend = Array.findMap
  (\(Tuple ident expr) -> if ident == name then Just expr else Nothing)
  (Array.concatMap _.bindings backend.bindings)

helperType :: Helper -> ExprType -> ExprType
helperType Identity result = arrow [ C.Func [ C.Unit ] result ] (C.Func [ C.Unit ] result)
helperType Force result = C.Func [ C.Func [ C.Unit ] result ] result

helperShape :: Helper -> ExprType -> Boolean
helperShape kind ty = case ty of
  C.ForAll [ variable ] body -> body == helperType kind (C.TypeVar variable)
  C.Func _ (C.TypeVar variable) -> ty == helperType kind (C.TypeVar variable)
  _ -> ty == helperType kind C.Int

unitReference :: Qualified Ident -> Boolean
unitReference = case _ of
  Qualified (Just (C.ModuleName "Data.Unit")) (C.Ident "unit") -> true
  _ -> false

helperResult :: Helper -> ExprType -> Maybe ExprType
helperResult kind ty = case ty of
  C.ForAll [ variable ] body -> guard (body == helperType kind (C.TypeVar variable)) $> C.TypeVar variable
  C.Func _ result@(C.TypeVar _) -> guard (ty == helperType kind result) $> result
  _ -> guard (ty == helperType kind C.Int) $> C.Int

peel :: ExprType -> NeutralExpr -> Maybe NeutralExpr
peel expected (NeutralExpr (S.Typed ty inner)) = guard (expected == ty) *> peel expected inner
peel _ expr = Just expr

helperBody :: Helper -> NeutralExpr -> Boolean
helperBody kind expr = case do
  ty <- pboAnnotation expr
  result <- helperResult kind ty
  let callback = C.Func [ C.Unit ] result
      returned = if kind == Identity then callback else result
      unquantified = case expr of
        NeutralExpr (S.Typed (C.ForAll _ _) inner) -> inner
        _ -> expr
  fn <- peel (helperType kind result) unquantified
  case fn of
    NeutralExpr (S.Abs parameters body) -> case NEA.toArray parameters of
      [ Tuple _ parameter ] -> do
        returnedBody <- peel returned body
        case kind, returnedBody of
          Identity, NeutralExpr (S.Local _ level) -> guard (level == parameter)
          Force, NeutralExpr (S.App target args) -> do
            targetBody <- peel callback target
            arguments <- traverse (peel C.Unit) (NEA.toArray args)
            case targetBody, arguments of
              NeutralExpr (S.Local _ level), [ NeutralExpr (S.Var name) ] -> guard (level == parameter && unitReference name)
              _, _ -> Nothing
          _, _ -> Nothing
      _ -> Nothing
    _ -> Nothing
  of
    Just _ -> true
    Nothing -> false

helperSource :: Helper -> C.Expr Ann -> Boolean
helperSource kind expr = case do
  ty <- annotation expr
  result <- helperResult kind ty
  case expr of
    C.ExprAbs (C.Ann ann) parameter body -> do
      let returned = if kind == Identity then C.Func [ C.Unit ] result else result
          newtypeIdentity = kind == Identity && ann.meta == Just C.IsNewtype && case body of
            C.ExprVar _ (Qualified Nothing name) -> name == parameter && annotation body == Nothing
            _ -> false
      guard (annotation body == Just returned || newtypeIdentity)
    _ -> Nothing
  of
    Just _ -> true
    Nothing -> false

prepareModule :: Module Ann -> BackendModule -> Maybe ThunkModule
prepareModule (Module source) backend = do
  guard (source.name == backend.name)
  let
    bindings = Array.concatMap sourceGroup source.decls
    qualify = Qualified (Just source.name)
    prefix = String.replaceAll (Pattern ".") (Replacement "_") (unwrap source.name)
    publicNames = map (\name -> sanitizeName (prefix <> "_" <> unwrap name))
      (map _.name bindings <> Array.fromFoldable (Map.keys source.foreign))
    localNames = foldMap (sourceNames <<< _.expr) bindings
    helpers = Array.foldl (\known binding -> case do
      guard (binding.singleton && not binding.recursive)
      expr <- lookupExpression binding.name backend
      sourceType <- annotation binding.expr
      optimizedType <- pboAnnotation expr
      let direct kind = helperShape kind sourceType && helperShape kind optimizedType && helperBody kind expr && helperSource kind binding.expr
          allowed name = case name of
            Qualified Nothing _ -> true
            _ -> unitReference name || Map.member name known
      guard (all allowed (sourceReferences binding.expr))
      kind <- if direct Identity then Just Identity else if direct Force then Just Force else case strip expr, binding.expr of
        NeutralExpr (S.Var name), C.ExprVar _ sourceName | name == sourceName -> do
          kind <- Map.lookup name known
          guard (helperShape kind sourceType && helperShape kind optimizedType)
          pure kind
        _, _ -> Nothing
      pure kind
      of
        Just kind -> Map.insert (qualify binding.name) kind known
        Nothing -> known) Map.empty bindings
    selected = Array.mapMaybe (\binding -> do
      guard binding.singleton
      group <- Array.find (Array.any (\(Tuple name _) -> name == binding.name) <<< _.bindings) backend.bindings
      guard (group.recursive == binding.recursive && (not group.recursive || Array.length group.bindings == 1))
      expression <- lookupExpression binding.name backend
      sourceType <- annotation binding.expr
      guard (pboAnnotation expression == Just sourceType)
      let count = sourceArity binding.expr
      worker <- case sourceType of
        C.Func arguments result -> do
          guard (count > 0 && count < Array.length arguments)
          let args = Array.take count arguments
              returned = arrow (Array.drop count arguments) result
          guard (returned == thunk && Array.elem thunk args)
          _ <- traverse nativeType args
          pure { name: qualify binding.name, nativeName: sanitizeName (prefix <> "_" <> unwrap binding.name) <> "_thunk_native", args, result: returned }
        _ -> Nothing
      validateSource worker.args worker.result binding.expr
      guard (all (allowedReference worker helpers) (sourceReferences binding.expr))
      guard (binding.recursive || not (Array.elem worker.name (sourceReferences binding.expr)))
      collected <- collect worker.args worker.result [] expression
      let levels = map _.level collected.args
      guard (all (\(Level level) -> level >= 0) levels && unique levels)
      body <- lower { worker, helpers, locals: Map.fromFoldable (map (\p -> Tuple p.level p.type) collected.args) } worker.result collected.body
      params <- traverse printParameter collected.args
      result <- nativeType worker.result
      guard (validIdentifier worker.nativeName && not (Array.elem worker.nativeName (publicNames <> localNames)))
      pure { worker, declaration: FsRaw ((if binding.recursive then "let rec private " else "let private ") <> worker.nativeName <> " " <> String.joinWith " " params <> " : " <> result <> " = " <> body) }
      ) bindings
  guard (not (Array.null selected) && unique publicNames && unique (map (_.nativeName <<< _.worker) selected))
  pure
    { declarations: map _.declaration selected
    , nativeNames: map (C.unQualified <<< _.name <<< _.worker) selected
    , workers: Map.fromFoldable (map (\item -> Tuple item.worker.name item.worker) selected)
    , helpers
    }

unique :: forall a. Ord a => Array a -> Boolean
unique values = Array.length (Array.nub values) == Array.length values

allowedReference :: Worker -> Map (Qualified Ident) Helper -> Qualified Ident -> Boolean
allowedReference worker helpers = case _ of
  Qualified Nothing _ -> true
  name | name == worker.name || Map.member name helpers || unitReference name -> true
  Qualified (Just (C.ModuleName owner)) (C.Ident name) -> case owner of
    "Data.Semiring" -> Array.elem name [ "add", "semiringInt" ]
    "Data.Ring" -> Array.elem name [ "sub", "ringInt" ]
    "Data.Eq" -> Array.elem name [ "eq", "notEq", "eqInt" ]
    "Data.Ord" -> Array.elem name [ "lessThan", "lessThanOrEq", "greaterThan", "greaterThanOrEq", "ordInt" ]
    _ -> false

sourceArity :: C.Expr Ann -> Int
sourceArity = case _ of
  C.ExprAbs _ _ body -> 1 + sourceArity body
  _ -> 0

validateSource :: Array ExprType -> ExprType -> C.Expr Ann -> Maybe Unit
validateSource remaining result expr = do
  guard (annotation expr == Just (arrow remaining result))
  if Array.null remaining then pure unit else case expr of
    C.ExprAbs _ _ body -> validateSource (Array.drop 1 remaining) result body
    _ -> Nothing

nativeType :: ExprType -> Maybe String
nativeType = case _ of
  C.Int -> Just "int"
  C.Boolean -> Just "bool"
  C.Unit -> Just "unit"
  ty | ty == thunk -> Just "(unit -> int)"
  _ -> Nothing

collect :: Array ExprType -> ExprType -> Array Parameter -> NeutralExpr -> Maybe { args :: Array Parameter, body :: NeutralExpr }
collect remaining result args expr@(NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == arrow remaining result) *> collect remaining result args inner
  _ | Array.null remaining -> Just { args, body: expr }
  S.Abs parameters body -> do
    let ps = NEA.toArray parameters
    guard (NEA.length parameters <= Array.length remaining)
    let next = Array.zipWith (\(Tuple _ level) ty -> { level, type: ty }) ps remaining
    collect (Array.drop (Array.length ps) remaining) result (args <> next) body
  _ -> Nothing

localName :: Level -> String
localName (Level level) = "sharpurs_thunk_local_" <> show level

printParameter :: Parameter -> Maybe String
printParameter p = do
  ty <- nativeType p.type
  pure ("(" <> localName p.level <> ": " <> ty <> ")")

lower :: Context -> ExprType -> NeutralExpr -> Maybe String
lower ctx expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> lower ctx expected inner
  S.Lit (C.LitInt n) -> guard (expected == C.Int) $> ("(" <> show n <> ")")
  S.Lit (C.LitBoolean b) -> guard (expected == C.Boolean) $> (if b then "true" else "false")
  S.Local _ level -> do
    guard (Map.lookup level ctx.locals == Just expected)
    pure (localName level)
  S.Var name -> guard (expected == C.Unit && unitReference name) $> "()"
  S.Abs parameters body -> do
    guard (expected == thunk)
    case NEA.toArray parameters of
      [ Tuple _ level@(Level n) ] -> do
        guard (n >= 0 && not (Map.member level ctx.locals))
        bodyCode <- lower (ctx { locals = Map.insert level C.Unit ctx.locals }) C.Int body
        pure ("(fun (" <> localName level <> ": unit) -> " <> bodyCode <> ")")
      _ -> Nothing
  S.App fn args -> case strip fn of
    NeutralExpr (S.Local _ level) -> do
      guard (expected == C.Int && Map.lookup level ctx.locals == Just thunk)
      referenceType thunk fn
      case NEA.toArray args of
        [ arg ] -> do
          argCode <- lower ctx C.Unit arg
          pure ("(" <> localName level <> " " <> argCode <> ")")
        _ -> Nothing
    NeutralExpr (S.Var name) | name == ctx.worker.name -> do
      guard (expected == ctx.worker.result && NEA.length args == Array.length ctx.worker.args)
      referenceType (arrow ctx.worker.args ctx.worker.result) fn
      values <- traverse (\(Tuple ty arg) -> lower ctx ty arg) (Array.zip ctx.worker.args (NEA.toArray args))
      pure ("(" <> ctx.worker.nativeName <> String.joinWith "" (map (\v -> " (" <> v <> ")") values) <> ")")
    NeutralExpr (S.Var name) -> do
      helper <- Map.lookup name ctx.helpers
      referenceType (helperType helper C.Int) fn
      case helper, NEA.toArray args of
        Identity, [ arg ] -> guard (expected == thunk) *> lower ctx thunk arg
        Force, [ arg ] -> do
          guard (expected == C.Int)
          value <- lower ctx thunk arg
          pure ("(" <> value <> " ())")
        _, _ -> Nothing
    _ -> Nothing
  S.PrimOp (S.Op2 op left right) -> do
    prim <- primitive op
    guard (expected == prim.result)
    a <- lower ctx C.Int left
    b <- lower ctx C.Int right
    pure ("(" <> a <> " " <> prim.symbol <> " " <> b <> ")")
  S.Let _ level@(Level n) value body -> do
    guard (n >= 0 && not (Map.member level ctx.locals) && all (_ < level) (Map.keys ctx.locals))
    ty <- typeOf ctx value
    native <- nativeType ty
    rhs <- lower ctx ty value
    rest <- lower (ctx { locals = Map.insert level ty ctx.locals }) expected body
    pure ("(let " <> localName level <> ": " <> native <> " = " <> rhs <> " in " <> rest <> ")")
  S.Branch branches otherwise -> do
    cases <- traverse (\(Pair condition value) -> Tuple <$> lower ctx C.Boolean condition <*> lower ctx expected value) (NEA.toArray branches)
    fallback <- lower ctx expected otherwise
    pure (foldr (\(Tuple condition value) rest -> "(if " <> condition <> " then " <> value <> " else " <> rest <> ")") fallback cases)
  -- Failures, foreigns, unknown calls and other callback shapes stay boxed.
  _ -> Nothing

referenceType :: ExprType -> NeutralExpr -> Maybe Unit
referenceType expected (NeutralExpr syntax) = case syntax of
  S.Typed ty inner -> guard (ty == expected) *> referenceType expected inner
  S.Var _ -> Just unit
  S.Local _ _ -> Just unit
  _ -> Nothing

primitive :: S.BackendOperator2 -> Maybe { symbol :: String, result :: ExprType }
primitive = case _ of
  S.OpIntNum S.OpAdd -> Just { symbol: "+", result: C.Int }
  S.OpIntNum S.OpSubtract -> Just { symbol: "-", result: C.Int }
  S.OpIntOrd op -> Just { symbol: case op of
    S.OpEq -> "="
    S.OpNotEq -> "<>"
    S.OpLt -> "<"
    S.OpLte -> "<="
    S.OpGt -> ">"
    S.OpGte -> ">=", result: C.Boolean }
  _ -> Nothing

typeOf :: Context -> NeutralExpr -> Maybe ExprType
typeOf ctx (NeutralExpr syntax) = case syntax of
  S.Typed ty _ -> Just ty
  S.Local _ level -> Map.lookup level ctx.locals
  S.Lit (C.LitInt _) -> Just C.Int
  S.Lit (C.LitBoolean _) -> Just C.Boolean
  S.Var name | unitReference name -> Just C.Unit
  S.PrimOp (S.Op2 op _ _) -> _.result <$> primitive op
  S.Abs _ _ -> Just thunk
  S.App fn _ -> case strip fn of
    NeutralExpr (S.Local _ level) -> guard (Map.lookup level ctx.locals == Just thunk) $> C.Int
    NeutralExpr (S.Var name) | name == ctx.worker.name -> Just ctx.worker.result
    NeutralExpr (S.Var name) -> case Map.lookup name ctx.helpers of
      Just Identity -> Just thunk
      Just Force -> Just C.Int
      _ -> Nothing
    _ -> Nothing
  _ -> Nothing

fromExpr :: ThunkModule -> C.Expr Ann -> Maybe FsExpr
fromExpr selected expr = do
  guard (annotation expr == Just C.Int)
  case expr of
    C.ExprApp _ fn value -> do
      name <- helperReference Force fn
      guard (Map.lookup name selected.helpers == Just Force)
      native <- sourceThunk selected Map.empty value
      pure (FsIdent ("(box (" <> native <> " ()))"))
    _ -> Nothing

helperReference :: Helper -> C.Expr Ann -> Maybe (Qualified Ident)
helperReference kind = case _ of
  expr@(C.ExprVar _ name) -> guard (annotation expr == Just (helperType kind C.Int)) $> name
  expr@(C.ExprTypeApp _ generic@(C.ExprVar _ name) C.Int) -> do
    guard (annotation expr == Just (helperType kind C.Int))
    ty <- annotation generic
    case ty of
      C.ForAll [ variable ] body -> guard (body == helperType kind (C.TypeVar variable)) $> name
      _ -> Nothing
  _ -> Nothing

sourceThunk :: ThunkModule -> Map Ident String -> C.Expr Ann -> Maybe String
sourceThunk selected locals expr = do
  guard (annotation expr == Just thunk)
  case expr of
    C.ExprAbs _ name body -> do
      let captures = Array.nub (Array.filter (_ /= name) (intReferences body))
          reserved = map (sanitizeName <<< unwrap) captures <> sourceNames body <> Array.fromFoldable (Map.values locals)
          fresh i = "sharpurs_thunk_capture_" <> show i
          available = Array.filter (\candidate -> not (Array.elem candidate reserved))
            (map fresh (Array.range 0 (Array.length captures + Array.length reserved)))
          bindings = Array.zip captures available
      values <- traverse (\(Tuple ident target) -> do
        value <- case Map.lookup ident locals of
          Just "()" -> Nothing
          Just value -> Just value
          Nothing -> Just ("(unbox<int> (box " <> sanitizeName (unwrap ident) <> "))")
        pure { target, value }) bindings
      bodyCode <- sourceInt (Map.insert name "()" (Map.union (Map.fromFoldable bindings) locals)) body
      pure (foldr (\capture rest -> "(let " <> capture.target <> ": int = " <> capture.value <> " in " <> rest <> ")")
        ("(fun () -> " <> bodyCode <> ")") values)
    C.ExprApp _ fn value | Just name <- helperReference Identity fn -> do
      guard (Map.lookup name selected.helpers == Just Identity)
      sourceThunk selected locals value
    _ -> do
      let flat = flattenApp expr
      name <- case flat.fn of
        C.ExprVar _ name -> Just name
        _ -> Nothing
      worker <- Map.lookup name selected.workers
      guard (annotation flat.fn == Just (arrow worker.args worker.result))
      guard (Array.length flat.args == Array.length worker.args)
      validateApplications worker.args worker.result expr
      values <- traverse (\(Tuple ty arg) -> if ty == thunk then sourceThunk selected locals arg
        else if ty == C.Int then sourceInt locals arg else Nothing) (Array.zip worker.args flat.args)
      pure ("(" <> worker.nativeName <> String.joinWith "" (map (\value -> " (" <> value <> ")") values) <> ")")

sourceInt :: Map Ident String -> C.Expr Ann -> Maybe String
sourceInt locals expr = do
  guard (annotation expr == Just C.Int)
  case expr of
    C.ExprLit _ (C.LitInt n) -> Just ("(" <> show n <> ")")
    C.ExprVar _ (Qualified Nothing name) -> do
      case Map.lookup name locals of
        Just "()" -> Nothing
        Just native -> Just native
        Nothing -> Just ("(unbox<int> (box " <> sanitizeName (unwrap name) <> "))")
    _ -> do
      arithmetic <- IntArithmetic.fromExpr expr
      a <- sourceInt locals arithmetic.left
      b <- sourceInt locals arithmetic.right
      symbol <- case arithmetic.operator of
        S.OpAdd -> Just "+"
        S.OpSubtract -> Just "-"
        _ -> Nothing
      pure ("(" <> a <> " " <> symbol <> " " <> b <> ")")

intReferences :: C.Expr Ann -> Array Ident
intReferences expr = case expr of
  C.ExprVar _ (Qualified Nothing name) | annotation expr == Just C.Int -> [ name ]
  _ -> foldMap intReferences (sourceChildren expr)

flattenApp :: C.Expr Ann -> { fn :: C.Expr Ann, args :: Array (C.Expr Ann) }
flattenApp (C.ExprApp _ fn arg) = let flat = flattenApp fn in flat { args = Array.snoc flat.args arg }
flattenApp expr = { fn: expr, args: [] }

validateApplications :: Array ExprType -> ExprType -> C.Expr Ann -> Maybe Unit
validateApplications args result expr = do
  guard (annotation expr == Just result)
  case Array.unsnoc args, expr of
    Just { init, last }, C.ExprApp _ fn arg -> do
      guard (annotation arg == Just last)
      validateApplications init (arrow [ last ] result) fn
    Nothing, _ -> pure unit
    _, _ -> Nothing

sourceReferences :: C.Expr Ann -> Array (Qualified Ident)
sourceReferences = case _ of
  C.ExprVar _ name -> [ name ]
  expr -> foldMap sourceReferences (sourceChildren expr)

sourceNames :: C.Expr Ann -> Array String
sourceNames expr = map (sanitizeName <<< unwrap) (case expr of
  C.ExprAbs _ name _ -> [ name ]
  C.ExprLet _ bindings _ -> map _.name (Array.concatMap sourceGroup bindings)
  C.ExprCase _ _ branches -> foldMap (\(C.CaseAlternative binders _) -> foldMap binderNames binders) branches
  _ -> []) <> foldMap sourceNames (sourceChildren expr)
  where
  binderNames = case _ of
    C.BinderVar _ name -> [ name ]
    C.BinderNamed _ name inner -> [ name ] <> binderNames inner
    C.BinderLit _ literal -> foldMap binderNames literal
    C.BinderConstructor _ _ _ binders -> foldMap binderNames binders
    _ -> []

sourceChildren :: C.Expr Ann -> Array (C.Expr Ann)
sourceChildren = case _ of
  C.ExprLit _ literal -> foldMap (\expr -> [ expr ]) literal
  C.ExprAccessor _ target _ -> [ target ]
  C.ExprUpdate _ target props -> [ target ] <> map C.propValue props
  C.ExprAbs _ _ body -> [ body ]
  C.ExprApp _ fn arg -> [ fn, arg ]
  C.ExprCase _ targets branches -> targets <> foldMap (\(C.CaseAlternative _ result) -> case result of
    C.Unconditional body -> [ body ]
    C.Guarded guards -> foldMap (\(C.Guard condition body) -> [ condition, body ]) guards) branches
  C.ExprLet _ bindings body -> map _.expr (Array.concatMap sourceGroup bindings) <> [ body ]
  C.ExprTypeApp _ expr _ -> [ expr ]
  _ -> []
