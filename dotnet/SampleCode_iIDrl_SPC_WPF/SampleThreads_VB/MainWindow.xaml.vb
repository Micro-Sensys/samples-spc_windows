Imports iIDReaderLibrary
Imports iIDReaderLibrary.SpcInterfaceFunctions
Imports iIDReaderLibrary.Utils

Class MainWindow

    Dim m_SpcInterface As SpcInterfaceControl = Nothing
    Private m_Disposing As Boolean = False

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim portNames As String() = InterfaceCommunicationSettings.GetAvailablePortNames()
        If portNames IsNot Nothing Then
            If portNames.Length > 0 Then
                For Each device As String In portNames
                    comboBox_PortSelect.Items.Add(device)
                Next
                comboBox_PortSelect.IsEnabled = True
                button_OpenPort.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        m_Disposing = True
        If m_SpcInterface IsNot Nothing Then
            m_SpcInterface.Dispose()
        End If
    End Sub

    Private Sub Button_OpenPort_Click(sender As Object, e As RoutedEventArgs)
        textBox_Logging.Text = ""

        If comboBox_PortSelect.SelectedIndex = -1 Then
            ' No device selected --> Do nothing
            MessageBox.Show("Please select a device to connect to")
            Return
        End If

        Try
            'Initialize InterfaceCommunicationSettings
            '   PortType = 2 --> Bluetooth
            '   PortName = selected device in ComboBox
            Dim readerPortSettings As InterfaceCommunicationSettings = InterfaceCommunicationSettings.GetForSerialDevice(2, comboBox_PortSelect.SelectedItem.ToString())
            Dim suffix As String = ChrW(&HD) + ChrW(&HA)
            m_SpcInterface = New SpcInterfaceControl(readerPortSettings, "", suffix)

            'Open communication port
            AddHandler m_SpcInterface.InitializeCompleted, AddressOf SpcInterfaceControl_InitializeCompleted
            m_SpcInterface.StartInitialize()
        Catch ex As Exception
            AddLoggingText(String.Format("{0} - cannot open", comboBox_PortSelect.SelectedItem.ToString()))
        End Try
    End Sub

    Private Sub Button_ClosePort_Click(sender As Object, e As RoutedEventArgs)
        If m_SpcInterface IsNot Nothing Then
            m_SpcInterface.Dispose()
            m_SpcInterface = Nothing
        End If
        button_OpenPort.IsEnabled = True
        button_Read.IsEnabled = False
        button_Write.IsEnabled = False
    End Sub

    Private Sub Button_Read_Click(sender As Object, e As RoutedEventArgs)
        ' Generate "SCAN" request and send to reader
        Dim command As String = "~T"          ' SOH + Read-Identifier
        AddLoggingText(String.Format("Send READ Request: {0}", command))

        Dim resultBackground As SolidColorBrush = border_Result.Background
        If resultBackground.Color <> Colors.Transparent Then
            border_Result.Background = New SolidColorBrush(Colors.Transparent)
        End If

        textBox_Data.Text = textBox_Label.Text = String.Empty

        m_SpcInterface.SendSpcRequest(command)
    End Sub

    Private Sub Button_Write_Click(sender As Object, e As RoutedEventArgs)
        ' Check if TID Label Text is empty
        If String.IsNullOrEmpty(textBox_Label.Text) Then
            MessageBox.Show("TID Text field is empty")
            Return
        End If
        ' Generate "WRITE" request and send to reader
        Dim command As String = "~W"          ' SOH + Write-Identifier
        command += textBox_Label.Text ' TID
        command += textBox_ToWrite.Text ' Equipment - Data To write
        AddLoggingText(String.Format("Send WRITE Request: {0}", command))

        Dim resultBackground As SolidColorBrush = border_Result.Background
        If resultBackground.Color <> Colors.Transparent Then
            border_Result.Background = New SolidColorBrush(Colors.Transparent)
        End If

        textBox_Data.Text = textBox_Label.Text = String.Empty

        m_SpcInterface.SendSpcRequest(command)
    End Sub

    Private Sub SpcInterfaceControl_InitializeCompleted(_sender As Object, _portOpen As Boolean)
        If _portOpen Then
            AddHandler m_SpcInterface.RawDataReceived, AddressOf SpcInterface_RawDataReceived
            AddHandler m_SpcInterface.ReaderHeartbeatReceived, AddressOf SpcInterface_ReaderHeartbeatReceived

            Dispatcher.Invoke(Sub()
                                  AddLoggingText(String.Format("Port {0} open", comboBox_PortSelect.SelectedItem.ToString()))
                                  button_Read.IsEnabled = True
                                  button_Write.IsEnabled = True
                                  button_OpenPort.IsEnabled = False
                              End Sub)
        Else
            'False --> Communication port could not be opened
            AddLoggingText(String.Format("{0} - cannot open", comboBox_PortSelect.SelectedItem.ToString()))
        End If
    End Sub

    Private Sub SpcInterface_ReaderHeartbeatReceived(_sender As Object, _heartbeat As ReaderHeartbeat)
        'Event raised when Heartbeat received from Reader
        If m_Disposing Then
            Return
        End If
        AddLoggingText(String.Format("Heartbeat received: {0}, {1}", _heartbeat.ReaderID, _heartbeat.BatteryStatus))
        Dispatcher.Invoke(Sub()
                              textBlock_ReaderID.Text = _heartbeat.ReaderID.ToString()
                              textBlock_Battery.Text = _heartbeat.BatteryStatus.ToString()
                              Dim resultBackground As SolidColorBrush = border_Result.Background
                              If resultBackground.Color <> Colors.Transparent Then
                                  border_Result.Background = New SolidColorBrush(Colors.Transparent)
                              End If
                          End Sub)
    End Sub

    Private Sub SpcInterface_RawDataReceived(_sender As Object, _rawData As RawDataReceived)
        'Event raised when data received from Reader
        If m_Disposing Then
            Return
        End If
        DecodeReceivedText(_rawData.Data)
    End Sub

    Private Sub DecodeReceivedText(_receivedText As String)
        ' Remove <CR> if present
        Dim cr As Char = ChrW(&HD)
        Do Until _receivedText(_receivedText.Length - 1) <> cr
            _receivedText = _receivedText.Substring(0, _receivedText.Length - 1)
        Loop
        AddLoggingText(String.Format("Data received: {0}", _receivedText))
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

                    ' Get Data (16 - 92)
                    Dim secondData As String = ""
                    If _receivedText.Length > 16 Then
                        secondData = _receivedText.Substring(16)
                        Do Until secondData(secondData.Length - 1) <> nullChar ' Remove not initialized data
                            secondData = secondData.Substring(0, secondData.Length - 1)
                        Loop
                    End If

                    If m_Disposing Then
                        Return
                    End If
                    Dispatcher.Invoke(Sub()
                                          textBox_Label.Text = firstData
                                          textBox_Data.Text = secondData
                                          border_Result.Background = New SolidColorBrush(Colors.LimeGreen)
                                      End Sub)
                End If
            Case "R" ' Result string
                If _receivedText.StartsWith("RW") Then
                    ' Write result
                    Select Case _receivedText
                        Case "RW00"
                            If m_Disposing Then
                                Return
                            End If
                            Dispatcher.Invoke(Sub()
                                                  border_Result.Background = New SolidColorBrush(Colors.LimeGreen)
                                              End Sub)
                        Case "RW24"
                            If m_Disposing Then
                                Return
                            End If
                            Dispatcher.Invoke(Sub()
                                                  border_Result.Background = New SolidColorBrush(Colors.Tomato)
                                              End Sub)
                    End Select
                End If
                If _receivedText.StartsWith("RT") Then
                    ' Read transponder error
                    If m_Disposing Then
                        Return
                    End If
                    If _receivedText.Substring(2, 2) = "00" Then
                        Dispatcher.Invoke(Sub()
                                              border_Result.Background = New SolidColorBrush(Colors.LimeGreen)
                                          End Sub)
                    Else
                        Dispatcher.Invoke(Sub()
                                              border_Result.Background = New SolidColorBrush(Colors.Tomato)
                                          End Sub)
                    End If
                End If
        End Select
    End Sub

    Private Sub AddLoggingText(sLogging As String)
        If m_Disposing Then
            Return
        End If
        Dispatcher.Invoke(Sub()
                              If textBox_Logging.Text.Length > 5000 Then
                                  textBox_Logging.Text = ""
                              End If
                              textBox_Logging.Text = String.Format("{0:HH:mm:ss}; {1}", DateTime.Now, sLogging) + Environment.NewLine + textBox_Logging.Text
                          End Sub)
    End Sub
End Class
