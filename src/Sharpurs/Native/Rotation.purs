-- | Generic binary-tree rotations expressed in MutableIR. The node layout is
-- | defined by the field indices passed by the caller (left, key, right).
module Sharpurs.Native.Rotation
  ( Candidate
  , detect
  , plan
  , rotateLeft
  , rotateRight
  ) where

import Prelude

import Data.Array as Array
import Data.Maybe (Maybe(..))
import Sharpurs.Native.Destination as Destination
import Sharpurs.Native.IR (NativeCase, NativeExpr(..))
import Sharpurs.Native.MutableIR (Location(..), MutableExpr(..), MutableStmt(..))
import Sharpurs.Native.Pattern (NativePattern(..))
import Sharpurs.Native.Types as NativeTypes

-- | Rotate a node left around its right child. Field 0 is the left link,
-- | field 2 the right link; field 1 is carried through untouched.
rotateLeft :: Location -> MutableExpr
rotateLeft root =
  MLet "sharpurs_rotation_root" (MRead root)
    (MLet "sharpurs_rotation_right" (MRead (Field (Local "sharpurs_rotation_root") 2))
      (MSeq
        [ Assign (Field (Local "sharpurs_rotation_root") 2)
            (MRead (Field (Local "sharpurs_rotation_right") 0))
        , Assign (Field (Local "sharpurs_rotation_right") 0)
            (MRead (Local "sharpurs_rotation_root"))
        ]
        (MRead (Local "sharpurs_rotation_right"))))

-- | Rotate a node right around its left child.
rotateRight :: Location -> MutableExpr
rotateRight root =
  MLet "sharpurs_rotation_root" (MRead root)
    (MLet "sharpurs_rotation_left" (MRead (Field (Local "sharpurs_rotation_root") 0))
      (MSeq
        [ Assign (Field (Local "sharpurs_rotation_root") 0)
            (MRead (Field (Local "sharpurs_rotation_left") 2))
        , Assign (Field (Local "sharpurs_rotation_left") 2)
            (MRead (Local "sharpurs_rotation_root"))
        ]
        (MRead (Local "sharpurs_rotation_left"))))

type Candidate =
  { target :: String
  , constructor :: String
  , children :: Array String
  , arguments :: Array NativeExpr
  }

-- | Find branches that consume a constructor and rebuild the same constructor
-- | with at least one nested child constructor. This is the structural shape
-- | a rotation/destination-passing pass can optimize; no semantic rewrite is
-- | made here.
detect :: NativeExpr -> Array Candidate
detect expression = case expression of
  NativeMatch targets cases fallback ->
    Array.concatMap (candidate targets) cases
      <> Array.concatMap detect targets
      <> Array.concatMap (detect <<< _.body) cases
      <> case fallback of
          Nothing -> []
          Just body -> detect body
  NativeLambda _ body -> detect body
  NativeApply function arguments -> detect function <> Array.concatMap detect arguments
  NativeLet _ _ value body -> detect value <> detect body
  NativeConstructor _ arguments -> Array.concatMap detect arguments
  NativeReuse _ target arguments -> detect target <> Array.concatMap detect arguments
  NativeProject _ _ value -> detect value
  NativePrimitive _ arguments -> Array.concatMap detect arguments
  NativeBox expression' -> detect expression'
  NativeCopy _ expression' -> detect expression'
  NativeCast _ expression' -> detect expression'
  _ -> []

candidate :: Array NativeExpr -> NativeCase -> Array Candidate
candidate targets case' = case targets of
  [ NativeVar target _ ] -> case case'.patterns of
    [ PatternConstructor constructor fields ] ->
      case constructorArguments constructor case'.body of
        Just arguments ->
          [ { target
            , constructor
            , children: Array.concatMap childNames fields
            , arguments
            }
          ]
        Nothing -> []
    _ -> []
  _ -> []

childNames :: NativePattern -> Array String
childNames = case _ of
  PatternConstructor name _ -> [ name ]
  PatternNamed _ inner -> childNames inner
  _ -> []

constructorArguments :: String -> NativeExpr -> Maybe (Array NativeExpr)
constructorArguments constructor = case _ of
  NativeConstructor name arguments | name == constructor -> Just arguments
  NativeLet _ _ _ body -> constructorArguments constructor body
  NativeMatch _ cases _ ->
    Array.findMap (constructorArguments constructor <<< _.body) cases
  NativeReuse name _ arguments | name == constructor -> Just arguments
  _ -> Nothing

plan :: Candidate -> Maybe MutableExpr
plan candidate =
  Destination.lowerReuse candidate.constructor
    (NativeVar candidate.target (NativeTypes.NativeBoxed))
    candidate.arguments
