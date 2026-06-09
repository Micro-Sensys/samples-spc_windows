from ConsoleFlow import PrintMainConsoleFunctions, PrintDocFunctions
from connectionSettingsFunctions import ModifyConnectionSettings
from restfulRequest import PerformGetRequest, PerformPostRequest, TestReceiveUpdatesSSE
from Constants import __errorPrintHeaderStr, __hostname

'''
Little Console Application without GUI
'''
if __name__ == "__main__":
    print("== Sample code for iIDservice ==")
    print("Console application on Python")
    print("--> iIDservice running on: " + __hostname)

    while True:
        PrintMainConsoleFunctions()
        selValue = input() # read console input
        if len(selValue) > 1:
            selValue = selValue(0)
        
        if (selValue == "1"):
            # ConnectionSettings
            print ("\t1 - ConnectionSettings")
            ModifyConnectionSettings(__hostname)
        elif selValue == "2":
            # DOC Functions
            print ("\t2 - SPC functions")
            # Get is connected DOC
            print("Get connection state...")
            answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/spc/IsInitialized')
            if answer != None:
                if answer.status_code == 200:
                    print("Initialized: " + answer.text)
                    if answer.text == "false":
                        # CONNECT SPC
                        print("Initializing...")
                        # answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/spc/Initialize?&SpcDataPrefix=:&SpcDataSufix=@&UseMssProtocol=false')
                        answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/spc/Initialize?&UseMssProtocol=false')
                        if answer != None:
                            if answer.status_code == 200:
                                print(answer.text)
                                if answer.text == "false":
                                    continue
                            else:
                                print(__errorPrintHeaderStr + str(answer.status_code))
                                continue
                        else:
                            continue
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
                    continue
            else:
                continue
            
            while True:
                PrintDocFunctions()
                '''
                    print(" = SPCfunctions =")
                    print("Select one option + ENTER:")
                    print("1 - GET IsInitialized")
                    print("2 - GET LastHeartbeat") # TODO!?
                    print("3 - POST SendSpcRequestASCII")
                    print("4 - SSE ReceiveUpdates")
                    print("0 - <BACK> (+ auto disconnect)")
                '''
                selValue = input() # read console input2
                if len(selValue) > 1:
                    continue
                    # selValue = selValue(0)
                
                if selValue == "1":
                    print("\t1 - GET IsInitialized")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/spc/IsInitialized')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "2":
                    print("\t2 - GET LastHeartbeat")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/spc/LastHeartbeat')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "3":
                    print("\t3 - POST SendSpcRequestASCII")
                    answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/spc/SendSpcRequestASCII?Request=~T')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "4":
                    print("\t4 - SSE ReceiveUpdates")
                    answer = TestReceiveUpdatesSSE('http://' + __hostname + ':19813/api/iidservice/spc/ReceiveUpdates', 3000)
                    if answer != None:
                        if answer.status_code == 200:
                            print("\t\t=> OK")
                        else:
                            print("\t\t=> ERROR")
                            print(answer.text)
                elif selValue == "0":
                    break
            # DISCONNECT SPC
            print("Terminating...")
            answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/spc/Terminate')
            if answer != None:
                if answer.status_code == 200:
                    print("Disconnect: " + answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "h" or selValue == "H":
            print("\tH - Change RESTful destination")
            print("New destination: ", end="")
            __hostname = input() # read console
            print("--> iIDservice running on: " + __hostname)
        elif selValue == "v" or selValue == "V":
            print ("\tV - Version information")
            answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/Version')
            if answer != None:
                if answer.status_code == 200:
                    print(answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "0":
            break
