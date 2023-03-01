def PrintMainConsoleFunctions():
    print(" = MAIN = ")
    print("Select one option + ENTER:")
    print("1 - ConnectionSettings")
    print("2 - SPC functions (+ auto connect)")
    print("H - Change RESTful destination")
    print("V - Version information")
    print("0 - <EXIT>")
    print("---------------------------------------")

def PrintConnectionSettingsFunctions():
    print(" = ConnectionSettings =")
    print("Select one option + ENTER:")
    print("1 - GET possible settings")
    print("2 - GET current settings")
    print("3 - POST new settings")
    print("0 - <BACK>")
    print("---------------------------------------")

def PrintDocFunctions():
    print(" = SPCfunctions =")
    print("Select one option + ENTER:")
    print("1 - GET IsInitialized")
    print("2 - GET LastHeartbeat") # TODO!?
    print("3 - POST SendSpcRequestASCII")
    print("4 - SSE ReceiveUpdates")
    print("0 - <BACK> (+ auto disconnect)")
    print("---------------------------------------")