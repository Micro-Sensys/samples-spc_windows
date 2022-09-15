using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using System;
using System.Threading;

namespace SampleThreads_CSharp
{
    class Program
    {
        /*
         * SampleThreads_CSharp
         *      SampleCode for iIDReaderLibrary.SpcInterfaceControl
         *      Implemented in C#
         *      Using "Start..." functions
         *      
         * This sample demonstrates how to call the SpcInterfaceControl functions that run the process in a separate new thread.
         * This is only for demo purposes. For a Console application is not efficient to work in this way.
         */

        private static readonly object m_Lock = new object();
        private static string m_LastId = null;

        static void Main(string[] args)
        {
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
        }

        private static void SendReadRequest(SpcInterfaceControl _spcIntControl)
        {
            // Generate "SCAN" request and send to reader
            string command = "~T";          // SOH + Read-Identifier
            _spcIntControl.SendSpcRequest(command);
            Console.WriteLine(string.Format("\nSend READ Request: {0}", command));
            AddConsoleLines();
        }
        private static void SendWriteRequest(SpcInterfaceControl _spcIntControl)
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

        private static void AddConsoleLines()
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

        static volatile bool initializeCompleted = false;
        private static SpcInterfaceControl Console_InitializeSpcInterfaceControl()
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
            SpcInterfaceControl result = new SpcInterfaceControl(readerPortSettings, "", "\r\n");
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

        private static void SpcInterfaceControl_InitializeCompleted(object _sender, bool _portOpen)
        {
            // using "_portOpen" the result of the operation can be checked
            initializeCompleted = true;
        }

        private static void SpcInterface_ReaderHeartbeatReceived(object _sender, ReaderHeartbeat _heartbeat)
        {
            //Event raised when Heartbeat received from Reader
            lock (m_Lock)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 3);
                Console.WriteLine(string.Format("Heartbeat {0}: {1}, {2}", _heartbeat.Timestamp.ToString("HH:mm:ss"), _heartbeat.ReaderID, _heartbeat.BatteryStatus));
                Console.SetCursorPosition(0, Console.CursorTop + 2);
            }
        }

        private static void SpcInterface_RawDataReceived(object _sender, RawDataReceived _rawData)
        {
            //Event raised when data received from Reader
            DecodeReceivedText(_rawData.Data);

            AddConsoleLines();
        }
        private static void DecodeReceivedText(string _receivedText)
        {
            // Remove <CR> if present
            _receivedText = _receivedText.TrimEnd(new char[] { '\r' });
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
                            _receivedText = _receivedText.Substring(1);

                            // Get TID (0 - 15)
                            string firstData = _receivedText.Substring(0, 16);
                            firstData = firstData.TrimEnd(new char[] { '\0' });      // Remove not initialized data
                            m_LastId = firstData;

                            // Get Data (16 - 92)
                            string secondData = "";
                            if (_receivedText.Length > 16)
                            {
                                secondData = _receivedText.Substring(16);
                                secondData = secondData.TrimEnd(new char[] { '\0' }); // Remove not initialized data
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

        private static int Console_InitializePortType()
        {
            Console.WriteLine("Port Type (0 = Serial, 2 = Bluetooth, 4 = USB)");
            Console.Write("Selection (confirm with ENTER): ");
            string portTypeTxt = Console.ReadLine();
            switch (portTypeTxt)
            {
                case "0":
                    Console.WriteLine("\tSerial selected");
                    return 0;
                case "2":
                    Console.WriteLine("\tBluetooth selected");
                    return 2;
                case "4":
                    Console.WriteLine("\tUSB selected");
                    return 4;
                default:
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    return Console_InitializePortType();
            }
        }
        private static string Console_InitializePortName()
        {
            int cursorTop = Console.CursorTop;
            string[] portNames = InterfaceCommunicationSettings.GetAvailablePortNames();
            Console.WriteLine("Port Name:");
            for (int i = 0; i < portNames.Length; i++)
            {
                Console.WriteLine(string.Format("{0} - {1}", i, portNames[i]));
            }
            Console.Write("Selection (confirm with ENTER): ");
            string portNameIndexTxt = Console.ReadLine();
            if (int.TryParse(portNameIndexTxt, out int portNameIndex))
            {
                if (portNameIndex < portNames.Length)
                {
                    Console.WriteLine(string.Format("\t{0} selected", portNames[portNameIndex]));
                    return portNames[portNameIndex];
                }
            }

            //Selection failed
            Console.SetCursorPosition(0, cursorTop);
            return Console_InitializePortName();
        }
    }
}
