// === CONSTANTS
const API_KEY_NAME = "ApiKey";
const API_KEY_KEY  = "hL4bA4nB4yI0vI0fC8fH7eT6";

const PROTOCOL   = "http";
const IP_ADDRESS = "localhost";
const PORT       = "19813";

function GETversion() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/Version")
}

/* === MAIN FUNCTIONS === */
function GETisInitialized() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/IsInitialized")
}
function POSTinitialize() {
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/Initialize?&UseMssProtocol=false")
}
function POSTterminate() {
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/Terminate")
}
function GETlastHeartbeat() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/LastHeartbeat")
}
function POSTsendSpcRequest() {
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/SendSpcRequestASCII?Request=~T");
}

/* === MAIN SSE FUNCTIONS === */
function SSEstartReceiveUpdates() {
    let timeout      = parseInt(document.getElementById("timeout").value);
    executeStartSSE(onOpenMain, onSpcDataMain, onSpcDataMain, PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/ReceiveUpdates?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&HeartbeatTimeoutMs="+ timeout);
}


/* === DEMO search list === */
function SSEstartDemoSearchList() {
    executeStartSSE(onOpenDemoSearch, onHeartbeatDemoSearch, onRawDataDemoSearch, PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/ReceiveUpdates?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&HeartbeatTimeoutMs=10000");
}

/* === INTERFACE FUNCTIONS === */
function GETpossibleSettings() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/interface/PossibleSettings");
}
function GETcurrentSettings() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/interface/CurrentSettings");
}
function POSTnewConnectionSettings() {
    let PortPath      = encodeURIComponent(String(document.getElementById("portPath").value));
    let PortType      = parseInt(document.getElementById("portType").value);
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/spc/interface/CurrentSettings?&PortName="+ PortPath +'&PortType='+ PortType);
}



/* === HELPER FUNCTIONS === */
function executeApiCall(method, apiPath) {
    let CONN_INFO = new Object();
    CONN_INFO.METHOD = method;
    CONN_INFO.API    = apiPath;
    CONN_INFO.ASYNC  = true;

    let xhttp = createXMLHttpRequest(CONN_INFO);
    if (!xhttp) {
        alert("XMLHttpRequest not available");
        return;
    }
    xhttp.onreadystatechange = function () {
        if (xhttp.readyState === 4) { //Request ended
            try {
                if (xhttp.responseText.length > 0){
                    let msg = JSON.parse(xhttp.responseText);
                    console.log(msg);
                    if (document.getElementById("CodeResult") != null)
                        document.getElementById("CodeResult").innerHTML = '<b>Statuscode:&nbsp;</b>' + xhttp.status;
                    if (document.getElementById("MsgResult") != null)
                        document.getElementById("MsgResult").textContent = JSON.stringify(msg, undefined, 2);
                }
            } catch (error) {
                console.error("ERROR: JSON parse");
                document.getElementById("CodeResult").innerHTML = '<b>Statuscode:&nbsp;</b> ERROR';
            }
            stopTimer();
        }
    };
    xhttp.onerror = function () {
        stopTimer();
        console.error('Request failed.');
    };
    xhttp.ontimeout = function () {
        stopTimer();
        console.error('Request timeout.', xhttp.responseURL);
    };

    let pos = document.getElementById("ResponseTime");
    if (pos != null){
        startTimer(pos);
    }

    xhttp.send();
}

function createXMLHttpRequest(conn_info) {
    let xhttp = null;
    if (window.XMLHttpRequest) {
        try {
            xhttp = new XMLHttpRequest();
            xhttp.open(conn_info.METHOD, conn_info.API, conn_info.ASYNC);
            xhttp.setRequestHeader('Access-Control-Allow-Origin', '*');
            xhttp.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhttp.setRequestHeader(API_KEY_NAME, API_KEY_KEY);
            xhttp.timeout = 10000; // 10 seconds
        } catch (e) {
            console.log("Error XMLHttpRequest");
            xhttp = null;
        }
    }
    return xhttp;
}

let sseSourceEvent = null;
function executeStartSSE(onOpen, onHeartbeat, onRawData, apiPath) {

    if (sseSourceEvent != null) {
        return;
    }

    if (window.EventSource) {

        sseSourceEvent = new EventSource(apiPath);

        sseSourceEvent.addEventListener('open', onOpen, false);
        sseSourceEvent.addEventListener('error', OnError, false);
        sseSourceEvent.addEventListener('message', OnMessage, false);
        sseSourceEvent.addEventListener('readerHeartbeat', onHeartbeat, false);
        sseSourceEvent.addEventListener('readerRawData', onRawData, false);
    }
    else {
        alert("SSE not possible :(");
    }

    function OnError(e) {
        if (e.readyState == EventSource.CLOSED) {
            console.log("Connection was closed.");
        } else {
            console.error("EventSource ERROR");
            console.log(e);
        }
    }
    
    function OnMessage(e) {
        console.log(e);
    }
};

function SSEstop() {
    SSEclose();
};

window.onclose = function () {
    console.log("EventSource close");
    SSEclose();
};

function SSEclose() {
    if (sseSourceEvent != null) {
        sseSourceEvent.close();
        if (document.getElementById("CodeResult") != null)
            document.getElementById("CodeResult").innerHTML = '<b>SSE closed</b>';

        setTimeout(() => {
            sseSourceEvent = null;
        }, 500);
    }
}


function onOpenMain(e) {
    document.getElementById("MsgResult").textContent = '';
    document.getElementById("ResponseTime").innerHTML = '';
    document.getElementById("CodeResult").innerHTML = '<b>SSE open. Last received:</b>';
    document.getElementById("outputScan").innerHTML = '';
    console.log("EventSource Connection established");
}
function onSpcDataMain(e) {
    try {
        console.log(e);
        let msg = JSON.parse(e.data);
        writeJsonToOutputScreen(msg);
    } catch (error) {
        console.log("Error parse JSON")
    }
}
function writeJsonToOutputScreen(msg) {
    document.getElementById("MsgResult").textContent = JSON.stringify(msg, undefined, 2);
    document.getElementById("outputScan").insertAdjacentHTML("afterbegin", "<p>########################################################################</p>");
    document.getElementById("outputScan").insertAdjacentHTML("afterbegin", "<p>" + JSON.stringify(msg, undefined, 2) + "</p>");
}

function onOpenDemoSearch(e) {
    console.log("EventSource Connection established");
}
function onHeartbeatDemoSearch(e) {
    if (e.lastEventId >= 0) {
        changeLED(false);
        try {
            let heartbeat = JSON.parse(e.data);
            let heartString = "ReaderID: " + heartbeat.ReaderID + " - Battery: " + heartbeat.Battery + " - LastUpdate: " + heartbeat.LastUpdate;
            document.getElementById("readerID-name").innerHTML = heartString;
        } catch (error) {
            console.error(error);
        }
    } else {
        changeLED(true);
        document.getElementById("readerID-name").innerHTML = "";
        console.log("Connection lost");
    }

}
function onRawDataDemoSearch(e) {
    decodeReceivedRawData(e.data, e.lastEventId);
}
function changeLED(connectionLost) {
    if (connectionLost) {
        document.getElementById("ledRedDiv").style.display = "block";
        document.getElementById("ledGreenDiv").style.display = "none";
    } else {
        document.getElementById("ledRedDiv").style.display = "none";
        document.getElementById("ledGreenDiv").style.display = "block";
    }
}
function decodeReceivedRawData(receivedText, eventID)
{
    let dataJson = JSON.parse(receivedText)
    // Remove <CR> if present
    let dataText = dataJson.data;
    dataText = dataText.replace(/\r/g, '');
    //For this implementation, first char is an "Identifier"
    switch (dataText[0])
    {
        case 'T': //Transponder read. Decode TID and Data from received string
            if (dataText.length >= 16) // Check if minimum length received
            {
                document.getElementById("resultInfo").innerHTML = "RT: success";

                // Remove "T" Identifier
                dataText = dataText.substring(1);

                // Get Personal-Nr., Ausweis-Nr. oder Equi-Nr. (0 - 15)
                let firstData = dataText.substring(0, 16);
                firstData = firstData.replace(/\0/g, '');      // Remove not initialized data

                // Get Equi-Daten (16 - 92)
                let secondData = "";
                if (dataText.length > 16)
                {
                    secondData = dataText.substring(16);
                    secondData = secondData.replace(/\0/g, ''); // Remove not initialized data
                }

                //add information to table
                addTable(dataJson.Timestamp, eventID, firstData, secondData);

                //set tagid to search field if selected
                let x = document.activeElement.id;
                if (x.length > 0 && firstData.length == 16) {
                    document.getElementById(x).value = firstData;
                    searchFunction();
                }
            }
            break;
        case 'R': //Result string
            let rawString = "";

            if (dataText.startsWith("RW"))
            {
                // Write result
                switch (dataText)
                {
                    case "RW00": // Result OK
                        rawString = "RW: success";
                        break;
                    case "RW24": // Transponder not found or error writing (Error)
                        rawString = "RW: error";
                        break;
                }
            }
            if (dataText.startsWith("RT"))
            {
                if (dataText.substring(2, 2) == "00")
                {
                    //Result OK
                    rawString = "RT: true";
                }
                else
                {
                    //Error
                    //Example "RT24" --> Transponder not found or error reading
                    rawString = "RT: error";
                }
            }

            document.getElementById("resultInfo").innerHTML = rawString;

            break;
    }
}
function searchFunction() {
    // Declare variables
    let input, filter, table, tr, td, i, txtValue;
    input = document.getElementById("searchTermID");
    filter = input.value.toUpperCase();
    table = document.getElementById("myDynamicTable");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[2];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}