
/**
 * Convert a hex string to a byte array
 *
 * @param hex the string of hex.
 * @returns byte array from hex string.
 */
function hexStringToBytesArray(hex) {
    for (var bytes = [], c = 0; c < hex.length; c += 2)
    bytes.push(parseInt(hex.substr(c, 2), 16));
    return bytes;
}


/**
 * Convert a byte array to a hex string
 *
 * @param bytes bytes array
 * @returns hex string from bytes array
 */
function bytesArrayToHexString(bytes) {
    for (var hex = [], i = 0; i < bytes.length; i++) {
        var current = bytes[i] < 0 ? bytes[i] + 256 : bytes[i];
        hex.push((current >>> 4).toString(16));
        hex.push((current & 0xF).toString(16));
    }
    return hex.join("");
}

/**
 * string to bytes array
 * @param {string} str 
 */
function string2Bin(str) {
    var result = [];
    for (var i = 0; i < str.length; i++) {
        result.push(str.charCodeAt(i));
    }
    return result;
}
  
/**
 * bytes array to string
 * @param {bytes} array 
 */
function bin2String(array) {
    return String.fromCharCode.apply(String, array);
}

/**
 * Check and Convert Hexadecimal string to base64 string
 * @param {string} strVal hexadecimal
 * @returns base64 string
 */
function HexaStringToBase64(strVal) {

    //check only 0-9 and A-F
    var regex = /^[0-9a-fA-F ]*$/;;
    if (!regex.test(strVal)) {
        return null;
    }
    var bytesArrayHexa = hexStringToBytesArray(strVal);
    var base64Hexa = bytesArrayToBase64(bytesArrayHexa);

    return base64Hexa;
}