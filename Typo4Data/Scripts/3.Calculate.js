"edge-aware";

return function (input, callback){
  callback(null, eval("(" + input.replace(/\b([a-z]+\d*)/g, 'Math.$1') + ")"));
};
