Imports System.Threading
Imports iIDReaderLibrary
Imports iIDReaderLibrary.Utils

Module Program
    '
    ' SampleManualCheck_VB
    '       SampleCode for iIDReaderLibrary.SpcInterfaceControl
    '       Implemented in VB
    '       Using "Start..." functions
    '
    ' This sample demonstrates how to call the SpcInterfaceControl functions that run the process in a separate new thread.
    ' This is only for demo purposes. For a Console application is not efficient to work in this way.

    ReadOnly m_Lock As New Object()
    Dim m_LastId As String

    Sub Main(args As String())
        Console.WriteLine(".NETCore Console")
        Console.WriteLine("SampleThreads_C#")
        Console.WriteLine("--------------------")
        Console.WriteLine("Library Version: " + SpcInterfaceControl.LibraryVersion)

        'Get SpcInterfaceControl instance
        Dim spcIntControl As SpcInterfaceControl = Console_InitializeSpcInterfaceControl()
        If spcIntControl IsNot Nothing Then
            'SpcInterfaceControl is initialized
            AddConsoleLines()

            'Check cyclically if key pressed
            Dim exitMain = False
            While Not exitMain
                Dim pressedKey As ConsoleKeyInfo = Console.ReadKey()
                Select Case pressedKey.Key
                    Case ConsoleKey.R
                        SendReadRequest(spcIntControl)
                    Case ConsoleKey.W
                        SendWriteRequest(spcIntControl)
                    Case ConsoleKey.X
                        exitMain = True
                End Select
                Thread.Sleep(500)
            End While

            spcIntControl.Terminate()
            Console.WriteLine("")
            Console.Write("EXITING in 5")
            Thread.Sleep(1000)
            Console.Write(", 4")
            Thread.Sleep(1000)
            Console.Write(", 3")
            Thread.Sleep(1000)
            Console.Write(", 2")
            Thread.Sleep(1000)
            Console.Write(", 1")
            Thread.Sleep(1000)
        Else
            Console.Write("Initialization error <press ENTER to exit>")
            Console.ReadLine()
        End If
    End Sub

    Private Sub SendReadRequest(_spcIntControl As SpcInterfaceControl)
        ' Generate "SCAN" request and send to reader
        Dim command As String = "~T"          ' SOH + Read-Identifier
        _spcIntControl.SendSpcRequest(command)
        Console.WriteLine(Environment.NewLine + String.Format("Send READ Request: {0}", command))
        AddConsoleLines()
    End Sub

    Private Sub SendWriteRequest(_spcIntControl As SpcInterfaceControl)
        If String.IsNullOrEmpty(m_LastId) Then
            Return
        End If
        ' Generate "WRITE" request and send to reader
        Dim command As String = "~W"          ' SOH + Write-Identifier
        command += m_LastId ' TID
        command += "ConsoleTest " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") ' Equipment - Data To write
        _spcIntControl.SendSpcRequest(command)
        Console.WriteLine(Environment.NewLine + String.Format("Send WRITE Request: {0}", command))
        AddConsoleLines()
    End Sub

    Private Sub AddConsoleLines()
        SyncLock m_Lock
            Console.WriteLine("")
            Console.WriteLine("Console running... (press R to send read request, or W to send write request, or X to exit)")

            Console.WriteLine("")
            Console.WriteLine("") 'HEARTBEAT
            Console.WriteLine("") 'DATA
            Console.WriteLine("") 'DATA
        End SyncLock
    End Sub

    Dim initializeCompleted As Boolean = False
    Private Function Console_InitializeSpcInterfaceControl() As SpcInterfaceControl
        Console.WriteLine("== Select initialize parameters ==")
        'Get PortType
        Dim portType As Integer = Console_InitializePortType()
        Dim portName = ""
        Select Case portType
            Case 0, 2
                'For serial & Bluetooth, PortName needed.
                portName = Console_InitializePortName()
        End Select
        'Initialize InterfaceCommunicationSettings class
        Dim readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, portName)

        'Parameters selected --> Initialize class instance
        Console.WriteLine("")
        Dim suffix As String = ChrW(&HD) + ChrW(&HA)
        Dim result = New SpcInterfaceControl(readerPortSettings, "", suffix)
        Console.WriteLine(String.Format("Selected parameters: PortType: {0} | PortName: {1}", portType, portName))

        'Call initialize to open the communication port
        AddHandler result.InitializeCompleted, AddressOf SpcInterfaceControl_InitializeCompleted
        result.StartInitialize()
        Console.Write("Initializing...")
        'For demo purposes, just wait blocking execution until "Initialize" process is completed (notified using "InitializeCompleted" event
        While Not initializeCompleted
            Thread.Sleep(100)
            Console.Write(".")
        End While
        Console.WriteLine("")
        If result.IsInitialized Then
            Console.WriteLine(vbTab + "Initialized")
            AddHandler result.RawDataReceived, AddressOf SpcInterface_RawDataReceived
            AddHandler result.ReaderHeartbeatReceived, AddressOf SpcInterface_ReaderHeartbeatReceived
            Return result
        Else
            'Iniitalization failed: Terminate class instance & retry
            Console.WriteLine(vbTab + "Initialize failed")
            result.Terminate()
            Return Console_InitializeSpcInterfaceControl()
        End If
    End Function

    Private Sub SpcInterface_ReaderHeartbeatReceived(_sender As Object, _heartbeat As ReaderHeartbeat)
        'Event raised when Heartbeat received from Reader
        SyncLock m_Lock
            Console.SetCursorPosition(0, Console.CursorTop - 3)
            Console.WriteLine(String.Format("Heartbeat {0}: {1}, {2}", _heartbeat.Timestamp.ToString("HH:mm:ss"), _heartbeat.ReaderID, _heartbeat.BatteryStatus))
            Console.SetCursorPosition(0, Console.CursorTop + 2)
        End SyncLock
    End Sub

    Private Sub SpcInterface_RawDataReceived(_sender As Object, _rawData As RawDataReceived)
        'Event raised when data received from Reader
        DecodeReceivedText(_rawData.Data)

        AddConsoleLines()
    End Sub

    Private Sub DecodeReceivedText(_receivedText As String)
        ' Remove <CR> if present
        Dim cr As Char = ChrW(&HD)
        Do Until _receivedText(_receivedText.Length - 1) <> cr
            _receivedText = _receivedText.Substring(0, _receivedText.Length - 1)
        Loop
        SyncLock m_Lock
            Console.SetCursorPosition(0, Console.CursorTop - 2)
            Console.WriteLine(String.Format("Data received: {0}", _receivedText))
            Console.SetCursorPosition(0, Console.CursorTop + 1)
            Select Case _receivedText(0)
                Case "T" ' Transponder read. Decode TID and Data from received string
                    If _receivedText.Length >= 16 Then ' Check if minimum length received
                        ' Remove "T" Identifier
                        _receivedText = _receivedText.Substring(1)

                        ' Get TID (0 - 15)
                        Dim firstData As String = _receivedText.Substring(0, 16)
                        Dim nullChar As Char = ChrW(0)
                        Do Until firstData(firstData.Length - 1) <> nullChar ' Remove not initialized data
                            firstData = firstData.Substring(0, firstData.Length - 1)
                        Loop
                        m_LastId = firstData

                        ' Get Data (16 - 92)
                        Dim secondData As String = ""
                        If _receivedText.Length > 16 Then
                            secondData = _receivedText.Substring(16)
                            Do Until secondData(secondData.Length - 1) <> nullChar ' Remove not initialized data
                                secondData = secondData.Substring(0, secondData.Length - 1)
                            Loop
                        End If

                        Console.SetCursorPosition(0, Console.CursorTop - 1)
                        Console.WriteLine(vbTab + firstData + vbTab + secondData)
                    End If
                Case "R" ' Result string
                    If _receivedText.StartsWith("RW") Then
                        ' Write result
                    End If
                    If _receivedText.StartsWith("RT") Then
                        ' Read transponder error
                    End If
            End Select
        End SyncLock
    End Sub

    Private Sub SpcInterfaceControl_InitializeCompleted(_sender As Object, _portOpen As Boolean)
        ' using "_portOpen" the result of the operation can be checked
        initializeCompleted = True
    End Sub

    Private Function Console_InitializePortType() As Integer
        Console.WriteLine("Port Type (0 = Serial, 2 = Bluetooth, 4 = USB)")
        Console.Write("Selection (confirm with ENTER): ")
        Dim portTypeTet = Console.ReadLine()
        Select Case portTypeTet
            Case "0"
                Console.WriteLine(vbTab + "Serial selected")
                Return 0
            Case "2"
                Console.WriteLine(vbTab + "Bluetooth selected")
                Return 2
            Case "4"
                Console.WriteLine(vbTab + "USB selected")
                Return 4
            Case Else
                Console.SetCursorPosition(0, Console.CursorTop - 2)
                Return Console_InitializePortType()
        End Select
    End Function
    Private Function Console_InitializePortName() As String
        Dim cursorTop = Console.CursorTop
        Dim portNames As String() = InterfaceCommunicationSettings.GetAvailablePortNames()
        Console.WriteLine("Port Name:")
        For i = 0 To portNames.Length - 1
            Console.WriteLine(String.Format("{0} - {1}", i, portNames(i)))
        Next
        Console.Write("Selection (confirm with ENTER): ")
        Dim portNameIndexTxt = Console.ReadLine()
        Dim portNameIndex As Integer
        If Integer.TryParse(portNameIndexTxt, portNameIndex) Then
            If portNameIndex < portNames.Length Then
                Console.WriteLine(vbTab + String.Format("{0} selected", portNames(portNameIndex)))
                Return portNames(portNameIndex)
            End If
        End If

        'Selection failed
        Console.SetCursorPosition(0, cursorTop)
        Return Console_InitializePortName()
    End Function
End Module
