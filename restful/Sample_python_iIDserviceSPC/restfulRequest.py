import requests
from sseclient import SSEClient

def PerformGetRequest(url):
    headers = { "ApiKey": "hL4bA4nB4yI0vI0fC8fH7eT6" }
    try:
        return requests.get(url, headers = headers)
    except Exception:
        print(" <Exception calling GET on " + url + ">")
        return None

def PerformPostRequest(url):
    headers = { "ApiKey": "hL4bA4nB4yI0vI0fC8fH7eT6" }
    try:
        return requests.post(url, headers = headers)
    except Exception:
        print(" <Exception calling POST on " + url + ">")
        return None

def TestReceiveUpdatesSSE(url, timeout):
    url = url + '?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&HeartbeatTimeoutMs=' + str(timeout)
    messages = SSEClient(url)
    if messages != None:
        msgCount = 0 # For DEMO purposes exit after 10 messages
        for msg in messages:
            # print(msg)
            msgCount+=1
            if (msgCount == 3):
                # For DEMO purposes, perform Spc-Request on 3rd message
                print("\t - POST SendSpcRequestASCII")
                answer = PerformPostRequest('http://localhost:19813/api/iidservice/spc/SendSpcRequestASCII?Request=~T')
                if answer != None:
                    if answer.status_code == 200:
                        print(answer.text)
                    else:
                        from Constants import __errorPrintHeaderStr
                        print(__errorPrintHeaderStr + str(answer.status_code))
            if (msgCount > 10):
                break 
            if msg.event == 'readerHeartbeat':
                if (msg.id == '-1'):
                    break
                else:
                    print("HEARTBEAT_" + msg.id + " - " + msg.data)
            elif msg.event == 'readerRawData':
                print(msg.id + " - " + msg.data)
