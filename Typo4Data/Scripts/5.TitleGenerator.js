var noun = 'People,History,Way,Art,World,Information,Map,Family,Government,Health,System,Computer,Meat,Year,Thanks,Music,Person,Reading,Method,Data,Food,Understanding,Theory,Law,Bird,Literature,Problem,Software,Control,Knowledge,Power,Ability,Economics,Love,Internet,Television,Science,Library,Nature,Fact,Product,Idea,Temperature,Investment,Area,Society,Activity,Story,Industry,Media,Thing,Oven,Community,Definition,Safety,Quality,Development,Language,Management,Player,Variety,Video,Week,Security,Country,Exam,Movie,Organization,Equipment,Physics,Analysis,Policy,Series,Thought,Basis,Boyfriend,Direction,Strategy,Technology,Army,Camera,Freedom,Paper,Environment,Child,Instance,Month,Truth,Marketing,University,Writing,Article,Department,Difference,Goal,News,Audience,Fishing,Growth,Income,Marriage,User,Combination,Failure,Meaning,Medicine,Philosophy,Teacher,Communication,Night,Chemistry,Disease,Disk,Energy,Nation,Road,Role,Soup,Advertising,Location,Success,Addition,Apartment,Education,Math,Moment,Painting,Kill,Sensitive,Tap,Win,Attack,Claim,Constant,Drag,Drink,Guess,Minor,Pull,Raw,Soft,Solid,Wear,Weird,Wonder,Annual,Count,Dead,Doubt,Feed,Forever,Impress,Nobody,Repeat,Round,Sing,Slide,Strip,Whereas,Wish,Combine,Command,Dig,Divide,Equivalent,Hang,Hunt,Initial,March,Mention,Smell,Spiritual,Survey,Tie,Adult,Brief,Crazy,Escape,Gather,Hate,Prior,Repair,Rough,Sad,Scratch,Sick,Strike,Employ,External,Hurt,Illegal,Laugh,Lay,Mobile,Nasty,Ordinary,Respond,Royal,Senior,Split,Strain,Struggle,Swim,Train,Upper,Wash,Yellow,Convert,Crash,Dependent,Fold,Funny,Grab,Hide,Miss,Permit,Quote,Recover,Resolve,Roll,Sink,Slip,Spare,Suspect,Sweet,Swing,Twist,Upstairs,Usual,Abroad,Brave,Calm,Concentrate,Estimate,Grand,Male,Mine,Prompt,Quiet,Refuse,Regret,Reveal,Rush,Shake,Shift,Shine,Steal,Suck,Surround,Anybody,Bear,Brilliant,Dare,Dear,Delay,Drunk,Female,Hurry,Inevitable,Invite,Kiss,Neat,Pop,Punch,Quit,Reply,Representative,Resist,Rip,Rub,Silly,Smile,Spell,Stretch,Stupid,Tear,Temporary,Tomorrow,Wake,Wrap,Yesterday'.split(',');
var adjective = 'Lost,Only,Last,First,Third,Sacred,Bold,Lovely,Final,Missing,Shadowy,Seventh,Dwindling,Missing,Absent,Vacant,Cold,Hot,Burning,Forgotten,Weeping,Dying,Lonely,Silent,Laughing,Whispering,Forgotten,Smooth,Silken,Rough,Frozen,Wild,Trembling,Fallen,Ragged,Broken,Cracked,Splintered,Slithering,Silky,Wet,Magnificent,Luscious,Swollen,Erect,Bare,Naked,Stripped,Captured,Stolen,Sucking,Licking,Growing,Kissing,Green,Red,Blue,Azure,Rising,Falling,Elemental,Bound,Prized,Obsessed,Unwilling,Hard,Eager,Ravaged,Sleeping,Wanton,Professional,Willing,Devoted,Misty,Lost,Only,Last,First,Final,Missing,Shadowy,Seventh,Dark,Darkest,Silver,Silvery,Living,Black,White,Hidden,Entwined,Invisible,Next,Seventh,Red,Green,Blue,Purple,Grey,Bloody,Emerald,Diamond,Frozen,Sharp,Delicious,Dangerous,Deep,Twinkling,Dwindling,Missing,Absent,Vacant,Cold,Hot,Burning,Forgotten,Some,No,All,Every,Each,Which,What,Playful,Silent,Weeping,Dying,Lonely,Silent,Laughing,Whispering,Forgotten,Smooth,Silken,Rough,Frozen,Wild,Trembling,Fallen,Ragged,Broken,Cracked,Splintered'.split(',');
  
var f = [
    function () { 
        var a = Math.floor(Math.random() * noun.length), b = Math.floor(Math.random() * adjective.length); 
        return adjective[b] + " " + noun[a] 
    }, function () { 
        var c = Math.floor(Math.random() * adjective.length), d = Math.floor(Math.random() * noun.length); 
        return "The " + adjective[c] + " " + noun[d] 
    }, function () { 
        var e = Math.floor(Math.random() * noun.length), f = Math.floor(Math.random() * noun.length); 
        return noun[f] + " of " + noun[e] 
    }, function () { 
        var g = Math.floor(Math.random() * noun.length), h = Math.floor(Math.random() * noun.length); 
        return "The " + noun[g] + "'s " + noun[h] 
    }, function () { 
        var i = Math.floor(Math.random() * noun.length), j = Math.floor(Math.random() * noun.length); 
        return "The " + noun[i] + " of the " + noun[j] 
    }, function () { 
        var k = Math.floor(Math.random() * noun.length), l = Math.floor(Math.random() * noun.length); 
        return noun[k] + " in the " + noun[l] 
    }
];

console.log(f[Math.random() * f.length | 0]());