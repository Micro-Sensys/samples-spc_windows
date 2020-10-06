package de.microsensys.sampleapp_java.spccontrol;

import java.awt.Color;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.GridLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.WindowEvent;
import java.awt.event.WindowListener;
import java.util.Enumeration;

import javax.swing.AbstractButton;
import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.SpringLayout;
import javax.swing.SwingUtilities;
import javax.swing.border.EmptyBorder;
import javax.swing.text.DefaultCaret;

import de.microsensys.exceptions.MssException;
import de.microsensys.spc_control.RawDataReceived;
import de.microsensys.spc_control.ReaderHeartbeat;
import de.microsensys.spc_control.SpcInterfaceCallback;
import de.microsensys.spc_control.SpcInterfaceControl;

public class MainWindow extends JFrame implements ActionListener {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1167251094000250767L;
	
	private final String actionConnect = "connect";
	private final String actionDisconnect = "disconnect";
	private final String actionRead = "read";
	private final String actionWrite = "write";
	
	private JPanel contentPane;
	
	private JComboBox comboBoxDevices;
	private JButton buttonConnect;
	private JButton buttonDisconnect;
	private JTextField textFieldTidRead;
	private JButton buttonRead;
	private JTextArea textAreaDataRead;
	private JButton buttonWrite;
	private JTextArea textAreaDataToWrite;
	private JTextArea textAreaLogging;
	private JLabel labelReaderID;
	private JLabel labelBatStatus;
	private JLabel labelResultColor;
	
	private CheckConnectingReader mCheckThread;
	
	SpcInterfaceControl mSpcInterfaceControl;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					MainWindow frame = new MainWindow();
					frame.addWindowListener(new WindowListener() {

						@Override
						public void windowOpened(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowClosing(WindowEvent e) {
							frame.closeCommunication();
						}

						@Override
						public void windowClosed(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowIconified(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowDeiconified(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowActivated(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowDeactivated(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}
						
					});
					frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the frame.
	 */
	public MainWindow() {
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setBounds(100, 100, 450, 600);
		
		SpringLayout layout;
		
		layout = new SpringLayout();
		contentPane = new JPanel();
		contentPane.setBorder(new EmptyBorder(5, 5, 5, 5));
		contentPane.setLayout(layout);
		setContentPane(contentPane);
		
		//TODO get available COM-Ports. Example for Windows, Linux and macOS
		String comPortNames[] = {"COM12", "/dev/ttyUSB0", "/dev/tty.usbserial-00000001"};
		
		comboBoxDevices = new JComboBox(comPortNames);
		buttonConnect = new JButton("CONNECT");
		buttonConnect.setActionCommand(actionConnect);
		buttonConnect.addActionListener(this);
		buttonDisconnect = new JButton("DISCONNECT");
		buttonDisconnect.setActionCommand(actionDisconnect);
		buttonDisconnect.addActionListener(this);
		buttonDisconnect.setEnabled(false);
		JLabel tidTitle = new JLabel("TID text:");
		textFieldTidRead = new JTextField();
		tidTitle.setLabelFor(textFieldTidRead);
		buttonRead = new JButton("READ");
		buttonRead.setActionCommand(actionRead);
		buttonRead.addActionListener(this);
		buttonRead.setEnabled(false);
		JLabel dataReadTitle = new JLabel("  Data read:");
		dataReadTitle.setVerticalAlignment(JTextField.BOTTOM);
		textAreaDataRead = new JTextArea();
		textAreaDataRead.setEditable(false);
		textAreaDataRead.setLineWrap(true);
		textAreaDataRead.setWrapStyleWord(true);
		textAreaDataRead.setPreferredSize(new Dimension(30, 70));
		JLabel dataToWriteTitle = new JLabel("  Data to write:");
		dataToWriteTitle.setVerticalAlignment(JTextField.BOTTOM);
		buttonWrite = new JButton("WRITE");
		buttonWrite.setActionCommand(actionWrite);
		buttonWrite.addActionListener(this);
		buttonWrite.setEnabled(false);
		textAreaDataToWrite = new JTextArea();
		textAreaDataToWrite.setLineWrap(true);
		textAreaDataToWrite.setWrapStyleWord(true);
		textAreaDataToWrite.setPreferredSize(new Dimension(30, 70));
		textAreaLogging = new JTextArea();
		JScrollPane scrollPane = new JScrollPane(textAreaLogging);
		scrollPane.setVerticalScrollBarPolicy(JScrollPane.VERTICAL_SCROLLBAR_ALWAYS);
		DefaultCaret caret = (DefaultCaret)textAreaLogging.getCaret();
		caret.setUpdatePolicy(DefaultCaret.ALWAYS_UPDATE);
		textAreaLogging.setEditable(false);
		textAreaLogging.setLineWrap(true);
		textAreaLogging.setWrapStyleWord(true);
		labelReaderID = new JLabel("ReaderID: --");
		labelBatStatus = new JLabel("BatStatus: --");
		labelResultColor = new JLabel();
		labelResultColor.setOpaque(true);
		labelResultColor.setPreferredSize(new Dimension(10,10));
		JLabel resultTitle = new JLabel("Result:");
		
		contentPane.add(comboBoxDevices);
		layout.putConstraint(SpringLayout.NORTH, comboBoxDevices, 5, SpringLayout.NORTH, contentPane);
		layout.putConstraint(SpringLayout.WEST, comboBoxDevices, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, comboBoxDevices, -5, SpringLayout.EAST, contentPane);
		
		JPanel topPanel = new JPanel();
		topPanel.setLayout(new GridLayout(0,2));
		topPanel.add(buttonConnect);
		topPanel.add(buttonDisconnect);
		contentPane.add(topPanel);
		layout.putConstraint(SpringLayout.NORTH, topPanel, 5, SpringLayout.SOUTH, comboBoxDevices);
		layout.putConstraint(SpringLayout.WEST, topPanel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, topPanel, -5, SpringLayout.EAST, contentPane);
		
		contentPane.add(tidTitle);
		layout.putConstraint(SpringLayout.NORTH, tidTitle, 5, SpringLayout.SOUTH, topPanel);
		layout.putConstraint(SpringLayout.WEST, tidTitle, 5, SpringLayout.WEST, contentPane);
		contentPane.add(textFieldTidRead);
		layout.putConstraint(SpringLayout.NORTH, textFieldTidRead, 0, SpringLayout.NORTH, tidTitle);
		layout.putConstraint(SpringLayout.WEST, textFieldTidRead, 5, SpringLayout.EAST, tidTitle);
		layout.putConstraint(SpringLayout.EAST, textFieldTidRead, -5, SpringLayout.EAST, contentPane);
		
		JPanel top2Panel = new JPanel();
		top2Panel.setLayout(new GridLayout(0,2));
		top2Panel.add(buttonRead);
		top2Panel.add(dataReadTitle);
		contentPane.add(top2Panel);
		layout.putConstraint(SpringLayout.NORTH, top2Panel, 5, SpringLayout.SOUTH, textFieldTidRead);
		layout.putConstraint(SpringLayout.WEST, top2Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top2Panel, -5, SpringLayout.EAST, contentPane);
		
		contentPane.add(textAreaDataRead);
		layout.putConstraint(SpringLayout.NORTH, textAreaDataRead, 5, SpringLayout.SOUTH, top2Panel);
		layout.putConstraint(SpringLayout.WEST, textAreaDataRead, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, textAreaDataRead, -5, SpringLayout.EAST, contentPane);
		
		JPanel top3Panel = new JPanel();
		top3Panel.setLayout(new GridLayout(0,2));
		top3Panel.add(dataToWriteTitle);
		top3Panel.add(buttonWrite);
		contentPane.add(top3Panel);
		layout.putConstraint(SpringLayout.NORTH, top3Panel, 5, SpringLayout.SOUTH, textAreaDataRead);
		layout.putConstraint(SpringLayout.WEST, top3Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top3Panel, -5, SpringLayout.EAST, contentPane);
		
		contentPane.add(textAreaDataToWrite);
		layout.putConstraint(SpringLayout.NORTH, textAreaDataToWrite, 5, SpringLayout.SOUTH, top3Panel);
		layout.putConstraint(SpringLayout.WEST, textAreaDataToWrite, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, textAreaDataToWrite, -5, SpringLayout.EAST, contentPane);
		
		JLabel separator = new JLabel("Logging:");
		separator.setOpaque(true);
		separator.setBackground(Color.BLACK);
		separator.setPreferredSize(new Dimension(10, 10));
		layout.putConstraint(SpringLayout.NORTH, separator, 5, SpringLayout.SOUTH, textAreaDataToWrite);
		layout.putConstraint(SpringLayout.WEST, separator, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, separator, -5, SpringLayout.EAST, contentPane);
		
		contentPane.add(labelResultColor);
		contentPane.add(resultTitle);
		layout.putConstraint(SpringLayout.NORTH, labelResultColor, 0, SpringLayout.NORTH, resultTitle);
		layout.putConstraint(SpringLayout.SOUTH, labelResultColor, 0, SpringLayout.SOUTH, resultTitle);
		layout.putConstraint(SpringLayout.EAST, labelResultColor, -5, SpringLayout.EAST, contentPane);
		layout.putConstraint(SpringLayout.SOUTH, resultTitle, -5, SpringLayout.SOUTH, contentPane);
		layout.putConstraint(SpringLayout.EAST, resultTitle, -5, SpringLayout.WEST, labelResultColor);
		
		JPanel bottomPanel = new JPanel();
		bottomPanel.setLayout(new GridLayout(0,2));
		bottomPanel.add(labelReaderID);
		bottomPanel.add(labelBatStatus);
		contentPane.add(bottomPanel);
		layout.putConstraint(SpringLayout.SOUTH, bottomPanel, 0, SpringLayout.SOUTH, labelResultColor);
		layout.putConstraint(SpringLayout.EAST, bottomPanel, -5, SpringLayout.WEST, resultTitle);
		layout.putConstraint(SpringLayout.WEST, bottomPanel, 5, SpringLayout.WEST, contentPane);
		
		contentPane.add(scrollPane);
		layout.putConstraint(SpringLayout.NORTH, scrollPane, 5, SpringLayout.SOUTH, separator);
		layout.putConstraint(SpringLayout.SOUTH, scrollPane, -5, SpringLayout.NORTH, bottomPanel);
		layout.putConstraint(SpringLayout.WEST, scrollPane, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, scrollPane, -5, SpringLayout.EAST, contentPane);
		
//		JPanel top5Panel = new JPanel();
//		top5Panel.setLayout(new GridLayout(0,2));
//		top5Panel.add(buttonReaderID);
//		top5Panel.add(buttonIdentify);
//		contentPane.add(top5Panel);
//		layout.putConstraint(SpringLayout.NORTH, top5Panel, 5, SpringLayout.SOUTH, top4Panel);
//		layout.putConstraint(SpringLayout.WEST, top5Panel, 5, SpringLayout.WEST, contentPane);
//		layout.putConstraint(SpringLayout.EAST, top5Panel, -5, SpringLayout.EAST, contentPane);
//		
//		JPanel top6Panel = new JPanel();
//		top6Panel.setLayout(new GridLayout(0,2));
//		top6Panel.add(buttonReadBytes);
//		top6Panel.add(buttonWriteBytes);
//		contentPane.add(top6Panel);
//		layout.putConstraint(SpringLayout.NORTH, top6Panel, 5, SpringLayout.SOUTH, top5Panel);
//		layout.putConstraint(SpringLayout.WEST, top6Panel, 5, SpringLayout.WEST, contentPane);
//		layout.putConstraint(SpringLayout.EAST, top6Panel, -5, SpringLayout.EAST, contentPane);
//		
//		JPanel top7Panel = new JPanel();
//		top7Panel.setLayout(new GridLayout(0,3));
//		top7Panel.add(checkBoxLegicFs);
//		top7Panel.add(labelPageTitle);
//		top7Panel.add(textFieldPageNum);
//		contentPane.add(top7Panel);
//		layout.putConstraint(SpringLayout.NORTH, top7Panel, 5, SpringLayout.SOUTH, top6Panel);
//		layout.putConstraint(SpringLayout.WEST, top7Panel, 5, SpringLayout.WEST, contentPane);
//		layout.putConstraint(SpringLayout.EAST, top7Panel, -5, SpringLayout.EAST, contentPane);
//		
//		contentPane.add(scrollPane);
//		layout.putConstraint(SpringLayout.NORTH, scrollPane, 5, SpringLayout.SOUTH, top7Panel);
//		layout.putConstraint(SpringLayout.WEST, scrollPane, 5, SpringLayout.WEST, contentPane);
//		layout.putConstraint(SpringLayout.EAST, scrollPane, -5, SpringLayout.EAST, contentPane);
//		layout.putConstraint(SpringLayout.SOUTH, scrollPane, -5, SpringLayout.SOUTH, contentPane);
	}
	
	protected void closeCommunication() {
		if (mSpcInterfaceControl != null){
            mSpcInterfaceControl.closeCommunicationPort();
        }
	}
	
	void setEnabledRadioButtons(ButtonGroup _bg, boolean _enabled) {
		Enumeration<AbstractButton> en = _bg.getElements();
		while(en.hasMoreElements()) {
			((JRadioButton)en.nextElement()).setEnabled(_enabled);
		}
	}

	public void appendResultText(String _toAppend) {
		appendResultText(_toAppend, true);
	}
	public void appendResultText(String _toAppend, boolean _autoAppendNewLine) {
		SwingUtilities.invokeLater(new Runnable() {
        	public void run() {
        		if (_autoAppendNewLine)
        			textAreaLogging.append(_toAppend + "\n");
        		else
        			textAreaLogging.append(_toAppend);
        	}
        });
	}
	
	public void updateReaderInfo(int _readerID, ReaderHeartbeat.BatStatus _batStatus) {
		SwingUtilities.invokeLater(new Runnable() {
        	public void run() {
        		labelReaderID.setText("ReaderID: " + String.valueOf(_readerID));
        		labelBatStatus.setText("BatStatus: " + _batStatus.toString());
        	}
        });
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		switch(e.getActionCommand()) {
		case actionConnect:
			connect();
			break;
		case actionDisconnect:
			disconnect();
			break;
		case actionRead:
			sendReadRequest();
			break;
		case actionWrite:
			sendWriteRequest();
			break;
		}
	}
	
	private void connect() {
		textAreaLogging.setText("");
		
		//Before opening a new communication port, make sure that previous instance is disposed
		disposeSpcControl();
		if (comboBoxDevices.getSelectedIndex() == -1) {
			textAreaLogging.append("Please select a COM-Port to connect to");
			return;
		}
		comboBoxDevices.setEnabled(false);
		
		//Initialize SpcInterfaceControl instance.
		//	PortType = 2 --> Bluetooth
		//	PortName = selected COM-Port in ComboBox
		mSpcInterfaceControl = new SpcInterfaceControl(
				mSpcCallback, //Callback where events will be notified 
				2, //2 == Bluetooth
				comboBoxDevices.getSelectedItem().toString());
		//Configure DataPrefix and DataSuffix
		//	SpcInterfaceControl will automatically use this to divide the received data and call
		//		the "spcRawDataReceived" or "spcReaderHeartbeatReceived" methods from the callback
		mSpcInterfaceControl.setDataPrefix("");
		mSpcInterfaceControl.setDataSuffix("\r\n");
		
		//Try to open communication port. This call does not block!!
		try {
			mSpcInterfaceControl.openCommunicationPort();
			//No exception --> Check for process in a separate thread
			textAreaLogging.append("Connecting...");
			startCheckConnectingThread();
			buttonConnect.setEnabled(false);
			buttonDisconnect.setEnabled(true);
		} catch (MssException e) {
			//Exception thrown by "initialize" if something was wrong
			e.printStackTrace();
			textAreaLogging.append("Error opening port.");
			comboBoxDevices.setEnabled(true);
		}
	}
	public void connectProcedureFinished() {
		if (mSpcInterfaceControl.getIsCommunicationPortOpen()) {
			SwingUtilities.invokeLater(new Runnable() {
	        	public void run() {
	        		textAreaLogging.append("\n CONNECTED \n");
	        		buttonRead.setEnabled(true);
	        		buttonWrite.setEnabled(true);
	        	}
	        });
		}
		else {
			//Communication port is not open
			appendResultText("\n Reader NOT connected \n  --> PRESS DISCONNECT BUTTON");
		}
	}

	private void disconnect() {
		disposeSpcControl();
		comboBoxDevices.setEnabled(true);
		buttonConnect.setEnabled(true);
		buttonDisconnect.setEnabled(false);
		buttonRead.setEnabled(false);
		buttonWrite.setEnabled(false);
        labelReaderID.setText("ReaderID: --");
        labelBatStatus.setText("BatStatus: --");
	}
	
	private void sendReadRequest() {
		// Generate "SCAN" request and send to reader
		String command = "~T"; //SOH + Read-Identifier
		
		textFieldTidRead.setText("");
		textAreaDataRead.setText("");
		labelResultColor.setBackground(Color.WHITE);
		
		try {
			mSpcInterfaceControl.sendSpcRequest(command);
			appendResultText("Sent READ Request: " + command);
		} catch (MssException e) {
			e.printStackTrace();
		}
	}
	private void sendWriteRequest() {
		//Check if TID TextField is empty
		String tidText = textFieldTidRead.getText().toString();
		if (tidText.isEmpty()) {
			JOptionPane.showMessageDialog(this, "TID Text field is empty");
			return;
		}
		if (tidText.length() != 16) {
			JOptionPane.showMessageDialog(this, "TID Text is not the correct length");
			return;
		}
		String dataToWrite = textAreaDataToWrite.getText().toString();
		if (dataToWrite.isEmpty()) {
			JOptionPane.showMessageDialog(this, "Data to write field is empty");
			return;
		}
		
		// Generate "WRITE" request and send to reader
		String command = "~W"; //SOH + Write-Identifier
		command += tidText; //TID
		command += dataToWrite;
		
		labelResultColor.setBackground(Color.WHITE);
		
		try {
			mSpcInterfaceControl.sendSpcRequest(command);
			appendResultText("Sent WRITE Request: " + command);
		} catch (MssException e) {
			e.printStackTrace();
		}
	}
	
	private SpcInterfaceCallback mSpcCallback = new SpcInterfaceCallback() {
		
		@Override
		public void spcReaderHeartbeatReceived(ReaderHeartbeat _heartbeat) {
			SwingUtilities.invokeLater(new Runnable() {
	        	public void run() {
	        		textAreaLogging.append("Heartbeat received: " + _heartbeat.getReaderID() + ", " + _heartbeat.getBatStatus().toString() + "\n");
	        		labelReaderID.setText("ReaderID: " + _heartbeat.getReaderID());
	        		labelBatStatus.setText("BatStatus: " + _heartbeat.getBatStatus());
	        		labelResultColor.setBackground(Color.WHITE);
	        	}
	        });
		}
		
		@Override
		public void spcRawDataReceived(RawDataReceived _dataReceived) {
			SwingUtilities.invokeLater(new Runnable() {
	        	public void run() {
	        		decodeReceivedText(_dataReceived.getDataReceived());
	        	}
	        });
		}
	};
	
	private void decodeReceivedText(String _receivedText) {
		// Remove <CR> if present
        if (_receivedText.endsWith("\r"))
            _receivedText = _receivedText.substring(0, _receivedText.length()-1);
        appendResultText("Data received: " + _receivedText);
        switch ( _receivedText.charAt(0)){
            case 'T': //Transponder read. Decode TID and Data from received String
                if (_receivedText.length() >= 16){ //Check if minimum length received
                    // Remove "T" Identifier
                    _receivedText = _receivedText.substring(1);

                    //Get Personal-Nr., Ausweis-Nr. or Equi-Nr. (0-15)
                    String firstData = _receivedText.substring(0, 16);
                    firstData = firstData.replaceFirst("\\x00++$",""); //Remove possible not initialized data from the end of the String

                    //Get Equi-Daten (16 - 92)
                    String secondData = "";
                    if (_receivedText.length() > 16){
                        secondData = _receivedText.substring(16);
//                        secondData = secondData.replace("\0", ""); //Remove possible not initialized data
                        secondData = secondData.replaceFirst("\\x00++$",""); //Remove possible not initialized data from the end of the String
                    }

                    textFieldTidRead.setText(firstData);
                    textAreaDataRead.setText(secondData);
                    labelResultColor.setBackground(Color.GREEN);
                }
                break;
            case 'R': //Result String
                if  (_receivedText.startsWith("RW")){
                    //Write result
                    switch (_receivedText){
                        case "RW00":
                            // Result OK
                        	labelResultColor.setBackground(Color.GREEN);
                            break;
                        case "RW24":
                            //Transponder not found or error writing
                        	labelResultColor.setBackground(Color.RED);
                            break;
                    }
                }
                if  (_receivedText.startsWith("RT")){
                    //Read transponder error
                    if (_receivedText.substring(2, 2).equals("00")){
                        // Result OK
                    	labelResultColor.setBackground(Color.GREEN);
                    }
                    else{
                        // Error
                        // Example "RT2400" --> Transponder not found or error reading
                    	labelResultColor.setBackground(Color.RED);
                    }
                }
                break;
        }
	}
	
	private void disposeSpcControl(){
        if (mSpcInterfaceControl != null)
            mSpcInterfaceControl.closeCommunicationPort();
        mSpcInterfaceControl = null;
    }
	public void startCheckConnectingThread(){
        if (mCheckThread!=null){
            mCheckThread.cancel();
            mCheckThread=null;
        }
        mCheckThread = new CheckConnectingReader();
        mCheckThread.start();
    }
    private class CheckConnectingReader extends Thread {
        private boolean loop;

        CheckConnectingReader(){
            loop = true;
        }

        @Override
        public void run() {
            while (loop){
                if (mSpcInterfaceControl.getIsCommunicationPortOpening()){
                    //Still trying to connect -> Wait and continue
                    try {
                        Thread.sleep(200);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                    appendResultText(".", false);
                    continue;
                }
                //Connecting finished! Check if connected or not connected
                connectProcedureFinished();

                //Stop this thread
                cancel();
            }
        }

        void cancel(){
            loop = false;
        }
    }
}
