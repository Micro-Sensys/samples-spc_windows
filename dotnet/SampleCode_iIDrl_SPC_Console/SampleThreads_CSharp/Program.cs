/*
* SampleThreads_CSharp
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

object m_Lock = new();
string? m_LastId = null;

void SendReadRequest(SpcInterfaceControl _spcIntControl)
{
    // Generate "SCAN" request and send to reader
    string command = "~T";          // SOH + Read-Identifier
    _spcIntControl.SendSpcRequest(command);
    Console.WriteLine(string.Format("\nSend READ Request: {0}", command));
    AddConsoleLines();
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
    AddConsoleLines();
}

void AddConsoleLines()
{
    lock (m_Lock)
    {
        Console.WriteLine("");
        Console.WriteLine("Console running... (press R to send read request, or W to send write request, or X to exit)");

        Console.WriteLine("");
        Console.WriteLine(""); //HEARTBEAT
        Console.WriteLine(""); //DATA
        Console.WriteLine(""); //DATA
    }
}

bool initializeCompleted = false;
SpcInterfaceControl Console_InitializeSpcInterfaceControl()
{
    Console.WriteLine("== Select initialize parameters ==");
    //Get PortType
    int portType = Console_InitializePortType();
    string portName = "";
    switch (portType)
    {
        case 0:
        case 2:
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
    result.InitializeCompleted += SpcInterfaceControl_InitializeCompleted;
    result.StartInitialize();
    Console.Write("Initializing...");
    //For demo purposes, just wait blocking execution until "Initialize" process is completed (notified using "InitializeCompleted" event)
    while (!initializeCompleted) //Alternative, call "IsInitializing"
    {
        Thread.Sleep(100);
        Console.Write(".");
    }
    Console.WriteLine("");
    if (result.IsInitialized)
    {
        Console.WriteLine("\tInitialized");
        result.RawDataReceived += SpcInterface_RawDataReceived;
        result.ReaderHeartbeatReceived += SpcInterface_ReaderHeartbeatReceived;
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

void SpcInterfaceControl_InitializeCompleted(object _sender, bool _portOpen)
{
    // using "_portOpen" the result of the operation can be checked
    initializeCompleted = true;
}

void SpcInterface_ReaderHeartbeatReceived(object _sender, ReaderHeartbeat _heartbeat)
{
    //Event raised when Heartbeat received from Reader
    lock (m_Lock)
    {
        Console.SetCursorPosition(0, Console.CursorTop - 3);
        Console.WriteLine(string.Format("Heartbeat {0}: {1}, {2}", _heartbeat.Timestamp.ToString("HH:mm:ss"), _heartbeat.ReaderID, _heartbeat.BatteryStatus));
        Console.SetCursorPosition(0, Console.CursorTop + 2);
    }
}

void SpcInterface_RawDataReceived(object _sender, RawDataReceived _rawData)
{
    //Event raised when data received from Reader
    DecodeReceivedText(_rawData.Data);

    AddConsoleLines();
}
void DecodeReceivedText(string _receivedText)
{
    // Remove <CR> if present
    _receivedText = _receivedText.TrimEnd(['\r']);
    lock (m_Lock)
    {
        Console.SetCursorPosition(0, Console.CursorTop - 2);
        Console.WriteLine(string.Format("Data received: {0}", _receivedText));
        Console.SetCursorPosition(0, Console.CursorTop + 1);
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

                    Console.SetCursorPosition(0, Console.CursorTop - 1);
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
    AddConsoleLines();

    //Check cyclically if key pressed
    bool exit = false;
    while (!exit)
    {
        var key = Console.ReadKey();
        switch (key.Key)
        {
            case ConsoleKey.R:
                SendReadRequest(spcIntControl);
                break;
            case ConsoleKey.W:
                SendWriteRequest(spcIntControl);
                break;
            case ConsoleKey.X:
                exit = true;
                break;
        }
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