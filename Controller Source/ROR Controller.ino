/*
	Name:       ROR Controller.ino
	Created:    12/15/2020 3:13:50 PM
	Author:     Nick Baker - ccrunner84@gmail.com
*/

#include <P1AM.h>
#include <SPI.h>
#include <Ethernet.h>
#include <ArduinoJson.h>
#include <ArduinoJson.hpp>
#include <string.h>


//Set MAC Address
byte mac[] = { 0x60, 0x52, 0xD0, 0x06, 0x8F, 0x21 };

//Set the ROR Controller IP Address
EthernetServer tcpServer = EthernetServer(5000);

//Connection to TheSkyX
EthernetClient tcpClient;
volatile bool bTheSkyXConnected = false;
volatile bool bTheSkyXMountConnected = false;
volatile bool bTheSkyXMountParked = false;

//Set TheSkyX IP Address
byte mini_pc_ip[] = { 192, 168, 1, 201 }; //Mini PC

//Maximum Server Connections
#define MAX_CLIENTS 2
int nConnectedClients = 0;
EthernetClient clients[2];

enum MotorStatus { MOVING = 0, STOPPED = 1 };
enum MotorDirection { OPEN = 0, OPEN_SLOW = 1, CLOSE = 2, CLOSE_SLOW = 3, STOP = 4 };

MotorStatus mStatus = MotorStatus::STOPPED;
MotorDirection mDirection = MotorDirection::OPEN;

//Used when Changing Direction
int nDirectionSwitchTimeout = 0;

//Input Labels
channelLabel openSlowSensor = { 1,1 }; //slot 1 channel 1
channelLabel openLimitSensor = { 1,2 }; //slot 1 channel 2
channelLabel closeSlowSensor = { 1,3 }; //slot 1 channel 3
channelLabel closeLimitSensor = { 1,4 }; //slot 1 channel 4
channelLabel scopeParkedSensor = { 1,5 }; //slot 1 channel 5
channelLabel manualOpenSwitch = { 1,6 }; //slot 1 channel 6
channelLabel manualcloseSwitch = { 1,7 }; //slot 1 channel 7

//Forward / Reverse Outputs
channelLabel fowardOutput = { 1,1 }; //slot 1 channel 1
channelLabel reverseOutput = { 1,2 }; //slot 1 channel 2

//Fast / Slow Outputs
channelLabel fastOutput = { 1,3 }; //slot 1 channel 3
channelLabel slowOutput = { 1,4 }; //slot 1 channel 4

//Power Outputs
channelLabel mountPower = { 1,5 }; //slot 1 channel 5
channelLabel cameraPower = { 1,6 }; //slot 1 channel 6
channelLabel heaterPower = { 1,7 }; //slot 1 channel 7

//Set Outputs - All At Once (mask) (backwards?)
uint32_t openNormalSpeed = 9; //0x09;
uint32_t openSlowSpeed = 5; //0x05;
uint32_t closeNormalSpeed = 10; //0xA;
uint32_t closeSlowSpeed = 6; //0x06;

//Keep Track of Time
long previousMillis = 0;

//Observatory Status Delay - 2500ms
#define OBSERVATORY_STATUS_DELAY 2500

//Client Check Delay
//Observatory Status Delay - 100ms
#define CLIENT_CHECK_DELAY 100
short sClientCheckCounter = 0;
short sTheSkyXCheckCounter = 0;

//Flags for Roof Opening
volatile bool bOpenRoof = false;
volatile bool bManualOpenRoof = false;
volatile bool bRoofNearOpenPosition = false;
volatile bool bRoofAtOpenPosition = false;

//Flags for Roof Closing
volatile bool bCloseRoof = false;
volatile bool bManualCloseRoof = false;
volatile bool bRoofNearClosedPosition = false;
volatile bool bRoofAtClosedPosition = false;

//Scope Parked Flag (Sensor)
volatile bool bScopeParked = false;

//Power Outputs
volatile bool bMountPower = false;
volatile bool bCameraPower = false;
volatile bool bHeaterPower = false;

//TheSkyX Javascript Commands to Check Scope Position
char* sConnectedMessage = "/* Java Script */\n/* Socket Start Packet */\nsky6RASCOMTele.IsConnected\n/* Socket End Packet */\n";
char* sParkedMessage = "/* Java Script */\n/* Socket Start Packet */\nsky6RASCOMTele.IsParked()\n/* Socket End Packet */\n";

//TCP Client Received Buffer
#define RECV_BUFFER_SIZE 32
char receivedBuffer[RECV_BUFFER_SIZE];

//TCP Client JSON Status Buffer
#define JSON_BUFFER_LEN 350
char jsonBuffer[JSON_BUFFER_LEN];

// The setup() function runs once each time the micro-controller starts
void setup()
{
	//Start Serial Port
	Serial.begin(9600);

	//Wait for module sign-on
	while (!P1.init());

	//Output module info
	P1.printModules();

	//Init MKR Shield
	Ethernet.init(5);

	//Set Timeout
	tcpClient.setTimeout(100);
	tcpClient.setConnectionTimeout(100);

	//Start Ethernet Shield
	if (Ethernet.begin(mac) == 0)
	{
		//Failed to get IP Address
		Serial.println("Failed to configure Ethernet using DHCP");

		//Output Additional Information
		if (Ethernet.hardwareStatus() == EthernetNoHardware)
			Serial.println("Ethernet shield was not found.  Sorry, can't run without hardware. :(");
		else if (Ethernet.linkStatus() == LinkOFF)
			Serial.println("Ethernet cable is not connected.");
	}
	else
	{
		//Ethernet OK
		Serial.println("Ethernet connected.");
	}

	//Start TCP Server
	tcpServer.begin();

	//Record Time
	previousMillis = millis();

	//Setup Watchdog Timer
	P1.configWD(10000, TOGGLE);
	P1.startWD();
}

// Add the main program code into the continuous loop() function
void loop()
{
	unsigned long currentMillis = millis();

	//Start Ethernet Server
	if (Ethernet.linkStatus() != EthernetLinkStatus::LinkON)
		Ethernet.begin(mac);
	else
	{
		//Check for New TCP Connections
		CheckForConnectedClients();

		//Check for Client Commands
		CheckForClientCommands();

		//Check for SkyX Connection
		CheckForTheSkyXConnection();
	}

	//Update Motor Drive
	SetMotorDrive();

	//Update ASCOM Observatory Status
	if ((currentMillis - previousMillis) > OBSERVATORY_STATUS_DELAY)
	{
		//Reset
		previousMillis = currentMillis;

		//Update TheSkyX Status
		CheckTheSkyXStatus();

		//Report JSON Status
		ReportStatus(0, true);
	}

	//Reset Watchdog Timer
	P1.petWD();

	//Check Module Connection
	if (P1.checkConnection(1) != 0)
	{
		Serial.println("ModuleConnectionIssue");
		NVIC_SystemReset();
	}

	delay(1);
}

void SetMotorDrive()
{
	//Check for Direction Switch Timeout
	if (nDirectionSwitchTimeout > 0)
	{
		nDirectionSwitchTimeout--;
		return;
	}

	//Check Mount Status - Stop Any Movement
	if (bTheSkyXMountConnected == false || bTheSkyXMountParked == false &&
		(bOpenRoof == true || bCloseRoof == true))
	{
		bOpenRoof = bCloseRoof = false;
	}

	//Check manual movement switch
	bManualOpenRoof = P1.readDiscrete(manualOpenSwitch);
	bManualCloseRoof = P1.readDiscrete(manualcloseSwitch);

	//Check if Roof Commanded to Move
	if (bOpenRoof == true || bManualOpenRoof == true)
	{
		//Check if Already Moving
		if (mStatus != MotorStatus::STOPPED &&
			(mDirection == MotorDirection::CLOSE || mDirection == MotorDirection::CLOSE_SLOW))
		{
			//Reset Outputs - Stop Motor
			P1.writeDiscrete(LOW, fowardOutput);
			P1.writeDiscrete(LOW, reverseOutput);
			P1.writeDiscrete(LOW, fastOutput);
			P1.writeDiscrete(LOW, slowOutput);

			//Set As Stopped
			mStatus = MotorStatus::STOPPED;
			mDirection = MotorDirection::STOP;

			//Reset Previous Command
			bCloseRoof = false;
			bManualCloseRoof = false;

			//Wait
			nDirectionSwitchTimeout = 3000;
			return;
		}

		//Check if Motor Stopped or Moving to Open Position
		if (mStatus == MotorStatus::STOPPED ||
			mDirection == MotorDirection::OPEN || mDirection == MotorDirection::OPEN_SLOW)
		{
			//Get Sensor Status
			bool openSlowStatus = bRoofNearOpenPosition = P1.readDiscrete(openSlowSensor);
			bool openLimitStatus = bRoofAtOpenPosition = P1.readDiscrete(openLimitSensor);

			//Check if Move Allowed
			if (openLimitStatus == false)
			{
				//Check if Moving Slow or Fast
				if (openSlowStatus == true)
					mDirection = MotorDirection::OPEN_SLOW;
				else
					mDirection = MotorDirection::OPEN;
			}
			else
			{
				//Stop motor Movement
				bOpenRoof = false;
				bManualOpenRoof = false;
				mDirection = MotorDirection::STOP;
			}
		}
	}
	else if (bCloseRoof == true || bManualCloseRoof == true)
	{
		//Check if Already Moving
		if (mStatus != MotorStatus::STOPPED &&
			(mDirection == MotorDirection::OPEN || mDirection == MotorDirection::OPEN_SLOW))
		{
			//Reset Outputs - Stop Motor
			P1.writeDiscrete(LOW, fowardOutput);
			P1.writeDiscrete(LOW, reverseOutput);
			P1.writeDiscrete(LOW, fastOutput);
			P1.writeDiscrete(LOW, slowOutput);

			//Set As Stopped
			mStatus = MotorStatus::STOPPED;
			mDirection = MotorDirection::STOP;

			//Reset Previous Command
			bOpenRoof = false;
			bManualOpenRoof = false;

			//Wait
			nDirectionSwitchTimeout = 3000;
			return;
		}

		//Check if Motor Stopped or Moving to Closed Positions
		if (mStatus == MotorStatus::STOPPED ||
			mDirection == MotorDirection::CLOSE || mDirection == MotorDirection::CLOSE_SLOW)
		{
			//Get Sensor Status
			bool closeSlowStatus = bRoofNearClosedPosition = P1.readDiscrete(closeSlowSensor);
			bool closeLimitStatus = bRoofAtClosedPosition = P1.readDiscrete(closeLimitSensor);

			//Check if Move Allowed
			if (closeLimitStatus == false)
			{
				//Check if Moving Slow or Fast
				if (closeSlowStatus == true)
					mDirection = MotorDirection::CLOSE_SLOW;
				else
					mDirection = MotorDirection::CLOSE;
			}
			else
			{
				//Stop motor Movement
				bCloseRoof = false;
				bManualCloseRoof = false;
				mDirection = MotorDirection::STOP;
			}
		}
	}
	else
	{
		//Disable All Outputs
		P1.writeDiscrete(LOW, fowardOutput);
		P1.writeDiscrete(LOW, reverseOutput);
		P1.writeDiscrete(LOW, fastOutput);
		P1.writeDiscrete(LOW, slowOutput);

		//Set As Stopped
		mStatus = MotorStatus::STOPPED;
		mDirection = MotorDirection::STOP;

		//Update Input Status when stationary
		bRoofNearClosedPosition = P1.readDiscrete(closeSlowSensor);
		bRoofAtClosedPosition = P1.readDiscrete(closeLimitSensor);
		bRoofNearOpenPosition = P1.readDiscrete(openSlowSensor);
		bRoofAtOpenPosition = P1.readDiscrete(openLimitSensor);
	}

	//Update Motor Outputs
	if (mDirection == MotorDirection::STOP)
	{
		//Disable All Outputs
		P1.writeDiscrete(LOW, fowardOutput);
		P1.writeDiscrete(LOW, reverseOutput);
		P1.writeDiscrete(LOW, fastOutput);
		P1.writeDiscrete(LOW, slowOutput);

		//Set As Stopped
		mStatus = MotorStatus::STOPPED;
	}
	else
	{
		if (mDirection == MotorDirection::OPEN)
		{
			//Open Full Speed
			P1.writeDiscrete(HIGH, fowardOutput);
			P1.writeDiscrete(LOW, reverseOutput);
			P1.writeDiscrete(HIGH, fastOutput);
			P1.writeDiscrete(LOW, slowOutput);

			//Set Status
			mStatus = MotorStatus::MOVING;
		}
		else if (mDirection == MotorDirection::OPEN_SLOW)
		{
			//Open Slow Speed
			P1.writeDiscrete(HIGH, fowardOutput);
			P1.writeDiscrete(LOW, reverseOutput);
			P1.writeDiscrete(LOW, fastOutput);
			P1.writeDiscrete(HIGH, slowOutput);

			//Set Status
			mStatus = MotorStatus::MOVING;
		}
		else if (mDirection == MotorDirection::CLOSE)
		{
			//Close Full Speed
			P1.writeDiscrete(LOW, fowardOutput);
			P1.writeDiscrete(HIGH, reverseOutput);
			P1.writeDiscrete(HIGH, fastOutput);
			P1.writeDiscrete(LOW, slowOutput);

			//Set Status
			mStatus = MotorStatus::MOVING;
		}
		else if (mDirection == MotorDirection::CLOSE_SLOW)
		{
			//Close Slow Speed
			P1.writeDiscrete(LOW, fowardOutput);
			P1.writeDiscrete(HIGH, reverseOutput);
			P1.writeDiscrete(LOW, fastOutput);
			P1.writeDiscrete(HIGH, slowOutput);

			//Set Status
			mStatus = MotorStatus::MOVING;
		}
		else
		{
			//STOP
			//Disable All outputs
			P1.writeDiscrete(LOW, fowardOutput);
			P1.writeDiscrete(LOW, reverseOutput);
			P1.writeDiscrete(LOW, fastOutput);
			P1.writeDiscrete(LOW, slowOutput);

			//Set Status
			mStatus = MotorStatus::STOPPED;
		}
	}
}

bool ParseCommand(String& sCommand)
{
	//Check Command - Set Flags
	if (sCommand == "open_roof")
	{
		bOpenRoof = true;
		bCloseRoof = false;

		return true;
	}
	else if (sCommand == "close_roof")
	{
		bOpenRoof = false;
		bCloseRoof = true;

		return true;
	}
	else if (sCommand == "stop_roof")
	{
		bOpenRoof = false;
		bCloseRoof = false;

		return true;
	}
	else if (sCommand == "mount_power_on")
	{
		bMountPower = true;
		P1.writeDiscrete(HIGH, mountPower);

		return true;
	}
	else if (sCommand == "mount_power_off")
	{
		bMountPower = false;
		P1.writeDiscrete(LOW, mountPower);

		return true;
	}
	else if (sCommand == "camera_power_on")
	{
		bCameraPower = true;
		P1.writeDiscrete(HIGH, cameraPower);

		return true;
	}
	else if (sCommand == "camera_power_off")
	{
		bCameraPower = false;
		P1.writeDiscrete(LOW, cameraPower);

		return true;
	}
	else if (sCommand == "heater_power_on")
	{
		bHeaterPower = true;
		P1.writeDiscrete(HIGH, heaterPower);

		return true;
	}
	else if (sCommand == "heater_power_off")
	{
		bHeaterPower = false;
		P1.writeDiscrete(LOW, heaterPower);

		return true;
	}
	else
		return false;
}

void CheckForTheSkyXConnection()
{
	//Add Delay for Client Check
	if (tcpClient.connected() == false && ++sTheSkyXCheckCounter > CLIENT_CHECK_DELAY)
		sTheSkyXCheckCounter = 0;
	else
		return;

	//Check if Connected
	if (tcpClient.connected() == false)
	{
		//Clear Errors
		bTheSkyXConnected = false;
		tcpClient.clearWriteError();

		if (tcpClient.connect(mini_pc_ip, 3040) == 1)
			bTheSkyXConnected = true;
		else
			bTheSkyXConnected = false;
	}
}

int GetTCPClientReply(int nTimeoutMS)
{
	//Reset Received Buffer - Set First Char to NULL
	receivedBuffer[0] = '\0';

	int nIdx = 0;
	int nTimeOut = 0;
	while (true)
	{
		//Check if Data Available
		while (tcpClient.available() > 0)
		{
			//Read Each Character
			receivedBuffer[nIdx++] = tcpClient.read();

			//Check for Maximum Size
			if (nIdx >= RECV_BUFFER_SIZE - 1)
				break;
		}

		//Check for Maximum Size
		if (nIdx >= RECV_BUFFER_SIZE - 1)
			break;

		//Check for Timeout
		if (++nTimeOut >= nTimeoutMS)
			break;

		//Wait
		delay(1);
	}

	//Check if Data Recieved & Complete Message
	if (nIdx > 0)
		receivedBuffer[nIdx++] = '\0';

	return nIdx;
}

void CheckTheSkyXStatus()
{
	//Check if Connected to the SkyX
	if (bTheSkyXConnected == true && tcpClient.connected() == true)
	{
		//Check Mount Connected Status
		tcpClient.write(sConnectedMessage);

		//Check for TheSkyX Reply Message
		int nReplyLength = GetTCPClientReply(250);

		//Check if Data Recieved
		if (nReplyLength > 0)
		{
			//Parse Response
			if (receivedBuffer[0] == '1')
				bTheSkyXMountConnected = true;
			else
				bTheSkyXMountConnected = false;
		}
		else
			bTheSkyXMountConnected = false;

		//If Mount is Connected, check if Parked
		if (bTheSkyXMountConnected == true)
		{
			//Check Mount Connected Status
			tcpClient.write(sParkedMessage);

			//Check for TheSkyX Reply Message
			nReplyLength = GetTCPClientReply(250);

			//Check if Data Recieved
			if (nReplyLength > 0)
			{
				//Parse Response
				if (receivedBuffer[0] == 't')
					bTheSkyXMountParked = true;
				else
					bTheSkyXMountParked = false;
			}
			else
				bTheSkyXMountParked = false;
		}
		else
			bTheSkyXMountParked = false;
	}
	else
	{
		bTheSkyXMountParked = false;
		bTheSkyXMountConnected = false;
	}
}

void CheckForConnectedClients()
{
	//Add Delay for Client Check
	if (nConnectedClients == 0 || ++sClientCheckCounter > CLIENT_CHECK_DELAY)
		sClientCheckCounter = 0;
	else
		return;

	//Check for New Connected Client
	EthernetClient newClient = tcpServer.accept();
	if (newClient)
	{
		for (int i = 0; i < MAX_CLIENTS; i++)
		{
			if (!clients[i])
			{
				nConnectedClients++;
				clients[i] = newClient;

				//Send Status to Newly Connected Client
				CheckTheSkyXStatus();
				ReportStatus(i, false);
				break;
			}
		}
	}
}

void CheckForClientCommands()
{
	//Check for New Client Commands
	for (int i = 0; i < MAX_CLIENTS; i++)
	{
		if (clients[i] && clients[i].available() > 0)
		{
			//Get Command
			String receiveBuffer = clients[i].readStringUntil('\n');

			//Parse String
			bool bParsed = ParseCommand(receiveBuffer);

			//Send Immediate Status Update
			if (bParsed == true)
				ReportStatus(i, false);
		}
	}

}

void ReportStatus(int nClient, bool bAllClients)
{
	if (nConnectedClients == 0)
		return;

	//Set JSON Document
	StaticJsonDocument<(size_t)JSON_BUFFER_LEN> doc;

	//doc["roof_status"]["ts"] = millis();
	doc["roof_status"]["opening"] = bOpenRoof;
	doc["roof_status"]["closing"] = bCloseRoof;

	if (mStatus == MotorStatus::STOPPED)
		doc["roof_status"]["moving"] = "false";
	else
		doc["roof_status"]["moving"] = "true";

	doc["roof_status"]["opened"] = bRoofAtOpenPosition;
	doc["roof_status"]["closed"] = bRoofAtClosedPosition;

	doc["sensor_status"]["near_open_position"] = bRoofNearOpenPosition;
	doc["sensor_status"]["at_open_position"] = bRoofAtOpenPosition;

	doc["sensor_status"]["near_closed_position"] = bRoofNearClosedPosition;
	doc["sensor_status"]["at_closed_position"] = bRoofAtClosedPosition;

	doc["sensor_status"]["mount_parked"] = bScopeParked;

	doc["theskyx_status"]["mount_connected"] = bTheSkyXMountConnected;
	doc["theskyx_status"]["mount_parked"] = bTheSkyXMountParked;

	//Build JSON String
	int nJsonLength = serializeJson(doc, jsonBuffer, sizeof(jsonBuffer));

	//Shift Array to Add STX & ETX
	for (int i = nJsonLength; i >= 0; i--)
		jsonBuffer[i] = jsonBuffer[i - 1];

	//Set STX
	jsonBuffer[0] = 2;

	//Set ETX
	jsonBuffer[++nJsonLength] = 3;
	jsonBuffer[++nJsonLength] = 0;

	if (bAllClients == true)
	{
		//Send Status to All Clients
		for (int i = 0; i < MAX_CLIENTS; i++)
		{
			if (clients[i])
			{
				if (clients[i].connected() == true)
					clients[i].write(jsonBuffer);
				else
				{
					nConnectedClients--;
					clients[i].stop();
				}
			}
		}
	}
	else if (nClient < MAX_CLIENTS)
	{
		if (clients[nClient])
		{
			if (clients[nClient].connected() == true)
				clients[nClient].write(jsonBuffer);
		}
	}

}
