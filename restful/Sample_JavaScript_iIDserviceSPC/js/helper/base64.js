
/**
 * Encode bytes array to base64 string
 * @param {*} bytesArray bytes array
 * @returns base64 string
 */
function bytesArrayToBase64( bytesArray ) {
    var binary = '';
    var bytes = new Uint8Array( bytesArray );
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode( bytes[ i ] );
    }
    return window.btoa( binary );
}


/**
 * Decode base64 string to bytes array
 * @param {*} base64 base64 string
 * @returns bytes array
 */
function base64ToBytesArray(base64) {
    var binary_string =  window.atob(base64);
    var len = binary_string.length;
    var bytesArray = new Uint8Array( len );
    for (var i = 0; i < len; i++)        {
        bytesArray[i] = binary_string.charCodeAt(i);
    }
    return bytesArray;
}


// // base64 to buffer async
// function base64ToBufferAsync(base64) {
//     var dataUrl = "data:application/octet-binary;base64," + base64;
    
//     fetch(dataUrl)
//         .then(res => res.arrayBuffer())
//         .then(buffer => {
//             console.log("base64 to buffer: " + new Uint8Array(buffer));
//         })
// }
  
// // buffer to base64 async
// function bufferToBase64Async( buffer ) {
//     var blob = new Blob([buffer], {type:'application/octet-binary'});    
//     console.log("buffer to blob:" + blob)
    
//     var fileReader = new FileReader();
//     fileReader.onload = function() {
//         var dataUrl = fileReader.result;
//         console.log("blob to dataUrl: " + dataUrl);
        
//         var base64 = dataUrl.substr(dataUrl.indexOf(',')+1)      
//         console.log("dataUrl to base64: " + base64);
//     };
//     fileReader.readAsDataURL(blob);
// }