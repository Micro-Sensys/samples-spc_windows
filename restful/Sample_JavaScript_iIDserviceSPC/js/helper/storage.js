

let myCollection = [];

function randomNumer() {
    return Math.floor(Math.random() * PRODUCTS.length);
}


function addTable(timestamp, eventID, tid, message) {

    const HEADER_NAMES = ["Timestamp added", "EventID", "TID", "Message"];
    
    let val = [timestamp, eventID, tid, message ];

    let found = myCollection.find(function(value) {
        return value[2] === tid
    });
    if (found == null){
        myCollection.push(
            val
        );
        // Re-print table with new values
        let myTableDiv = document.getElementById("myDynamicTable");
        myTableDiv.innerHTML = '';
        
        let table = document.createElement('TABLE');
        table.border = '1';
        
        let tableBody = document.createElement('TBODY');
        table.appendChild(tableBody);
        
        let tr_header = document.createElement('TR');
        tableBody.appendChild(tr_header);
        for (const element of HEADER_NAMES) {
            let th = document.createElement('TH');
            //th.width = '300px';
            th.appendChild(document.createTextNode(element));
            tr_header.appendChild(th);
        }

        for (const element of myCollection) {
            let tr = document.createElement('TR');
            tableBody.appendChild(tr);
            
            for (const subElement of element) {
                let td = document.createElement('TD');
                //td.width = '300px';
                td.appendChild(document.createTextNode(subElement));
                tr.appendChild(td);
            }
        }
        myTableDiv.appendChild(table);
    } else{
        //TODO already added --> paint blue!?
    }
    
}