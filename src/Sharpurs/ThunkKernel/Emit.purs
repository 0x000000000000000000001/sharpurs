-- | F# templates for already checked workers, calls and captures. These render
-- | the native subset; the public curried object ABI is emitted by CodeGen.
module Sharpurs.ThunkKernel.Emit
  ( Definition
  , CaptureValue
  , Branch
  , worker
  , localName
  , intLiteral
  , booleanLiteral
  , unitValue
  , lambda
  , localCall
  , call
  , force
  , binary
  , letIn
  , branch
  , capturedInt
  , capturedThunk
  , boxForced
  ) where

import Prelude

import Data.Foldable (foldr)
import Data.Maybe (Maybe)
import Data.Newtype (unwrap)
import Data.String as String
import Data.Traversable (traverse)
import PureScript.Backend.Optimizer.CoreFn (Ident)
import PureScript.Backend.Optimizer.Syntax (Level(..))
import Sharpurs.FsAst (FsDecl(..), FsExpr(..), sanitizeName)
import Sharpurs.ThunkKernel.Analysis (Parameter, Worker, nativeType)

type Definition = { parameters :: Array Parameter, body :: String }
type CaptureValue = { target :: String, value :: String }
type Branch = { condition :: String, body :: String }

worker :: Worker -> Boolean -> Definition -> Maybe FsDecl
worker selected recursive lowered = do
  parameters <- traverse printParameter lowered.parameters
  result <- nativeType selected.result
  pure (FsRaw ((if recursive then "let rec private " else "let private ") <> selected.nativeName <> " " <> String.joinWith " " parameters <> " : " <> result <> " = " <> lowered.body))

printParameter :: Parameter -> Maybe String
printParameter p = do
  ty <- nativeType p.type
  pure ("(" <> localName p.level <> ": " <> ty <> ")")

localName :: Level -> String
localName (Level level) = "sharpurs_thunk_local_" <> show level

intLiteral :: Int -> String
intLiteral n = "(" <> show n <> ")"

booleanLiteral :: Boolean -> String
booleanLiteral b = if b then "true" else "false"

unitValue :: String
unitValue = "()"

lambda :: Level -> String -> String
lambda level body = "(fun (" <> localName level <> ": unit) -> " <> body <> ")"

localCall :: Level -> String -> String
localCall level arg = "(" <> localName level <> " " <> arg <> ")"

call :: String -> Array String -> String
call target args = "(" <> target <> String.joinWith "" (map (\value -> " (" <> value <> ")") args) <> ")"

force :: String -> String
force value = "(" <> value <> " ())"

binary :: String -> String -> String -> String
binary symbol left right = "(" <> left <> " " <> symbol <> " " <> right <> ")"

letIn :: Level -> String -> String -> String -> String
letIn level native value body = "(let " <> localName level <> ": " <> native <> " = " <> value <> " in " <> body <> ")"

branch :: Array Branch -> String -> String
branch cases otherwise = foldr (\item rest -> "(if " <> item.condition <> " then " <> item.body <> " else " <> rest <> ")") otherwise cases

capturedInt :: Ident -> String
capturedInt name = "(unbox<int> (box " <> sanitizeName (unwrap name) <> "))"

-- Capture values are evaluated when constructing the closure. Its body stays
-- delayed and is evaluated afresh on each force; there is no memoization.
capturedThunk :: Array CaptureValue -> String -> String
capturedThunk captures body = foldr
  (\capture rest -> "(let " <> capture.target <> ": int = " <> capture.value <> " in " <> rest <> ")")
  ("(fun () -> " <> body <> ")") captures

boxForced :: String -> FsExpr
boxForced value = FsRawExpr ("(box (" <> value <> " ()))")
