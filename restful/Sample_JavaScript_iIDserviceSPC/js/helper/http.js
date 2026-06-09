
function createXMLHttpRequest(conn_info) {
    var xhttp = null;
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
            xhttp = false;
        }
    }
    return xhttp;
}