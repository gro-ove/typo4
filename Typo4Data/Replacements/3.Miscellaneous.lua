--[[
  Replaces “([SOMETHING])” by a specific character. For example, “(inf)” will be replaced by “∞”.
]]

return function (input)
  local val = input

  -- options
  regex.IgnoreCase = true; -- to override locally, use :ReplaceIgnoreCase() or :ReplaceConsiderCase()

  -- spaces
  val = regex:Replace(val, [[\(zws\)]], "​")
  val = regex:Replace(val, [[\((?:   | em| ем)\)]], " ")
  val = regex:Replace(val, [[\((?:  | en| ен)\)]], " ")
  val = regex:Replace(val, [[\(( nb| нб)\)]], " ")

  -- punctuation
  val = regex:Replace(val, [[\(pri\)|\('\)]], "′")
  val = regex:Replace(val, [[\(dpr\)|\(''\)]], "″")
  val = regex:Replace(val, [[\(tpr\)|\('''\)]], "‴")

  val = regex:Replace(val, [[\(ss|пар\)]], '§')
  val = regex:Replace(val, [[\(\*\)]], '•')
  val = regex:Replace(val, [[\( \)]], ' ')

  -- special symbols
  val = regex:Replace(val, [[\((?:cr|кр)\)]], "©")
  val = regex:Replace(val, [[\((?:rt|рт)\)]], "®")
  val = regex:Replace(val, [[\((?:ph|фо)\)]], "℗")
  val = regex:Replace(val, [[\((?:sm|см)\)]], "℠")
  val = regex:Replace(val, [[\((?:tm|тм)\)]], "™")
  val = regex:Replace(val, [[\((?:se|се)\)]], "§")
  val = regex:Replace(val, [[\((?:pa|па)\)]], "¶")
  val = regex:Replace(val, [[\(!:(?:¶|pa|па)\)]], "⁋")

  -- math
  val = regex:Replace(val, [[\([xх]\)]], '×')
  val = regex:Replace(val, [[\((?:-:-)\)]], '÷')
  val = regex:Replace(val, [[\((?:\+-)\)]], '±')
  val = regex:Replace(val, [[\((?:!=|=/=|=\=)\)]], '≠')
  val = regex:Replace(val, [[\((?:~=|=~)\)]], '≈')
  val = regex:Replace(val, [[\((?:inf|беск|инф)\)]], '∞')

  -- currencies
  val = regex:Replace(val, [[\((?:cen|цен)\)]], "¢")
  val = regex:Replace(val, [[\((?:dol|дол)\)]], "＄")
  val = regex:Replace(val, [[\((?:eur|евр)\)]], "€")
  val = regex:Replace(val, [[\((?:lir|лир)\)]], "₤")
  val = regex:Replace(val, [[\((?:pou|фун)\)]], "£")
  val = regex:Replace(val, [[\((?:rub|руб)\)]], "₽")

  return val
end
