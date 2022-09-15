﻿using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using iIDReaderLibrary.Utils.Definitions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SampleManualCheck_CSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Threading.CancellationTokenSource m_CancellationTokenSource;

        SpcInterfaceControl m_SpcInterface = null;
        private volatile bool m_Disposing = false;

        public MainWindow()
        {
            InitializeComponent();

            m_CancellationTokenSource = new System.Threading.CancellationTokenSource();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get port names and populate ComboBox
            string[] portNames = InterfaceCommunicationSettings.GetAvailablePortNames();
            if (portNames != null)
            {
                if (portNames.Length > 0)
                {
                    foreach (string device in portNames)
                    {
                        comboBox_PortSelect.Items.Add(device);
                    }
                    comboBox_PortSelect.IsEnabled = true;
                    button_OpenPort.IsEnabled = true;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_Disposing = true;
            if (m_SpcInterface != null)
                m_SpcInterface.Dispose();
        }

        private void ComboBox_PortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Button_OpenPort_ClickAsync(object sender, RoutedEventArgs e)
        {
            textBox_Logging.Text = "";

            if (comboBox_PortSelect.SelectedIndex == -1)
            {
                //No device selected --> Do nothing
                MessageBox.Show("Please select a Device to connect to");
                return;
            }

            try
            {
                //Initialize InterfaceCommunicationSettings
                //  PortType = -> Bluteooth
                //  PortName = selected device in ComboBox
                var readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(
                    PortTypeEnum.PortType_Bluetooth, 
                    comboBox_PortSelect.SelectedItem.ToString());
                m_SpcInterface = new SpcInterfaceControl(readerPortSettings, "", "\r\n");

                //Open communication port
                if (await m_SpcInterface.InitializeAsync())
                {
                    AddLoggingText(string.Format("Port {0} open", comboBox_PortSelect.SelectedItem.ToString()));
                    button_GetHeartbeatAsync.IsEnabled = true;
                    button_GetLastHeartbeat.IsEnabled = true;
                    button_GetLastRawData.IsEnabled = true;
                    button_GetRawDataAsync.IsEnabled = true;
                    button_Read.IsEnabled = true;
                    button_Write.IsEnabled = true;
                    button_OpenPort.IsEnabled = false;
                }
                else
                {
                    //False --> Communication port could not be opened
                    AddLoggingText(string.Format("{0} - can not open", comboBox_PortSelect.SelectedItem.ToString()));
                }
            }
            catch
            {
                AddLoggingText(string.Format("{0} - can not open", comboBox_PortSelect.SelectedItem.ToString()));
            }
        }

        private void Button_ClosePort_Click(object sender, RoutedEventArgs e)
        {
            if (m_SpcInterface != null)
            {
                m_SpcInterface.Dispose();
                m_SpcInterface = null;
            }
            button_OpenPort.IsEnabled = true;
            button_GetHeartbeatAsync.IsEnabled = false;
            button_GetLastHeartbeat.IsEnabled = false;
            button_GetLastRawData.IsEnabled = false;
            button_GetRawDataAsync.IsEnabled = false;
            button_Read.IsEnabled = false;
            button_Write.IsEnabled = false;
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
            // Generate "SCAN" request and send to reader
            string command = "~T";          // SOH + Read-Identifier
            AddLoggingText(string.Format("Send READ Request: {0}", command));

            if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                border_Result.Background = new SolidColorBrush(Colors.Transparent);

            textBox_Data.Text = textBox_Label.Text = string.Empty;

            m_SpcInterface.SendSpcRequest(command);
        }

        private void Button_Write_Click(object sender, RoutedEventArgs e)
        {
            // Check if TID Label Text is empty
            if (string.IsNullOrEmpty(textBox_Label.Text))
            {
                MessageBox.Show("TID Text field is empty");
                return;
            }

            // Generate "WRITE" request and send to reader
            string command = "~W";              // SOH + Write-Identifier
            command += textBox_Label.Text;      // TID
            command += textBox_ToWrite.Text;    // Equipment-data to write

            AddLoggingText(string.Format("Send WRITE Request: {0}", command));

            if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                border_Result.Background = new SolidColorBrush(Colors.Transparent);

            m_SpcInterface.SendSpcRequest(command);
        }

        private void DecodeReceivedText(string _receivedText)
        {
            // Remove <CR> if present
            _receivedText = _receivedText.TrimEnd(new char[] { '\r' });
            AddLoggingText(string.Format("Data received: {0}", _receivedText));
            //For this implementation, first char is an "Identifier"
            switch (_receivedText[0])
            {
                case 'T': //Transponder read. Decode TID and Data from received string
                    if (_receivedText.Length >= 16) // Check if minimum length received
                    {
                        // Remove "T" Identifier
                        _receivedText = _receivedText.Substring(1);

                        // Get Personal-Nr., Ausweis-Nr. oder Equi-Nr. (0 - 15)
                        string firstData = _receivedText.Substring(0, 16);
                        firstData = firstData.TrimEnd(new char[] { '\0' });      // Remove not initialized data

                        // Get Equi-Daten (16 - 92)
                        string secondData = "";
                        if (_receivedText.Length > 16)
                        {
                            secondData = _receivedText.Substring(16);
                            secondData = secondData.TrimEnd(new char[] { '\0' }); // Remove not initialized data
                        }

                        if (m_Disposing) return;


                        Dispatcher.Invoke((Action)(() =>
                        {
                            textBox_Label.Text = firstData;
                            textBox_Data.Text = secondData;
                            border_Result.Background = new SolidColorBrush(Colors.LimeGreen);
                        }));
                    }
                    break;
                case 'R': //Result string
                    if (_receivedText.StartsWith("RW"))
                    {
                        // Write result
                        switch (_receivedText)
                        {
                            case "RW00": // Result OK
                                if (m_Disposing)
                                    return;
                                Dispatcher.Invoke((Action)(() => border_Result.Background = new SolidColorBrush(Colors.LimeGreen)));
                                break;
                            case "RW24": // Transponder not found or error writing (Error)
                                if (m_Disposing)
                                    return;
                                Dispatcher.Invoke((Action)(() => border_Result.Background = new SolidColorBrush(Colors.Tomato)));
                                break;
                        }
                    }
                    if (_receivedText.StartsWith("RT"))
                    {
                        // Read Transponder error
                        if (m_Disposing)
                            return;
                        if (_receivedText.Substring(2, 2) == "00")
                        {
                            //Result OK
                            Dispatcher.Invoke((Action)(() => border_Result.Background = new SolidColorBrush(Colors.LimeGreen)));
                        }
                        else
                        {
                            //Error
                            //Example "RT24" --> Transponder not found or error reading
                            Dispatcher.Invoke((Action)(() => border_Result.Background = new SolidColorBrush(Colors.Tomato)));
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Add Text data in Logging TextBox
        /// </summary>
        /// <param name="sLogging">String to write</param>
        private void AddLoggingText(string sLogging)
        {
            if (m_Disposing) return;
            Dispatcher.Invoke(() =>
            {
                if (textBox_Logging.Text.Length > 5000) textBox_Logging.Text = "";

                textBox_Logging.Text = string.Format("{0:HH:mm:ss}; {1}\r\n", DateTime.Now, sLogging) + textBox_Logging.Text;

            });
        }

        private void Button_GetLastHeartbeat_Click(object sender, RoutedEventArgs e)
        {
            var heartbeat = m_SpcInterface.Heartbeat;
            if (heartbeat != null)
            {
                AddLoggingText(string.Format("Heartbeat received: {0}, {1}", heartbeat.ReaderID, heartbeat.BatteryStatus));
                Dispatcher.Invoke(() =>
                {
                    textBlock_ReaderID.Text = heartbeat.ReaderID.ToString();
                    textBlock_Battery.Text = heartbeat.BatteryStatus.ToString();
                    if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                        border_Result.Background = new SolidColorBrush(Colors.Transparent);
                });
            }
        }

        private void Button_GetLastRawData_Click(object sender, RoutedEventArgs e)
        {
            var rawDataReceived = m_SpcInterface.DataReceived;
            if (rawDataReceived != null)
                DecodeReceivedText(rawDataReceived.Data);
        }

        private async void Button_GetHeartbeatAsync_ClickAsync(object sender, RoutedEventArgs e)
        {
            m_CancellationTokenSource.Cancel();
            AddLoggingText("Waiting asynchronously for Heartbeat...");
            var heartbeat = await m_SpcInterface.GetHeartbeatAsync(m_CancellationTokenSource.Token);
            if (heartbeat != null)
            {
                AddLoggingText(string.Format("Heartbeat received: {0}, {1}", heartbeat.ReaderID, heartbeat.BatteryStatus));
                Dispatcher.Invoke(() =>
                {
                    textBlock_ReaderID.Text = heartbeat.ReaderID.ToString();
                    textBlock_Battery.Text = heartbeat.BatteryStatus.ToString();
                    if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                        border_Result.Background = new SolidColorBrush(Colors.Transparent);
                });
            }
        }

        private async void Button_GetRawDataAsync_ClickAsync(object sender, RoutedEventArgs e)
        {
            m_CancellationTokenSource.Cancel();
            AddLoggingText("Waiting asynchronously for RawData...");
            var rawData = await m_SpcInterface.GetDataReceivedAsync(m_CancellationTokenSource.Token);
            if (rawData != null)
                DecodeReceivedText(rawData.Data);
        }
    }
}
