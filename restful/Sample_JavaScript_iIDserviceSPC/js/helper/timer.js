
let myFunc      = null;

let timeStart = 0;
let timeStop  = 0;

function startTimer(funcPtr) {
    myFunc = funcPtr;
    myFunc.innerHTML = '<p>Response time: ...</p>';
    timeStart = performance.now();
}

function stopTimer() {
    timeStop = performance.now();

    let myTime = parseInt(timeStop - timeStart);
    let myNum = ("000000" + myTime).slice(-6);
    
    if (myFunc != null){
        myFunc.innerHTML = '<p>Response time: ' + myNum + ' ms</p>';
    }

    myFunc = null;
}