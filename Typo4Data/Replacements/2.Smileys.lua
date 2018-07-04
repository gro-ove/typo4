--[[
  From time to time I like to use text smileys, like ¯\_(ツ)_/¯, by selecting “*whynot*” and pressing Caps Lock.
]]

local smileys = {
  bird = "(°<°)",
  blame = "＜(。_。)＞",
  calm = "┐(￣ー￣)┌",
  cat = "≡^.ㅅ.^≡",
  comeon = "ヽ༼ ಠ益ಠ ༽ﾉ",
  cute = "(✿◠‿◠)",
  dunno = "ɿ(｡･ɜ･)ɾ",
  facepalm = "(–…ლ)",
  fingerguns = "(☞ﾟヮﾟ)☞",
  flipthetable = "(╯°□°）╯︵ ┻━┻",
  happy = "｡◕‿◕｡",
  hi = "ヽ(°◇° )ノ",
  hithere = "ヾ(。◕ฺ∀◕ฺ)ノ",
  hug = "＼(^o^)／",
  lenny = "( ͡° ͜ʖ ͡°)",
  morning = "٩(ˊᗜˋ*)و",
  murr = "ლ^◕ω◕^ლ",
  ohyeah = "( •_•)\n( •_•) ⌐■-■\n(⌐■_■)",
  scare = "(>﹏<)",
  shock = "ಠ╭╮ಠ",
  smug = "<(￣︶￣)>",
  touch = "ԅ(≖‿≖ԅ)",
  what = "ಠ_ಠ",
  why = "щ(ºДºщ)",
  whynot = "¯\\_(ツ)_/¯",
  yes = "( ･ㅂ･)و ̑̑"
}

return function (input)
  local val = input

  -- val = regex:Replace(val, [[:3]], ":​3") -- zero-width space thing for skype
  val = regex:ReplaceCallback(val, [[\*(\w+)\*]], function (_, n)
    if smileys[n] then return smileys[n] end
    return _
  end)

  return val
end
