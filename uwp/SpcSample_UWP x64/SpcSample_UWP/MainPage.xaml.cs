using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/*
* SpcSample_UWP
* 
* This sample represents how to handle the communication with a Bluetooth reader with a bidirectional script
* Requirements:
* 	- Bluetooth reader of the "space" series (POCKETwork, PENsolid) with following configuration:
* 		1) Bluetooth: SPP
* 		2) Bidirectional script (for unidirectional scripts, no request can be sent, but data can still be received using this sample)
* 	- Device must be paired into the operating system (normally a Virtual COM-Port will be asigned, but the COM-Port name is not needed for this sample)
*/ 

namespace SpcSample_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        microsensys.UwpDevTools.Communication.SpcInterfaceControl.SpcInterfaceControl m_SpcInterface = null;
        private volatile bool m_Disposing = false;

        public MainPage()
        {
            this.InitializeComponent();
            border_Result.Background = new SolidColorBrush(Colors.Transparent);
            comboBox_PortSelect.IsEnabled = false;
            button_OpenPort.IsEnabled = false;
            textBox_Logging.Text = "";
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
        {
            //Get Bluetooth devices paired and populate ComboBox
            string[] deviceNames = await microsensys.UwpDevTools.Communication.SerialPort.UwpBluetoothPort.GetBtDeviceNamesAsync();
            if (deviceNames != null)
            {
                if (deviceNames.Length > 0)
                {
                    foreach (string device in deviceNames)
                    {
                        comboBox_PortSelect.Items.Add(device);
                    }
                    comboBox_PortSelect.IsEnabled = true;
                    button_OpenPort.IsEnabled = true;
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //Dispose all instances when leaving the Page
            m_Disposing = true;
            if (m_SpcInterface != null)
                m_SpcInterface.Dispose();
        }

        private void comboBox_PortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_SpcInterface != null)
            {
                if (!button_OpenPort.IsEnabled || !m_SpcInterface.IsCommunicationPortOpen) button_OpenPort.IsEnabled = true;
            }
        }

        private async void button_OpenPort_ClickAsync(object sender, RoutedEventArgs e)
        {
            textBox_Logging.Text = "";

            //Before opening a new Communication Port, make sure that previous instance is disposed
            DisposeSpcInterface();
            if (comboBox_PortSelect.SelectedIndex == -1)
            {
                //No device selected --> Do nothing
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                new MessageDialog("Please select a Device to connect to").ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return;
            }

            try
            {
                //Initialize SpcInterfaceControl instance. 
                //  PortType = 2 --> Bluetooth
                //  PortName = selected device in ComboBox --> Device name as shown in Settings
                m_SpcInterface = new microsensys.UwpDevTools.Communication.SpcInterfaceControl.SpcInterfaceControl(
                    2, //2 == Bluetooth
                    comboBox_PortSelect.SelectedItem.ToString())
                {
                    //Configure DataPrefix and DataSuffix. 
                    //  SpcInterfaceControl will automatically use this to divide the received data and raise the RawDataReceived event or ReaderHeartbeatReceived event
                    DataPrefix = "",
                    DataSuffix = "\r\n"
                };
                //Open communication port. Sync version is also available if desired "OpenCommunicationPort"
                if (await m_SpcInterface.OpenCommunicationPortAsync())
                {
                    //True --> Communication port open
                    //Add EventHandlers to each event
                    m_SpcInterface.ReaderHeartbeatReceived += SpcInterface_ReaderHeartbeatReceived;
                    m_SpcInterface.RawDataReceived += SpcInterface_RawDataReceived;

                    AddLoggingText(String.Format("Port {0} was opened", comboBox_PortSelect.SelectedItem.ToString()));
                    button_Read.IsEnabled = true;
                    button_Write.IsEnabled = true;
                    button_OpenPort.IsEnabled = false;
                }
                else
                {
                    //False --> Communication port could not be opened
                    AddLoggingText(String.Format("{0} - can not open", comboBox_PortSelect.SelectedItem.ToString()));
                }
            }
            catch
            {
                AddLoggingText(String.Format("{0} - can not open", comboBox_PortSelect.SelectedItem.ToString()));
            }
        }

        private void button_Read_Click(object sender, RoutedEventArgs e)
        {
            // Generate "SCAN" request and send to reader
            string command = "~T";          // SOH + Read-Identifier
            AddLoggingText(String.Format("Send READ Request: {0}", command));

            if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                border_Result.Background = new SolidColorBrush(Colors.Transparent);

            textBox_Data.Text = textBox_Label.Text = String.Empty;

            m_SpcInterface.SendSpcRequest(command);
        }

        private void button_Write_Click(object sender, RoutedEventArgs e)
        {
            // Check if TID Label Text is empty
            if (string.IsNullOrEmpty(textBox_Label.Text))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                new MessageDialog("TID Text field is empty").ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return;
            }

            // Generate "WRITE" request and send to reader
            string command = "~W";              // SOH + Write-Identifier
            command += textBox_Label.Text;      // TID
            command += textBox_ToWrite.Text;    // Equipment-data to write

            AddLoggingText(String.Format("Send WRITE Request: {0}", command));

            if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                border_Result.Background = new SolidColorBrush(Colors.Transparent);

            m_SpcInterface.SendSpcRequest(command);
        }

        private void DisposeSpcInterface()
        {
            //Dispose SpcInterfaceControl
            if (m_SpcInterface != null)
                m_SpcInterface.Dispose();
        }

        private void SpcInterface_ReaderHeartbeatReceived(object sender, microsensys.UwpDevTools.Communication.SpcInterfaceControl.ReaderHeartbeatEventArgs e)
        {
            //Event raised when Heartbeat received from Reader
            if (m_Disposing) return;
            AddLoggingText(String.Format("Heartbeat received: {0}, {1}", e.ReaderId, e.BatStatus));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBlock_ReaderID.Text = e.ReaderId.ToString();
                textBlock_Battery.Text = e.BatStatus.ToString();
                if (((SolidColorBrush)border_Result.Background).Color != Colors.Transparent)
                    border_Result.Background = new SolidColorBrush(Colors.Transparent);

            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void SpcInterface_RawDataReceived(object sender, microsensys.UwpDevTools.Communication.SpcInterfaceControl.RawDataReceivedEventArgs e)
        {
            //Event raised when data received from Reader
            DecodeReceivedText(e.Data);
        }
        
        private void DecodeReceivedText(string _receivedText)
        {
            // Remove <CR> if present
            _receivedText = _receivedText.TrimEnd(new char[] { '\r' });
            AddLoggingText(String.Format("Data received: {0}", _receivedText));
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


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            textBox_Label.Text = firstData;
                            textBox_Data.Text = secondData;
                            border_Result.Background = new SolidColorBrush(Colors.LimeGreen);
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => border_Result.Background = new SolidColorBrush(Colors.LimeGreen));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                break;
                            case "RW24": // Transponder not found or error writing (Error)
                                if (m_Disposing)
                                    return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => border_Result.Background = new SolidColorBrush(Colors.Tomato));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => border_Result.Background = new SolidColorBrush(Colors.LimeGreen));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                        else
                        {
                            //Error
                            //Example "RT24" --> Transponder not found or error reading
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => border_Result.Background = new SolidColorBrush(Colors.Tomato));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (this.textBox_Logging.Text.Length > 5000) this.textBox_Logging.Text = "";

                this.textBox_Logging.Text = string.Format("{0:HH:mm:ss}; {1}\r\n", DateTime.Now, sLogging) + this.textBox_Logging.Text;

            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
