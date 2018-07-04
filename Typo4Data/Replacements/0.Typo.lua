--[[
  When selected/all/clipboard text is being replaced, this file specifies how to transform it. Write here anything you
  might need. This part is to improve text typographically with proper quotes, dashes and that.
]]

return function (input)
  local val = input
  local rus = regex:IsMatch(val, "[а-я]")

  -- options
  regex.IgnoreCase = true; -- to override locally, use :ReplaceIgnoreCase() or :ReplaceConsiderCase()

  -- reset
  val = regex:Replace(val, '[«»„“”]', '"')
  val = regex:Replace(val, '[–—]', '-')

  -- accent
  val = regex:Replace(val, [[((?<=[eyuioaаеиоуыэюя]|◌)[`])]], "́")

  -- punctuation
  val = regex:Replace(val, [[(?<!\.)\.\.\.(?!\.)]], "…")

  -- bullets
  val = regex:Replace(val, [[((?<=[\n\r])(\s*)\*(?=\s))|\((?:bu|бу)\)]], "$1•")
  val = regex:Replace(val, [[((?<=[\n\r])(\s*)>(?=\s))|\((?:tb|тб)\)]], "$1‣")
  val = regex:Replace(val, [[((?<=[\n\r])(\s*)o(?=\s))|\((?:wb|бб)\)]], "$1◦")
  val = regex:Replace(val, [[((?<=[\n\r])(\s*)-(?=\s))|\((?:hb|дб)\)]], "$1⁃")

  -- arrows
  val = regex:Replace(val, [[->]], '→')
  val = regex:Replace(val, [[<-]], '←')
  val = regex:Replace(val, [[=>]], '⇒')
  val = regex:Replace(val, [[<=]], '⇐')
  val = regex:Replace(val, [[<->]], '↔')

  -- math
  val = regex:Replace(val, [[\^0\b]], '°')
  val = regex:Replace(val, [[\^1\b]], '¹')
  val = regex:Replace(val, [[\^2\b]], '²')
  val = regex:Replace(val, [[\^3\b]], '³')

  val = regex:Replace(val, [[\b1/8\b]], '⅛')
  val = regex:Replace(val, [[\b1/4\b]], '¼')
  val = regex:Replace(val, [[\b1/3\b]], '⅓')
  val = regex:Replace(val, [[\b3/8\b]], '⅜')
  val = regex:Replace(val, [[\b1/2\b]], '½')
  val = regex:Replace(val, [[\b5/8\b]], '⅝')
  val = regex:Replace(val, [[\b2/3\b]], '⅔')
  val = regex:Replace(val, [[\b3/4\b]], '¾')
  val = regex:Replace(val, [[\b7/8\b]], '⅞')

  -- special symbols
  val = regex:Replace(val, [[\([cс]\)]], '©')
  val = regex:Replace(val, [[\([rр]\)]], '®')
  val = regex:Replace(val, [[\(tm|тм\)]], '™')

  -- dashes
  val = regex:Replace(val, [[([0-9])-([0-9])]], '$1–$2')
  val = regex:Replace(val, [[(^|\s)-(\s|$)]], '$1—$2')

  -- cool quotes
  if rus then
    val = regex:Replace(val, [["([a-zа-я0-9<>]([^"]|"[a-zа-я0-9<>][^"]*")*)"]], '«$1»')
    val = regex:Replace(val, [["([a-zа-я0-9<>]([^"]|"[a-zа-я0-9<>][^"]*")*)"]], '„$1“')
  else
    val = regex:Replace(val, [["(\S(?:[\s\S]*?\S)?)?"]], '“$1”')
    val = regex:Replace(val, [[([Ia-z])'(d|m|s|ll|ve|re)\b]], '$1’$2')
    val = regex:Replace(val, [[([a-z]s)'(?=\s|$)]], '$1’')
    val = regex:Replace(val, [[n't\b]], 'n’t')
  end

  return val
end
