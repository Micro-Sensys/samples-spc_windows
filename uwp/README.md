# SPC sample for UWP
This sample code is for **SPC** communication (devices in SPC mode) on Windows devices.

> For details on SPC communication check [Useful Links](#Useful-Links) 

## Requirements
* IDE Visual Studio 2017
* Micro-Sensys RFID reader with appropriate script running
* RFID transponders

> For compatible script files, check [Useful Links](#Useful-Links)

## Implementation
This code shows how to use **SpcInterfaceControl** class to communicate with a device running on SPC mode. 
Using this class the communication port can be open/closed. It automatically handles the data received and notifies the App using a EventHandlers, and provides a function to send trigger commands to the script.

## Steps
Import the project into your IDE and check the communication port name for the RFID reader and fill the name into the code.
For some samples the available devices are automatically detected. 

![Screenshot](screenshot/SpcSample_Uwp.png)

 1. Select the device you wish to connect to, and press OPEN PORT. Once the connect process finishes, the result will be shown in the EditText on the bottom side, and if the device is connected, the READ/WRITE buttons will be enabled.
 2. Received data will be automatically forwarded to the UI using the following Events
    * *ReaderHeartbeatReceived* will be called when Heartbeat is received
	* *RawDataReceived* will be called when other data is received
 3. Use READ/WRITE buttons to trigger the processes built in the script

## Useful Links

* [Scripts](https://www.microsensys.de/downloads/DevSamples/Sample%20Codes/SPC/Additionals/Sample%20scripts/)
* [iID® INTERFACE configuration tool (tool to upload script to reader)](https://www.microsensys.de/downloads/CDContent/Install/iID%c2%ae%20interface%20config%20tool.zip)
* GitHub *documentation* repository: [Micro-Sensys/documentation](https://github.com/Micro-Sensys/documentation)
	* [communication-modes/spc](https://github.com/Micro-Sensys/documentation/tree/master/communication-modes/spc)

## Contact

* For coding questions or questions about this sample code, you can use [support@microsensys.de](mailto:support@microsensys.de)
* For general questions about the company or our devices, you can contact us using [info@microsensys.de](mailto:info@microsensys.de)

## Authors

* **Victor Garcia** - *Initial work* - [MICS-VGarcia](https://github.com/MICS-VGarcia/)
