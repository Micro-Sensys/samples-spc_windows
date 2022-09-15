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

    Dim m_Completed = False
    Dim m_LastId As String

    Sub Main(args As String())
        'For demo purposes, just call MainAsync without waiting
        MainAsync(args)
        While Not m_Completed
            Thread.Sleep(1000)
        End While
    End Sub
    Async Sub MainAsync(args As String())
        Console.WriteLine(".NETCore Console")
        Console.WriteLine("SampleManualCheck_C#")
        Console.WriteLine("--------------------")
        Console.WriteLine("Library Version: " + SpcInterfaceControl.LibraryVersion)

        'Get SpcInterfaceControl instance
        Dim spcIntControl As SpcInterfaceControl = Await Console_InitializeSpcInterfaceControlAsync()
        If spcIntControl IsNot Nothing Then
            'SpcInterfaceControl is initialized
            Console.WriteLine("")
            Console.Write("Waiting for Heartbeat...")
            Dim heartbeat As ReaderHeartbeat = Await spcIntControl.GetHeartbeatAsync()
            If heartbeat IsNot Nothing Then
                Console.WriteLine("")
                Console.WriteLine("Detected Reader:")
                ShowHeartbeat(heartbeat)
            End If


            'Reader info obtained --> execute functions using menu
            Console.WriteLine("")
            While Await Console_ExecuteAndContinueAsync(spcIntControl)
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

        m_Completed = True
    End Sub

    Private Async Function Console_ExecuteAndContinueAsync(_spcIntControl As SpcInterfaceControl) As Task(Of Boolean)
        'Main Console MENU
        Console.WriteLine("")
        Console.WriteLine("--------------------")
        Console.WriteLine(" Console MENU")
        Console.WriteLine("--------------------")
        Console.WriteLine("0 - Get Last HEARTBEAT")
        Console.WriteLine("1 - Get HEARTBEAT asynchronously")
        Console.WriteLine("2 - Get Last RawData")
        Console.WriteLine("3 - Get RawData asynchronously")
        Console.WriteLine("4 - SEND Read request")
        Console.WriteLine("5 - SEND Write request")
        Console.WriteLine("X - EXIT")
        Console.Write("Selection (confirm with ENTER): ")
        Dim operationNumTxt = Console.ReadLine()
        Select Case operationNumTxt
            Case "0"
                Console.WriteLine(vbTab + "Get Last HEARTBEAT")
                Dim lastHb As ReaderHeartbeat = _spcIntControl.Heartbeat
                If lastHb IsNot Nothing Then
                    ShowHeartbeat(lastHb)
                End If
            Case "1"
                Console.WriteLine(vbTab + "Identfy")
                Dim hbAsync As ReaderHeartbeat = Await _spcIntControl.GetHeartbeatAsync()
                If hbAsync IsNot Nothing Then
                    ShowHeartbeat(hbAsync)
                End If
            Case "2"
                Console.WriteLine(vbTab + "Get Last RawData")
                Dim lastRawData As RawDataReceived = _spcIntControl.DataReceived
                If lastRawData IsNot Nothing Then
                    DecodeReceivedText(lastRawData.Data)
                End If
            Case "3"
                Console.WriteLine(vbTab + "Get RawData asynchronously")
                Dim lastRawData As RawDataReceived = Await _spcIntControl.GetDataReceivedAsync()
                If lastRawData IsNot Nothing Then
                    DecodeReceivedText(lastRawData.Data)
                End If
            Case "4"
                Console.WriteLine(vbTab + "Send READ request")
                SendReadRequest(_spcIntControl)
            Case "5"
                Console.WriteLine(vbTab + "Send WRITE request")
                SendWriteRequest(_spcIntControl)
            Case "x", "X"
                Return False
        End Select
        Thread.Sleep(500)
        Return True
    End Function

    Private Sub ShowHeartbeat(_heartbeat As ReaderHeartbeat)
        Console.WriteLine(String.Format("Heartbeat {0}: {1}, {2}", _heartbeat.Timestamp.ToString("HH:mm:ss"), _heartbeat.ReaderID, _heartbeat.BatteryStatus))
    End Sub
    Private Sub DecodeReceivedText(_receivedText As String)
        ' Remove <CR> if present
        Dim cr As Char = ChrW(&HD)
        Do Until _receivedText(_receivedText.Length - 1) <> cr
            _receivedText = _receivedText.Substring(0, _receivedText.Length - 1)
        Loop
        Console.WriteLine(String.Format("Data received: {0}", _receivedText))
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
    End Sub

    Private Sub SendReadRequest(_spcIntControl As SpcInterfaceControl)
        ' Generate "SCAN" request and send to reader
        Dim command As String = "~T"          ' SOH + Read-Identifier
        _spcIntControl.SendSpcRequest(command)
        Console.WriteLine(Environment.NewLine + String.Format("Send READ Request: {0}", command))
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
    End Sub

    Private Async Function Console_InitializeSpcInterfaceControlAsync() As Task(Of SpcInterfaceControl)
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
        Console.WriteLine("Initializing...")

        'Call initialize to open the communication port
        If Await result.InitializeAsync() Then
            Console.WriteLine(vbTab + "Initialized")
            Return result
        Else
            'Iniitalization failed: Terminate class instance & retry
            Console.WriteLine(vbTab + "Initialize failed")
            result.Terminate()
            Return Await Console_InitializeSpcInterfaceControlAsync()
        End If
    End Function

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
