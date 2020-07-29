using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpcSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.IO.Ports.SerialPort m_SerialPort;
        private volatile bool m_Disposing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Windows Loaded EventHandler
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the COM-Port names and update ComboBox
            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string pn in portNames)
            {
                comboBox_PortSelect.Items.Add(pn);
            }
        }

        /// <summary>
        /// Windows Closed EventHandler
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_Disposing = true;
            DisposeSerialPort();
        }

        /// <summary>
        /// Disable Port method
        /// </summary>
        private void DisposeSerialPort()
        {
            try
            {
                if (m_SerialPort != null)
                {
                    m_SerialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(m_SerialPort_DataReceived);

                    if (m_SerialPort.IsOpen)
                        m_SerialPort.Close();

                    System.Threading.Thread.Sleep(100);         // Wait a little bit to close SerialPort connection
                    m_SerialPort.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Button 'Open Port' Click EventHandler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_OpenPort_Click(object sender, RoutedEventArgs e)
        {
            textBox_Logging.Clear();

            DisposeSerialPort();
            if (comboBox_PortSelect.SelectedIndex == -1 || comboBox_PortSelect.Text.Length <= 3)
            {
                MessageBox.Show("Please select a COM port", "COM Port", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Initialize Serial Port Class. Baudrate = 57600, 8 Databtis, One stopbit, no parity/Handshake
            m_SerialPort = new System.IO.Ports.SerialPort(comboBox_PortSelect.Text);
            m_SerialPort.BaudRate = 57600;
            m_SerialPort.Parity = System.IO.Ports.Parity.None;
            m_SerialPort.DataBits = 8;
            m_SerialPort.StopBits = System.IO.Ports.StopBits.One;
            m_SerialPort.Handshake = System.IO.Ports.Handshake.None;
            m_SerialPort.ReadTimeout = 500;
            m_SerialPort.WriteTimeout = 500;
            m_SerialPort.DtrEnable = false;
            m_SerialPort.RtsEnable = false;

            // Wait a little bit for SerialPort initialization
            System.Threading.Thread.Sleep(100);

            try
            {
                // Open communication port
                m_SerialPort.Open();

                if (!m_SerialPort.IsOpen)
                {
                    // Can not open port
                    MessageBox.Show("Port could not be opened.\nPlease check Reader ConnectionTool parameters", "Open Port", MessageBoxButton.OK, MessageBoxImage.Information);
                    AddLoggingText(string.Format("{0} can not open", comboBox_PortSelect.Text));
                    return;
                }
                else
                {
                    // Port opened --> Initialize EventHandler
                    AddLoggingText(string.Format("Port {0} was opened", m_SerialPort.PortName));
                    m_SerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(m_SerialPort_DataReceived);
                    button_Read.IsEnabled = true;
                    button_Write.IsEnabled = true;
                    button_OpenPort.IsEnabled = false;
                }
            }
            catch
            {
                AddLoggingText(string.Format("{0} can not open", comboBox_PortSelect.Text));
                MessageBox.Show("Port could not be opened.\nPlease check  the Reader", "Open Port", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        /// <summary>
        /// Button 'Read' Click EventHandler
        /// </summary>
        /// <remarks> Send the Read Request CMD to start the read Operation</remarks>
        private void button_Read_Click(object sender, RoutedEventArgs e)
        {
            // Generate request Read using command
            string command = "~T";          // SOH + Read-Identifier
            AddLoggingText(string.Format("Send READ Request: {0}", command));

            if (textBlock_Result.Background != Brushes.Transparent) textBlock_Result.Background = Brushes.Transparent;
            textBox_Data.Text = textBox_Label.Text = string.Empty;

            m_SerialPort.WriteLine(command);
        }

        private void comboBox_PortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_SerialPort != null)
            {
                if (!button_OpenPort.IsEnabled || !m_SerialPort.IsOpen) button_OpenPort.IsEnabled = true;
            }
        }

        /// <summary>
        /// Button 'Write' Click EventHandler
        /// </summary>
        /// <remarks> Send the Write Request CMD to start the write Operation</remarks>
        private void button_Write_Click(object sender, RoutedEventArgs e)
        {
            // Check if TID Label Text is empty
            if (string.IsNullOrEmpty(textBox_Label.Text))
            {
                MessageBox.Show("TID Text field is empty", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Generate request Write using command
            string command = "~W";              // SOH + Write-Identifier
            command += textBox_Label.Text;      // TID
            command += textBox_ToWrite.Text;    // Variable data     

            AddLoggingText(string.Format("Send WRITE Request: {0}", command));

            if (textBlock_Result.Background != Brushes.Transparent) textBlock_Result.Background = Brushes.Transparent;
            m_SerialPort.WriteLine(command);
        }

        /// <summary>
        /// Serial port Communication EventHandler
        /// </summary>
        /// <param name="e">Serial Data received Event Arguments</param>
        void m_SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (m_Disposing) return;

            if (m_SerialPort.IsOpen)
            {
                // Read the Data of Serial port Input Buffer
                string inData = ((System.IO.Ports.SerialPort)sender).ReadLine();
                
                // Remove <CR>
                inData = inData.TrimEnd(new char[] { '\r' });

                AddLoggingText(string.Format("Data received: {0}", inData));

                if (string.IsNullOrEmpty(inData)) return;

                switch (inData[0])
                {
                    case 'T':
                        if (inData.Length >= 16) // Check if minimum length received
                        {
                            // Remove "T" Identifier
                            inData = inData.Substring(1);

                            // Get first data (0 - 15)
                            string firstData = inData.Substring(0, 16);
                            firstData = firstData.TrimEnd(new char[] { '\0' });      // Remove not initialized data

                            // Get variable data (16 - 92)
                            string secondData = "";
                            if (inData.Length > 16)
                            {
                                secondData = inData.Substring(16);
                                secondData = secondData.TrimEnd(new char[] { '\0' }); // Remove not initialized data
                            }

                            if (m_Disposing) return;

                            Dispatcher.Invoke((Action)(() =>
                            {
                                textBox_Label.Text = firstData;
                                textBox_Data.Text = secondData;
                                textBlock_Result.Background = Brushes.LimeGreen;
                            }));
                        }
                        break;
                    case 'R':
                        if (inData.StartsWith("RW"))
                        {
                            // Write result
                            switch (inData)
                            {
                                case "RW00": // Result OK
                                    if (m_Disposing)
                                        return;
                                    Dispatcher.Invoke((Action)(() => textBlock_Result.Background = Brushes.LimeGreen));
                                    break;
                                case "RW24": // Transponder not found or error writing (Error)
                                    if (m_Disposing)
                                        return;
                                    Dispatcher.Invoke((Action)(() => textBlock_Result.Background = Brushes.Tomato));
                                    break;
                            }
                        }
                        if (inData.StartsWith("RT"))
                        {
                            // Read Transponder error
                            switch (inData)
                            {
                                case "RT24": // Transponder not found or error reading (Error)
                                    if (m_Disposing)
                                        return;
                                    Dispatcher.Invoke((Action)(() => textBlock_Result.Background = Brushes.Tomato));
                                    break;
                            }
                        }
                        break;
                    case 'H':
                        // Data after "H" are HEX String
                        byte[] readerBytes = ToByteArray(inData.Substring(1));
                        if (readerBytes.Length >= 8 + 4) // Check length
                        {
                            string readerId = Convert.ToString(readerBytes[0] + readerBytes[1] * 0x100 + readerBytes[2] * 0x10000);
                            List<byte> BatteryBytes = new List<byte>(readerBytes.ToList().GetRange(8, 4));
                            string battery = "";

                            // Battery format => 4 bytes: [2x Should] [2x Is]
                            byte[] status = new byte[2];
                            status[0] = BatteryBytes[1];
                            status[1] = BatteryBytes[0];

                            // Get the current value of Battery
                            double ist_value = BitConverter.ToInt16(status, 0);

                            status[0] = BatteryBytes[3];
                            status[1] = BatteryBytes[2];

                            // Get the should value of Battery
                            double soll_value = BitConverter.ToInt16(status, 0);

                            // Calculate the percet of Battery
                            double percent = (ist_value / (soll_value * 1.15)) * 100;
                            if (percent > 100) percent = 100;

                            if (percent > 95) battery = "Perfect";
                            else if (percent > 90) battery = "Good";
                            else if (percent > 86) battery = "Medium";
                            else battery = "Low";

                            if (m_Disposing) return;
                            Dispatcher.Invoke((Action)(() =>
                            {
                                textBlock_ReaderID.Text = readerId;
                                textBlock_Battery.Text = battery;
                                System.Threading.Thread.Sleep(250);
                                if (textBlock_Result.Background != Brushes.Transparent)
                                    textBlock_Result.Background = Brushes.Transparent;

                            }));
                        }
                        break;
                    default:
                        break;
                }
            }
            return;
        }

        /// <summary>
        /// Convert Hexadecimal String data to Byte Array method
        /// </summary>
        /// <param name="_hexString">Hexadecimal data string</param>
        /// <returns>Byte Array with convert data</returns>
        private byte[] ToByteArray(string _hexString)
        {
            List<byte> result = new List<byte>();
            for (int i = 0; i < _hexString.Length / 2; i++)
            {
                try
                {
                    result.Add(byte.Parse(_hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
                }
                catch
                {
                    continue;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Add Text data in Logging TextBox
        /// </summary>
        /// <param name="sLogging">String to write</param>
        private void AddLoggingText(string sLogging)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (this.textBox_Logging.Text.Length > 5000) this.textBox_Logging.Clear();

                this.textBox_Logging.AppendText(string.Format("{0:HH:mm:ss}; {1}\r\n", DateTime.Now, sLogging));
                this.textBox_Logging.ScrollToEnd();

            }));
        }
    }
}
