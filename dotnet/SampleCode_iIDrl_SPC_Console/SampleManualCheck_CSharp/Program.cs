/*
* SampleManualCheck_CSharp
*      SampleCode for iIDReaderLibrary.SpcInterfaceControl
*      Implemented in C#
*      Using "Start..." functions
*      
* This sample demonstrates how to call the SpcInterfaceControl functions that run the process in a separate new thread.
* This is only for demo purposes. For a Console application is not efficient to work in this way.
*/

using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using iIDReaderLibrary.Utils.Definitions;

string? m_LastId = null;

async System.Threading.Tasks.Task<bool> Console_ExecuteAndContinueAsync(SpcInterfaceControl _spcIntControl)
{
    //Main Console MENU
    Console.WriteLine("");
    Console.WriteLine("--------------------");
    Console.WriteLine(" Console MENU");
    Console.WriteLine("--------------------");
    Console.WriteLine("0 - Get Last HEARTBEAT");
    Console.WriteLine("1 - Get HEARTBEAT asynchronously");
    Console.WriteLine("2 - Get Last RawData");
    Console.WriteLine("3 - Get RawData asynchronously");
    Console.WriteLine("4 - Send READ request");
    Console.WriteLine("5 - Send WRITE request");
    Console.WriteLine("X - EXIT");
    Console.Write("Selection (confirm with ENTER): ");
    switch (Console.ReadLine())
    {
        case "0":
            Console.WriteLine("\tGet Last HEARTBEAT");
            var lastHb = _spcIntControl.Heartbeat;
            if (lastHb != null)
                ShowHeartbeat(lastHb);
            break;
        case "1":
            Console.WriteLine("\tGet HEARTBEAT asynchronously");
            var hbAsync = await _spcIntControl.GetHeartbeatAsync();
            if (hbAsync != null)
                ShowHeartbeat(hbAsync);
            break;
        case "2":
            Console.WriteLine("\tGet Last RawData");
            var lastRawData = _spcIntControl.DataReceived;
            if (lastRawData != null)
                DecodeReceivedText(lastRawData.Data);
            break;
        case "3":
            Console.WriteLine("\tGet RawData asynchronously");
            var rawDataAsync = await _spcIntControl.GetDataReceivedAsync();
            if (rawDataAsync != null)
                DecodeReceivedText(rawDataAsync.Data);
            break;
        case "4":
            Console.WriteLine("\tSend READ request");
            SendReadRequest(_spcIntControl);
            break;
        case "5":
            Console.WriteLine("\tSend WRITE request");
            SendWriteRequest(_spcIntControl);
            break;
        case "X":
        case "x":
            return false;
        default:
            break;
    }
    Thread.Sleep(500);
    return true;
}

void ShowHeartbeat(ReaderHeartbeat _heartbeat)
{
    Console.WriteLine(string.Format("Heartbeat {0}: {1}, {2}", _heartbeat.Timestamp.ToString("HH:mm:ss"), _heartbeat.ReaderID, _heartbeat.BatteryStatus));
}

void DecodeReceivedText(string _receivedText)
{
    // Remove <CR> if present
    _receivedText = _receivedText.TrimEnd(['\r']);
    Console.WriteLine(string.Format("Data received: {0}", _receivedText));
    //For this implementation, first char is an "Identifier"
    switch (_receivedText[0])
    {
        case 'T': //Transponder read. Decode TID and Data from received string
            if (_receivedText.Length >= 16) // Check if minimum length received
            {
                // Remove "T" Identifier
                _receivedText = _receivedText[1..];

                // Get TID (0 - 15)
                string firstData = _receivedText[..16];
                firstData = firstData.TrimEnd(['\0']);      // Remove not initialized data
                m_LastId = firstData;

                // Get Data (16 - 92)
                string secondData = "";
                if (_receivedText.Length > 16)
                {
                    secondData = _receivedText[16..];
                    secondData = secondData.TrimEnd(['\0']); // Remove not initialized data
                }

                Console.WriteLine("\t" + firstData + "\t" + secondData);
            }
            break;
        case 'R': //Result string
            if (_receivedText.StartsWith("RW"))
            {
                // Write result
            }
            if (_receivedText.StartsWith("RT"))
            {
                // Read Transponder error
            }
            break;
    }
}
void SendReadRequest(SpcInterfaceControl _spcIntControl)
{
    // Generate "SCAN" request and send to reader
    string command = "~T";          // SOH + Read-Identifier
    _spcIntControl.SendSpcRequest(command);
    Console.WriteLine(string.Format("\nSend READ Request: {0}", command));
}
void SendWriteRequest(SpcInterfaceControl _spcIntControl)
{
    if (string.IsNullOrEmpty(m_LastId))
        return;
    // Generate "WRITE" request and send to reader
    string command = "~W";              // SOH + Write-Identifier
    command += m_LastId;      // TID
    command += "ConsoleTest " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");    // Equipment-data to write
    _spcIntControl.SendSpcRequest(command);
    Console.WriteLine(string.Format("\nSend WRITE Request: {0}", command));
}

SpcInterfaceControl Console_InitializeSpcInterfaceControl()
{
    Console.WriteLine("== Select initialize parameters ==");
    //Get PortType
    int portType = Console_InitializePortType();
    string portName = "";
    switch (portType)
    {
        case PortTypeEnum.PortType_Serial:
        case PortTypeEnum.PortType_Bluetooth:
        case PortTypeEnum.PortType_BluetoothLE:
            //For Serial & bluetooth, PortName needed.
            portName = Console_InitializePortName();
            break;
    }
    //Initialize InterfaceCommunicationSettings class
    var readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, portName);

    //Parameters selected --> Initialize class instance
    Console.WriteLine("");
    SpcInterfaceControl result = new(readerPortSettings, "", "\r\n");
    Console.WriteLine(string.Format("Selected parameters: PortType: {0} | PortName: {1}", portType, portName));

    //Call initialize to open the communication port

    Console.Write("Initializing...");
    Console.WriteLine("");
    if (result.Initialize())
    {
        Console.WriteLine("\tInitialized");
        return result;
    }
    else
    {
        //Initialization failed: Terminate class instance & try again
        Console.WriteLine("\tInitialize failed");
        result.Terminate();
        return Console_InitializeSpcInterfaceControl();
    }
}

int Console_InitializePortType()
{
    Console.WriteLine("Port Type (0 = Serial, 2 = Bluetooth, 4 = USB, 5 = BluetoothLE)");
    Console.Write("Selection (confirm with ENTER): ");
    switch (Console.ReadLine())
    {
        case "0":
            Console.WriteLine("\tSerial selected");
            return PortTypeEnum.PortType_Serial;
        case "2":
            Console.WriteLine("\tBluetooth selected");
            return PortTypeEnum.PortType_Bluetooth;
        case "4":
            Console.WriteLine("\tUSB selected");
            return PortTypeEnum.PortType_USB;
        case "5":
            Console.WriteLine("\tBluetoothLE selected");
            return PortTypeEnum.PortType_BluetoothLE;
        default:
            Console.SetCursorPosition(0, Console.CursorTop - 2);
            return Console_InitializePortType();
    }
}
string Console_InitializePortName()
{
    int cursorTop = Console.CursorTop;
    List<string> portNamesList =
    [
        .. InterfaceCommunicationSettings.GetAvailablePortNames(),
        .. InterfaceCommunicationSettings.GetAvailableBlePairedDevices(),
    ];
    Console.WriteLine("Port Name:");
    for (int i = 0; i < portNamesList.Count; i++)
    {
        Console.WriteLine(string.Format("{0} - {1}", i, portNamesList[i]));
    }
    Console.Write("Selection (confirm with ENTER): ");
    if (int.TryParse(Console.ReadLine(), out int portNameIndex))
    {
        if (portNameIndex < portNamesList.Count)
        {
            Console.WriteLine(string.Format("\t{0} selected", portNamesList[portNameIndex]));
            return portNamesList[portNameIndex];
        }
    }

    //Selection failed
    Console.SetCursorPosition(0, cursorTop);
    return Console_InitializePortName();
}

Console.WriteLine(".NETCore Console");
Console.WriteLine("SampleThreads_C#");
Console.WriteLine("--------------------");
Console.WriteLine("Library Version: " + SpcInterfaceControl.LibraryVersion);

//Get SpcInterfaceControl instance
SpcInterfaceControl spcIntControl = Console_InitializeSpcInterfaceControl();
if (spcIntControl != null)
{
    //SpcInterfaceControl is initialized
    Console.WriteLine("");
    Console.Write("Waiting for Heartbeat...");
    var heartbeat = await spcIntControl.GetHeartbeatAsync();
    if (heartbeat != null)
    {
        Console.WriteLine("");
        Console.WriteLine("Detected Reader:");
        ShowHeartbeat(heartbeat);
    }

    //Reader info obtained --> execute functions using menu
    Console.WriteLine("");
    while (await Console_ExecuteAndContinueAsync(spcIntControl))
    {
        Thread.Sleep(500);
    }

    spcIntControl.Terminate();
    Console.WriteLine("");
    Console.Write("EXITING in 5");
    Thread.Sleep(1000);
    Console.Write(", 4");
    Thread.Sleep(1000);
    Console.Write(", 3");
    Thread.Sleep(1000);
    Console.Write(", 2");
    Thread.Sleep(1000);
    Console.Write(", 1");
    Thread.Sleep(1000);
}