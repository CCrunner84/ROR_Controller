//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Dome driver for RORController
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Dome interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define Dome

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ASCOM.RORController
{
    //
    // Your driver's DeviceID is ASCOM.RORController.Dome
    //
    // The Guid attribute sets the CLSID for ASCOM.RORController.Dome
    // The ClassInterface/None attribute prevents an empty interface called
    // _RORController from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Dome Driver for RORController.
    /// </summary>
    [Guid("49fa7f0c-cd18-493f-b772-021ec472bf95")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Dome : IDomeV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.RORController.Dome";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM ROR Driver";

        internal static string sIPAddress = "192.168.1.200";
        internal static int nTCPPort = 5000;
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "false";

        internal static int nCommandTimeout = 5000 / 50; //50ms Sleep
        internal static int nOpenCloseTimeout = 20000 / 50; //50ms Sleep

        TcpClient tcpClientROR = new TcpClient();
        NetworkStream tcpStream;

        BackgroundWorker tcpClientThread = new BackgroundWorker();

        //Setup Dialog
        SetupDialogForm fSetup;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        private bool bMessageReceived = false;

        internal static bool bRoofOpened = false;
        internal static bool bRoofOpening = false;

        internal static bool bRoofClosed = false;
        internal static bool bRoofClosing = false;

        internal static bool bRoofMoving = false;

        internal static bool bNearClosedPositionInput = false;
        internal static bool bAtClosedPositionInput = false;

        internal static bool bNearOpenPositionInput = false;
        internal static bool bAtOpenPositionInput = false;

        internal static bool bScopeParked = false;

        internal static bool bTheSkyXMountConnected = false;
        internal static bool bTheSkyXMountParked = false;

        private Util ascomUtil = new Util();

        /// <summary>
        /// Initializes a new instance of the <see cref="RORController"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Dome()
        {
            tl = new TraceLogger("", "RORController");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("Dome", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object
            //TODO: Implement your additional construction here

            tl.LogMessage("Dome", "Completed initialisation");

            tcpClientThread.WorkerReportsProgress = true;
            tcpClientThread.WorkerSupportsCancellation = true;

            //Start Thread
            tcpClientThread.DoWork += TcpClientThread_DoWork;
            tcpClientThread.ProgressChanged += TcpClientThread_ProgressChanged;
        }

        private void TcpClientThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                string sReceivedMessage = (string)e.UserState;


                if (sReceivedMessage == "ACK" || sReceivedMessage == "NAK")
                {

                }
                else if (sReceivedMessage != "")
                {
                    //Check if JSON String
                    try
                    {
                        JObject jsonObject = JObject.Parse(sReceivedMessage);

                        //Update Roof Motor Status
                        bRoofOpened = Convert.ToBoolean(jsonObject["roof_status"]["opened"].ToString());
                        bRoofOpening = Convert.ToBoolean(jsonObject["roof_status"]["opening"].ToString());
                        bRoofClosed = Convert.ToBoolean(jsonObject["roof_status"]["closed"].ToString());
                        bRoofClosing = Convert.ToBoolean(jsonObject["roof_status"]["closing"].ToString());
                        bRoofMoving = Convert.ToBoolean(jsonObject["roof_status"]["moving"].ToString());

                        //Update Roof Sensor Status
                        bAtOpenPositionInput = Convert.ToBoolean(jsonObject["sensor_status"]["at_open_position"].ToString());
                        bNearOpenPositionInput = Convert.ToBoolean(jsonObject["sensor_status"]["near_open_position"].ToString());
                        bAtClosedPositionInput = Convert.ToBoolean(jsonObject["sensor_status"]["at_closed_position"].ToString());
                        bNearClosedPositionInput = Convert.ToBoolean(jsonObject["sensor_status"]["near_closed_position"].ToString());

                        //The Sky X Status
                        bTheSkyXMountConnected = Convert.ToBoolean(jsonObject["theskyx_status"]["mount_connected"].ToString());
                        bTheSkyXMountParked = Convert.ToBoolean(jsonObject["theskyx_status"]["mount_parked"].ToString());

                        //Update Shutter State
                        if (bRoofOpened == true)
                            domeShutterState = true;
                        else
                            domeShutterState = false;

                        //Set First Valid Message Received
                        if (bMessageReceived == false)
                            bMessageReceived = true;

                    }
                    catch (Exception ex)
                    {

                        return;
                    }
                    
                }
            }
        }

        private void TcpClientThread_DoWork(object sender, DoWorkEventArgs e)
        {
            const char STX = '\u0002';
            const char ETX = '\u0003';

            //Input TCP Buffer
            byte[] tcpBuffer = new byte[4096];

            //Temp Message - Only Grab STX & ETX Messages
            string sTempMessage = "";

            while (IsConnected == true) 
            {
                if (tcpClientROR != null && tcpClientROR.Connected == true && tcpStream.CanRead == true && tcpStream.DataAvailable == true)
                {
                    
                   // Incoming message may be larger than the buffer size.
                    do
                    {
                        //Read Available Data
                        int nBytesReceived = tcpStream.Read(tcpBuffer, 0, tcpBuffer.Length);

                        //Convert to String
                        sTempMessage += Encoding.ASCII.GetString(tcpBuffer, 0, nBytesReceived);

                    }
                    while (tcpStream.DataAvailable);

                    while (sTempMessage.Contains(STX.ToString()) && sTempMessage.Contains(ETX.ToString()))
                    {
                        int nFullStartIdx = sTempMessage.IndexOf(STX);
                        int nFullEndIdx = sTempMessage.IndexOf(ETX);
                        int nFullMessageLength = nFullEndIdx - nFullStartIdx + 1;

                        //Get Full Message w/ STX & ETX
                        string sFullMessage = sTempMessage.Substring(nFullStartIdx, nFullMessageLength);

                        //Trim STX & ETX
                        int nTrimStartIdx = sTempMessage.IndexOf(STX) + 1;
                        int nTrimEndIdx = sTempMessage.IndexOf(ETX) - 1;
                        int nTrimMessageLength = nTrimEndIdx - nTrimStartIdx + 1;

                        //Get Substring (Complete Message)
                        string sCompleteMessage = sTempMessage.Substring(nTrimStartIdx, nTrimMessageLength);

                        //Remove Processed Text
                        sTempMessage = sTempMessage.Remove(nFullStartIdx, nFullMessageLength);

                        //Post Update
                        tcpClientThread.ReportProgress(0, (Object)sCompleteMessage);
                    }

                    //Reset Temp Message
                    if (tcpStream.DataAvailable == false && sTempMessage != "")
                        sTempMessage = "";
                }

                Thread.Sleep(1);
            }
        }


        //
        // PUBLIC COM INTERFACE IDomeV2 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            // string retString = CommandString(command, raw); // Send the command and wait for the response
            // bool retBool = XXXXXXXXXXXXX; // Parse the returned string and create a boolean True / False value
            // return retBool; // Return the boolean value to the client

            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    try
                    {
                        //Connect to Arduino PLC
                        tcpClientROR.Connect(sIPAddress, nTCPPort);
                        tcpClientROR.ReceiveTimeout = 250;
                        tcpClientROR.SendTimeout = 250;

                        tcpStream = tcpClientROR.GetStream();
                        tcpStream.ReadTimeout = 250;

                        //Update Connected State
                        connectedState = tcpClientROR.Connected;

                        tcpClientThread.RunWorkerAsync();

                        //Wait
                        ascomUtil.WaitForMilliseconds(1000);

                        //Power Devices
                        byte[] bytePowerOn = System.Text.Encoding.UTF8.GetBytes("mount_power_on");
                        tcpStream.Write(bytePowerOn, 0, bytePowerOn.Length);

                        //Wait
                        ascomUtil.WaitForMilliseconds(1000);

                        bytePowerOn = System.Text.Encoding.UTF8.GetBytes("camera_power_on");
                        tcpStream.Write(bytePowerOn, 0, bytePowerOn.Length);

                        //Wait
                        ascomUtil.WaitForMilliseconds(1000);

                        bytePowerOn = System.Text.Encoding.UTF8.GetBytes("heater_power_on");
                        tcpStream.Write(bytePowerOn, 0, bytePowerOn.Length);

                        //Wait
                        ascomUtil.WaitForMilliseconds(1000);

                    }
                    catch { connectedState = false; };

                    if (fSetup == null)
                        fSetup = new SetupDialogForm(tl);
                    fSetup.Show();

                    LogMessage("Connected Set", "Connecting to IP Address: {0}", sIPAddress);
                }
                else
                {
                    if (fSetup != null)
                        fSetup.Hide();


                    //Close Stream
                    tcpStream.Close(1000);

                    //Close Port
                    if (tcpClientROR.Connected)
                        tcpClientROR.Close();

                    connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from IP Address {0}", sIPAddress);
                    // TODO disconnect from the device
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        public string Name
        {
            get
            {
                string name = "Short driver name - please customise";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region IDome Implementation

        private bool domeShutterState = false; // Variable to hold the open/closed status of the shutter, true = Open

        public void AbortSlew()
        {
            //Check if Connected to ROR Controller
            if (tcpClientROR != null && tcpClientROR.Connected == true)
            {
                //Send Open Roof Command
                byte[] byteStopRoof = System.Text.Encoding.UTF8.GetBytes("stop_roof");
                tcpStream.Write(byteStopRoof, 0, byteStopRoof.Length);
            }

            // This is a mandatory parameter but we have no action to take in this simple driver
            tl.LogMessage("AbortSlew", "Completed");
        }

        public double Altitude
        {
            get
            {
                tl.LogMessage("Altitude Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public bool AtHome
        {
            get
            {
                tl.LogMessage("AtHome Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("AtHome", false);
            }
        }

        public bool AtPark
        {
            get
            {
                tl.LogMessage("AtPark Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("AtPark", false);
            }
        }

        public double Azimuth
        {
            get
            {
                tl.LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                tl.LogMessage("CanFindHome Get", false.ToString());
                return false;
            }
        }

        public bool CanPark
        {
            get
            {
                tl.LogMessage("CanPark Get", false.ToString());
                return false;
            }
        }

        public bool CanSetAltitude
        {
            get
            {
                tl.LogMessage("CanSetAltitude Get", false.ToString());
                return false;
            }
        }

        public bool CanSetAzimuth
        {
            get
            {
                tl.LogMessage("CanSetAzimuth Get", false.ToString());
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                tl.LogMessage("CanSetPark Get", false.ToString());
                return false;
            }
        }

        public bool CanSetShutter
        {
            get
            {
                tl.LogMessage("CanSetShutter Get", true.ToString());
                return true;
            }
        }

        public bool CanSlave
        {
            get
            {
                tl.LogMessage("CanSlave Get", false.ToString());
                return false;
            }
        }

        public bool CanSyncAzimuth
        {
            get
            {
                tl.LogMessage("CanSyncAzimuth Get", false.ToString());
                return false;
            }
        }

        public void CloseShutter()
        {
            //Check if Mount Status Known
            if (bTheSkyXMountConnected == false || bTheSkyXMountParked == false)
            {
                tl.LogMessage("OpenShutter", "Mount is not connected, or Parked");
                return;
            }

            //Check if Connected to ROR Controller
            if (tcpClientROR != null && tcpClientROR.Connected == true)
            {
                //Send Close Roof Command
                byte[] byteCloseRoof = System.Text.Encoding.UTF8.GetBytes("close_roof");
                tcpStream.Write(byteCloseRoof, 0, byteCloseRoof.Length);

                //Wait for Roof to Start Closing
                int nTimeout = 0;
                while (++nTimeout < nCommandTimeout)
                {
                    //Check if Closing
                    if (bRoofOpened == true || bRoofOpening == true)
                    {
                        tl.LogMessage("CloseShutter", "Roof is closing");
                        break;
                    }

                    //Wait
                    ascomUtil.WaitForMilliseconds(50);
                }

                //Wait for Roof to Completely Close
                nTimeout = 0;
                while (++nTimeout < nOpenCloseTimeout)
                {
                    //Check if Opening
                    if (bRoofClosed == true)
                        break;

                    //Wait
                    ascomUtil.WaitForMilliseconds(50);
                }

                if (bRoofClosed == true)
                {
                    tl.LogMessage("CloseShutter", "Roof has been closed");
                    domeShutterState = false;
                }
                else
                {
                    tl.LogMessage("CloseShutter", "Close Roof Failed");
                    domeShutterState = false;
                }

            }
            else
            {
                tl.LogMessage("OpenShutter", "Close Roof Failed - No Connection");
                domeShutterState = false;
            }
        }

        public void FindHome()
        {
            tl.LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public void OpenShutter()
        {
            //Check if Mount Status Known
            if (bTheSkyXMountConnected == false || bTheSkyXMountParked == false)
            {
                tl.LogMessage("OpenShutter", "Mount is not connected, or Parked");
                return;
            }

            //Check if Connected to ROR Controller
            if (tcpClientROR != null && tcpClientROR.Connected == true)
            {
                //Send Open Roof Command
                byte[] byteOpenRoof = System.Text.Encoding.UTF8.GetBytes("open_roof");
                tcpStream.Write(byteOpenRoof, 0, byteOpenRoof.Length);

                //Wait for Roof to Start Opening
                int nTimeout = 0;
                while (++nTimeout < nCommandTimeout)
                {
                    //Check if Opening
                    if (bRoofOpened == true || bRoofOpening == true)
                    {
                        tl.LogMessage("OpenShutter", "Roof is opening");
                        break;
                    }

                    //Wait
                    ascomUtil.WaitForMilliseconds(50);
                }

                //Wait for Roof to Completely Open
                nTimeout = 0;
                while (++nTimeout < nOpenCloseTimeout)
                {
                    //Check if Opening
                    if (bRoofOpened == true)
                        break;

                    //Wait
                    ascomUtil.WaitForMilliseconds(50);
                }

                if (bRoofOpened == true)
                {
                    tl.LogMessage("OpenShutter", "Roof has been opened");
                    domeShutterState = true;
                }
                else
                {
                    tl.LogMessage("OpenShutter", "Open Roof Failed");
                    domeShutterState = false;
                }

            }
            else
            {
                tl.LogMessage("OpenShutter", "Open Roof Failed - No Connection");
                domeShutterState = false;
            }

        }

        public void Park()
        {
            tl.LogMessage("Park", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Park");
        }

        public void SetPark()
        {
            tl.LogMessage("SetPark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SetPark");
        }

        public ShutterState ShutterStatus
        {
            get
            {
                tl.LogMessage("ShutterStatus Get", false.ToString());
                if (domeShutterState)
                {
                    tl.LogMessage("ShutterStatus", ShutterState.shutterOpen.ToString());
                    return ShutterState.shutterOpen;
                }
                else
                {
                    tl.LogMessage("ShutterStatus", ShutterState.shutterClosed.ToString());
                    return ShutterState.shutterClosed;
                }
            }
        }

        public bool Slaved
        {
            get
            {
                tl.LogMessage("Slaved Get", false.ToString());
                return false;
            }
            set
            {
                tl.LogMessage("Slaved Set", "not implemented");
                throw new ASCOM.PropertyNotImplementedException("Slaved", true);
            }
        }

        public void SlewToAltitude(double Altitude)
        {
            tl.LogMessage("SlewToAltitude", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltitude");
        }

        public void SlewToAzimuth(double Azimuth)
        {
            tl.LogMessage("SlewToAzimuth", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAzimuth");
        }

        public bool Slewing
        {
            get
            {
                tl.LogMessage("Slewing Get", false.ToString());
                return false;
            }
        }

        public void SyncToAzimuth(double Azimuth)
        {
            tl.LogMessage("SyncToAzimuth", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAzimuth");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Dome";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                sIPAddress = driverProfile.GetValue(driverID, "IP_Address", string.Empty, "192.168.1.200");
                nTCPPort = Convert.ToInt32(driverProfile.GetValue(driverID, "TCP_Port", string.Empty, "5000"));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, "IP_Address", sIPAddress.ToString());
                driverProfile.WriteValue(driverID, "TCP_Port", nTCPPort.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion
    }
}
