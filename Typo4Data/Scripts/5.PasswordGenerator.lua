-- "noinput"

local char = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
math.randomseed(os.time())
local ret = ''
for i = 1, 8 do
  local j = math.random(1, #char)
  ret = ret .. char:sub(j, j)
end
return ret
