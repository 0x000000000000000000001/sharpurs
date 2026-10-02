module Naming.Records where

type Fields =
  { "a\"b" :: Int
  , "path\\part" :: Int
  , "line\nbreak" :: Int
  , "tab\tkey" :: Int
  , "\x0" :: Int
  , "\x1\x2\x3" :: Int
  , "\x85" :: Int
  , "\x2028\x2029" :: Int
  , "💡" :: Int
  , "\xd800" :: Int
  , "\xdc00" :: Int
  , "\\ud800" :: Int
  }

original :: Fields
original =
  { "a\"b": 1
  , "path\\part": 2
  , "line\nbreak": 3
  , "tab\tkey": 4
  , "\x0": 5
  , "\x1\x2\x3": 6
  , "\x85": 7
  , "\x2028\x2029": 8
  , "💡": 9
  , "\xd800": 10
  , "\xdc00": 11
  , "\\ud800": 12
  }

changed :: Fields
changed = original
  { "a\"b" = 101
  , "path\\part" = 102
  , "line\nbreak" = 103
  , "tab\tkey" = 104
  , "\x0" = 105
  , "\x1\x2\x3" = 106
  , "\x85" = 107
  , "\x2028\x2029" = 108
  , "💡" = 109
  , "\xd800" = 110
  , "\xdc00" = 111
  , "\\ud800" = 112
  }

accessors :: Fields -> Array Int
accessors record =
  [ record."a\"b"
  , record."path\\part"
  , record."line\nbreak"
  , record."tab\tkey"
  , record."\x0"
  , record."\x1\x2\x3"
  , record."\x85"
  , record."\x2028\x2029"
  , record."💡"
  , record."\xd800"
  , record."\xdc00"
  , record."\\ud800"
  ]

patterns :: Fields -> Array Int
patterns record = case record of
  { "a\"b": a, "path\\part": b, "line\nbreak": c, "tab\tkey": d
  , "\x0": e, "\x1\x2\x3": f, "\x85": g, "\x2028\x2029": h
  , "💡": i, "\xd800": j, "\xdc00": k, "\\ud800": l
  } -> [ a, b, c, d, e, f, g, h, i, j, k, l ]
