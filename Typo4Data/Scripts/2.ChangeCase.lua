if regex:IsMatch(input, '[A-ZА-Я]') then
    return unicode:ToLower(input)
else
    return unicode:ToUpper(input)
end
