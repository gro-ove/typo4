result = ''
table = {
  { "q", "й" }, { "w", "ц" }, { "e", "у" }, { "r", "к" }, { "t", "е" }, { "y", "н" }, { "u", "г" }, 
  { "i", "ш" }, { "o", "щ" }, { "p", "з" }, { "[", "х" }, { "]", "ъ" }, { "a", "ф" }, { "s", "ы" }, 
  { "d", "в" }, { "f", "а" }, { "g", "п" }, { "h", "р" }, { "j", "о" }, { "k", "л" }, { "l", "д" }, 
  { ";", "ж" }, { "'", "э" }, { "z", "я" }, { "x", "ч" }, { "c", "с" }, { "v", "м" }, { "b", "и" }, 
  { "n", "т" }, { "m", "ь" }, { ",", "б" }, { ".", "ю" }, { "/", "." }
}

for c in input:gmatch'.' do
  l = unicode:ToLower(c)
  w = l == c

  for i, p in ipairs(table) do
    if l == p[1] then l = p[2] end
  end

  if not w then l = unicode:ToUpper(l) end
  result = result .. l
end

return result
