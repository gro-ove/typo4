"edge-aware";
"noinput";

return function (_, callback){
  callback(null, Math.random() * 100 | 0);
};
