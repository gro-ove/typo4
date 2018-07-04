--[[
  Various greek letters just in case. For example, to enter “µ”, select “(u)” and press Caps Lock.
]]

local greek = {
  AL = "Α",
  al = "α",
  BE = "Β",
  be = "β",
  CH = "Χ",
  ch = "χ",
  cw = "ƈ",
  CW = "Ƈ",
  DE = "Δ",
  de = "δ",
  EP = "Ε",
  ep = "ε",
  ET = "Η",
  et = "η",
  fw = "ƒ",
  FW = "Ƒ",
  GA = "Γ",
  ga = "γ",
  IO = "Ι",
  io = "ι",
  KA = "Κ",
  ka = "κ",
  LA = "Λ",
  la = "λ",
  MU = "Μ",
  mu = "μ",
  NU = "Ν",
  nu = "ν",
  OM = "Ο",
  om = "ο",
  OM = "Ω",
  ow = "ω",
  PH = "Φ",
  ph = "φ",
  PI = "Π",
  pi = "π",
  PS = "Ψ",
  ps = "ψ",
  RH = "Ρ",
  rh = "ρ",
  sc = "ς",
  si = "σ",
  SI = "Σ",
  TA = "Τ",
  ta = "τ",
  TH = "Θ",
  th = "θ",
  UP = "Υ",
  up = "υ",
  XI = "Ξ",
  xi = "ξ",
  ZE = "Ζ",
  ze = "ζ",
}

return function (input)
  local val = input

  -- greek
  val = regex:ReplaceConsiderCase(val, [[\(q\)]], "σ")
  val = regex:ReplaceConsiderCase(val, [[\(v\)]], "υ")
  val = regex:ReplaceConsiderCase(val, [[\(w\)]], "ω")
  val = regex:ReplaceConsiderCase(val, [[\(f\)]], "ƒ")
  val = regex:ReplaceConsiderCase(val, [[\(u\)]], "µ")

  val = regex:ReplaceCallback(val, [[\(\w\w\)]], function (_, n)
    if greek[n] then return greek[n] end
  end)

  return val
end
