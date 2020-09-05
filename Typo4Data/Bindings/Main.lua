--[[
  Matches various Caps Lock+… hotkeys to various characters (or, if needed, words). For example, Caps Lock+X prints ×,
  or Caps Lock+1+4 prints ¼.

  One of inspiration sources (but far from compatible layout):
  http://ilyabirman.ru/projects/typography-layout/ 
]]

local base = {
  Oemtilde = "́",
  Oem7 = "’",

  D1 = "¹",
  D2 = "²",
  D3 = "³",
  D4 = "⁴",
  D5 = "‰",
  D6 = "√",
  D7 = "∧",
  D8 = "∞",
  D9 = "π",
  D0 = "∅",

  D1_D2 = "½",
  D1_D4 = "¼",
  D1_D5 = "⅕",
  D1_D6 = "⅙",
  D1_D8 = "⅛",
  D3_D4 = "¾",

  D3_D6 = "∛",
  D4_D6 = "∜",

  OemMinus = "—",
  Oemplus = "≠",
  OemQuestion = "…",

  A = "α",
  B = "β",
  C = "©",
  D = "°",
  E = "€",
  F = "φ",
  H = "₽",
  L = "λ",
  M = "−",
  P = "•",
  R = "®",
  T = "θ",
  U = "μ",
  X = "×",
  Y = "ѣ",
  
  OemQuotes = "’",
  Oemcomma = "«",
  OemPeriod = "»",

  Space = " ",
  Tab = " ",
  Back = "​",

  Left = "←",
  Right = "→",
  Up = "↑",
  Down = "↓",

  Left_Right = "↔",
  Down_Up = "↕",

  Left_Up = "↖",
  Right_Up = "↗",
  Down_Left = "↙",
  Down_Right = "↘",
}

local shift = {
  D1 = "¡",
  D5 = "‱",
  D7 = "¿",
  D9 = "☹",
  D0 = "☺",

  OemMinus = "–",
  Oemplus = "±",

  A = "Α",
  B = "Β",
  C = "¢",
  D = "Δ",
  E = "Σ",
  F = "Φ",
  P = "§",
  T = "Θ",
  U = "Μ",
  Y = "Ѣ",

  Oemcomma = "„",
  OemPeriod = "“",
  OemQuestion = "‽",

  Left = "⇐",
  Right = "⇒",
  Up = "⇑",
  Down = "⇓",

  Left_Right = "⇔",
  Down_Up = "⇕",

  Left_Up = "⇖",
  Right_Up = "⇗",
  Down_Left = "⇙",
  Down_Right = "⇘",
}

local alt = {
  D0 = "₀",
  D1 = "₁",
  D2 = "₂",
  D3 = "₃",
  D4 = "₄",
  D5 = "₅",

  D6 = "÷",
  D7 = "∨",

  Oemplus = "≈",

  W = "♀",
  M = "♂",
  P = "π",
  F = "£",
  X = "⋅",

  B = "⁃",
  OemPeriod = "…",

  Oemcomma = "“",
  OemPeriod = "”",
  
  Left = "☭",
  Right = "卐",
}

local ctrl = {
  OemQuestion = "÷",

  Left = "◀",
  Right = "▶",
  Up = "▲",
  Down = "▼",
  
  Oemcomma = "≤",
  OemPeriod = "≥",
}

-- change to true to print out actual keys combination in case symbol was not found
local debugMode = false

return function(keys, options)
  if not options.shift and not options.ctrl and not options.win and keys == 'Q' then
    return ''..math.random(9)..(options.alt and math.random(9)..math.random(9)..math.random(9) or '')
  end

  if not options.alt and not options.shift and not options.ctrl and not options.win then
    return base[keys]
  end

  if not options.alt and options.shift and not options.ctrl and not options.win then
    return shift[keys]
  end

  if options.alt and not options.shift and not options.ctrl and not options.win then
    return alt[keys]
  end
  if not options.alt and not options.shift and options.ctrl and not options.win then
    return ctrl[keys]
  end

  if debugMode then
    return keys
  else
    return nil
  end
end
