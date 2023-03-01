from ConsoleFlow import PrintConnectionSettingsFunctions
from restfulRequest import PerformGetRequest, PerformPostRequest
from Constants import __errorPrintHeaderStr

'''
    print("1 - GET possible settings")
    print("2 - GET current settings")
    print("3 - POST new settings")
    print("# - Back")
'''

def ModifyConnectionSettings(hostname):
    while True:
        PrintConnectionSettingsFunctions()
        selValue = input() # read console input
        if len(selValue) > 1:
            selValue = selValue(0)
        
        if selValue == "1":
            print("\t1 - GET possible settings")
            answer = PerformGetRequest('http://' + hostname + ':19813/api/iidservice/spc/interface/PossibleSettings')
            if answer != None:
                if answer.status_code == 200:
                    print(answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "2":
            print("\t2 - GET current settings")
            answer = PerformGetRequest('http://' + hostname + ':19813/api/iidservice/spc/interface/CurrentSettings')
            if answer != None:
                if answer.status_code == 200:
                    print(answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "3":
            print("\t3 - POST new settings")
            print("PortType: ", end="")
            portType = input() # read console for PortType
            if portType != "4":
                print("PortName: ", end="")
                portName = input() # read console for PortName
            else:
                portName = ""
            answer = PerformPostRequest('http://' + hostname + ':19813/api/iidservice/spc/interface/CurrentSettings?&PortName='+portName+'&PortType='+portType)
            if answer != None:
                if answer.status_code == 200:
                    print("\t\t=> OK")
                else:
                    print("\t\t=> ERROR")
                    print(answer.text)
        elif selValue == "0":
            break