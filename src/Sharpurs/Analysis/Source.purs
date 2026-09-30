-- | Structural views of the source TAST. These helpers preserve annotations and
-- | source order; each recognizer remains responsible for proving eligibility.
module Sharpurs.Analysis.Source
  ( annotation
  , hasType
  , hasPolymorphicType
  , Parameter
  , lambdas
  , Argument
  , applications
  , flattenApp
  , SourceBinding
  , bindings
  , children
  , references
  ) where

import Prelude

import Data.Array as Array
import Data.Foldable (foldMap)
import Data.Maybe (Maybe(..))
import PureScript.Backend.Optimizer.CoreFn (Ann(..), ExprType(..), Ident, Qualified)
import PureScript.Backend.Optimizer.CoreFn as C

annotation :: C.Expr Ann -> Maybe ExprType
annotation expr = case C.exprAnn expr of
  Ann ann -> ann.type

-- Missing annotations are not evidence of a type. Equality stays structural;
-- instantiation, inference and validation of nested signatures belong to callers.
hasType :: ExprType -> Ann -> Boolean
hasType expected (Ann ann) = ann.type == Just expected

-- Check exactly one quantified variable against a caller-supplied signature.
hasPolymorphicType :: (ExprType -> ExprType) -> Ann -> Boolean
hasPolymorphicType signature (Ann ann) = case ann.type of
  Just (ForAll [ variable ] body) -> body == signature (TypeVar variable)
  _ -> false

type Parameter a = { name :: Ident, ann :: a }

-- Only consecutive source lambdas establish arity. A let, case or TypeApp ends
-- this spine, even if its annotated result is another function.
lambdas :: forall a. C.Expr a -> { parameters :: Array (Parameter a), body :: C.Expr a }
lambdas = case _ of
  C.ExprAbs ann name body ->
    let rest = lambdas body
    in rest { parameters = Array.cons { name, ann } rest.parameters }
  body -> { parameters: [], body }

-- ann belongs to the enclosing App: its type is the result after this argument.
type Argument a = { value :: C.Expr a, ann :: a }

-- Preserve the head and every argument/application annotation. TypeApp is a
-- boundary here; constructor saturation has its own explicitly transparent walk.
applications :: forall a. C.Expr a -> { head :: C.Expr a, arguments :: Array (Argument a) }
applications expr = go expr []
  where
  go expression arguments = case expression of
    C.ExprApp ann head value -> go head (Array.cons { value, ann } arguments)
    head -> { head, arguments }

flattenApp :: forall a. C.Expr a -> { fn :: C.Expr a, args :: Array (C.Expr a) }
flattenApp expr =
  let call = applications expr
  in { fn: call.head, args: map _.value call.arguments }

type SourceBinding a = { name :: Ident, expr :: C.Expr a, recursive :: Boolean, singleton :: Boolean }

-- Flatten a binding group without losing whether it belongs to mutual recursion.
bindings :: forall a. C.Bind a -> Array (SourceBinding a)
bindings = case _ of
  C.NonRec (C.Binding _ name expr) -> [ { name, expr, recursive: false, singleton: true } ]
  C.Rec [ C.Binding _ name expr ] -> [ { name, expr, recursive: true, singleton: true } ]
  C.Rec group -> map (\(C.Binding _ name expr) -> { name, expr, recursive: true, singleton: false }) group

-- Visit all expression children, including guards, record updates and let RHSs.
-- Unlike an application spine, this structural walk descends through TypeApp.
children :: forall a. C.Expr a -> Array (C.Expr a)
children = case _ of
  C.ExprLit _ literal -> foldMap (\expr -> [ expr ]) literal
  C.ExprAccessor _ target _ -> [ target ]
  C.ExprUpdate _ target props -> [ target ] <> map C.propValue props
  C.ExprAbs _ _ body -> [ body ]
  C.ExprApp _ fn arg -> [ fn, arg ]
  C.ExprCase _ targets branches -> targets <> foldMap (\(C.CaseAlternative _ result) -> case result of
    C.Unconditional body -> [ body ]
    C.Guarded guards -> foldMap (\(C.Guard condition body) -> [ condition, body ]) guards) branches
  C.ExprLet _ groups body -> map _.expr (Array.concatMap bindings groups) <> [ body ]
  C.ExprTypeApp _ expr _ -> [ expr ]
  _ -> []

references :: forall a. C.Expr a -> Array (Qualified Ident)
references = case _ of
  C.ExprVar _ name -> [ name ]
  expr -> foldMap references (children expr)
