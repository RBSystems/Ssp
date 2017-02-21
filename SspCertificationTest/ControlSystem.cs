using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.Gateways;
using Crestron.SimplSharpPro.Lighting;
using SspCertificationTest.Configuration;               // JSON parsing helpers for system setup
using SspCertificationTest.AvSwitchControl;
using Crestron.ThirdPartyCommon.Interfaces;
using System.Collections.Generic;
using ProTransports;
using SspCompanyVideoDisplayCom;
using SspCompanyVideoDisplayEthernet;
using SspCertificationTest.Utilities;
using System.Text.RegularExpressions;

namespace SspCertificationTest
{
    public class ControlSystem : CrestronControlSystem
    {
        public const string CONFIG_PATH = @"\NVRAM\SystemConfig.json";
        public AvSwitchController AvMatrix;
        public List<IBasicVideoDisplay> Displays;

        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);

                CrestronConsole.AddNewConsoleCommand(DoWork, "DoWork", "Run test method", ConsoleAccessLevelEnum.AccessOperator);
                Displays = new List<IBasicVideoDisplay>();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }

        void DoWork(string args)
        {
            try
            {
                TestSshClient();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in DoWork: {0} -- {1}", e.Message, e.StackTrace);
            }
        }

        #region Test Methods
        private void TestAvSwitch()
        {
            string rawConfig = JsonHelper.ReadJsonData(@"\NVRAM\SystemConfig.json");
            SystemConfiguration configData = JsonHelper.ParseConfigJson(rawConfig);

            AvSwitchController avSwitch = new AvSwitchController(configData.AvSwitcher, this);

            if (avSwitch.Initialize())
            {
                CrestronConsole.PrintLine("avSwitch successfully initialized.");
                avSwitch.DeviceOnlineStatusEvent += new EventHandler<SspCertificationTest.Utilities.GenericEventArgs<bool>>(avSwitch_DeviceOnlineStatusEvent);
                avSwitch.AudioOutputSourceChangeEvent += new EventHandler<SspCertificationTest.Utilities.GenericEventArgs<uint, uint>>(avSwitch_AudioOutputSourceChangeEvent);
                avSwitch.VideoOutputSourceChangeEvent += new EventHandler<SspCertificationTest.Utilities.GenericEventArgs<uint, uint>>(avSwitch_VideoOutputSourceChangeEvent);
                avSwitch.InputVideoSyncEvent += new EventHandler<SspCertificationTest.Utilities.GenericEventArgs<uint, bool>>(avSwitch_InputVideoSyncEvent);

                CrestronConsole.PrintLine("Result of build:");
                CrestronConsole.PrintLine("Switch type: {0}", avSwitch.SwitchType);
                CrestronConsole.PrintLine("Number of inputs: {0}", avSwitch.NumInputs);
                CrestronConsole.PrintLine("Number of outputs: {0}", avSwitch.NumOutputs);
                CrestronConsole.PrintLine("Online status: {0}", avSwitch.Online);
                CrestronConsole.PrintLine("GUID: {0}", avSwitch.GUID);

                CrestronCollection<ComPort> rmcCom = avSwitch.GetEndpointComports(1);
                CrestronConsole.PrintLine("Output 1 com ports:");
                foreach (var c in rmcCom)
                {
                    CrestronConsole.PrintLine("\tIs registered: {0}", c.Registered);
                    CrestronConsole.PrintLine("\tBaud: {0}", c.BaudRate);
                    CrestronConsole.PrintLine("\tProtocol: {0}", c.Protocol);
                }

                CrestronConsole.PrintLine(string.Empty);
                avSwitch.Route(1, 1);
                CrestronConsole.PrintLine("result of route(): out {0} -- A{1} V{2}", avSwitch.GetCurrentAudioRoute(1), avSwitch.GetCurrentAudioRoute(1));
            }
        }

        private void TestComDisplay()
        {
            CrestronConsole.PrintLine("Running RS-232 display tests...");

            // Check display connection point. If 0 -> control system, otherwise AV Switcher
            ComPort displayPort = this.ComPorts[1];

            // if comport is not registered, register it
            if (!displayPort.Registered)
            {
                if (displayPort.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failed to register comport {0} -- {1}", 1, displayPort.DeviceRegistrationFailureReason);
                }
            }

            // Create new SerialTransport object passing target ComPort as constructor argument
            SerialTransport transport = new SerialTransport(displayPort);
            SspCompanyVideoDisplayComport d = new SspCompanyVideoDisplayComport();
            transport.SetComPortSpec(((ISerialComport)d).ComSpec);
            ((ISerialComport)d).Initialize(transport);
            d.StateChangeEvent += new Action<DisplayStateObjects, IBasicVideoDisplay, byte>(StateChangeEvent);
            Displays.Add(d);

            ((ISerialComport)d).Connect();
            CrestronConsole.PrintLine("Connection status: {0}", d.Connected);
            d.SetVolume(10);
            d.SetInputSource(Crestron.ThirdPartyCommon.Class.VideoConnections.Hdmi1);
            CrestronConsole.PrintLine("Connection status: {0}", d.Connected);
            
        }

        private void TestIpDisplay()
        {
            IPAddress addr = IPAddress.Parse("127.0.0.2");
            int port = 49175;

            SspCompanyVideoDisplayTcp disp = new SspCompanyVideoDisplayTcp();
            disp.Initialize(addr, port);
            disp.RxOut += new Action<string>(disp_RxOut);
            disp.StateChangeEvent += new Action<DisplayStateObjects, IBasicVideoDisplay, byte>(StateChangeEvent);

            disp.Connect();
            disp.PowerOn();
        }

        private void TestSshClient()
        {
            SshClientControl client = new SshClientControl();
            client.RxEvent += client_RxEvent;
            if (client.Connect("r-a104d-dmps3.mesa.gmu.edu", 22, "admin", "Mason1972"))
            {
                CrestronConsole.PrintLine("Connected!");
                client.SendCommand("ver -v\r\n");
            }
        }

        void client_RxEvent(object sender, GenericEventArgs<string> e)
        {
            CrestronConsole.PrintLine(e.Value);
            ((SshClientControl)sender).Disconnect();
        }
        #endregion

        #region Event Handlers
        void avSwitch_InputVideoSyncEvent(object sender, SspCertificationTest.Utilities.GenericEventArgs<uint, bool> e)
        {
            CrestronConsole.PrintLine("Input video sync event: {0} - {1}", e.Target, e.Value);
        }

        void avSwitch_VideoOutputSourceChangeEvent(object sender, SspCertificationTest.Utilities.GenericEventArgs<uint, uint> e)
        {
            CrestronConsole.PrintLine("Input output change event: {0} - {1}", e.Target, e.Value);
        }

        void avSwitch_AudioOutputSourceChangeEvent(object sender, SspCertificationTest.Utilities.GenericEventArgs<uint, uint> e)
        {
            CrestronConsole.PrintLine("audio output change event: {0} - {1}", e.Target, e.Value);
        }

        void avSwitch_DeviceOnlineStatusEvent(object sender, SspCertificationTest.Utilities.GenericEventArgs<bool> e)
        {
            CrestronConsole.PrintLine("Device online/offline event: {0}", e.Value);
        }
        
        void StateChangeEvent(DisplayStateObjects arg1, IBasicVideoDisplay arg2, byte arg3)
        {
            switch (arg1)
            {
                case DisplayStateObjects.Audio:
                    CrestronConsole.PrintLine("Audio RX Event.");
                    break;
                case DisplayStateObjects.Connection:
                    CrestronConsole.PrintLine("Connection RX Event.");
                    break;
                case DisplayStateObjects.Input:
                    CrestronConsole.PrintLine("input RX Event.");
                    break;
                case DisplayStateObjects.LampHours:
                    CrestronConsole.PrintLine("lamp hour RX Event.");
                    break;
                case DisplayStateObjects.Power:
                    CrestronConsole.PrintLine("power RX Event.");
                    break;
                default:
                    CrestronConsole.PrintLine("unknown RX Event.");
                    break;
            }
        }

        void disp_RxOut(string obj)
        {
            CrestronConsole.PrintLine("RX Out: {0}", obj);
        }
        #endregion
    }
}