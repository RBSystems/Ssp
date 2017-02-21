using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SspCertificationTest.Configuration;
using SspCertificationTest.Utilities;
using System.Text.RegularExpressions;

namespace SspCertificationTest.Sources.Codec
{
    /// <summary>
    /// SSH implementation of the IVtcCodec interface. Uses an SSH client to connect to the hardware.
    /// NOTE: the Initialize() method must be run before any methods can be called.
    /// </summary>
    public class SshVtcCodec : IVtcCodec, IDisposable
    {
        #region Events
        /// <summary>
        /// Triggered when the call state of the codec changes, either from an incoming call
        /// or when the user begins a call.
        /// </summary>
        public event EventHandler<GenericEventArgs<eCallState>> CallEvent;
        /// <summary>
        /// Triggered when the internal SSH client connects or disconnects from the codec device
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> DeviceOnlineEvent;
        #endregion

        #region Public Properties
        /// <summary>
        /// The current activity state of the codec.
        /// </summary>
        public eCallState CallState { get; private set; }
        /// <summary>
        /// TRUE = local mic mute, FALSE = local mic active
        /// </summary>
        public bool PrivacyOn { get; set; }
        /// <summary>
        /// The manufacturer's company name
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Device model number/name
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// TRUE = SSH client is connected to device, FALSE = disconnected
        /// </summary>
        public bool Connected { get; private set; }
        /// <summary>
        /// The unique fuision ID used to remotely manage the device
        /// </summary>
        public Guid GUID { get; private set; }
        /// <summary>
        /// The current device state. If IDLE then the device is ready to receive or make a call.
        /// </summary>
        public eCallState CurrentState { get; private set; }
        #endregion

        private SshClientControl clientControl; // Communication with hardware
        private Source codecData;               // Codec information from JSON file
        private string numberToDial;            // Accessed when dialing remote site
        private eCallState previousState;       // The state the system was in before current state
        private bool disposed;                  // disposal management flag

        /// <summary>
        /// Assign internal properties to default values and prepare for initialization.
        /// </summary>
        /// <param name="codecData">the data object created when parsing a JSON config file.</param>
        public SshVtcCodec(Source codecData)
        {
            disposed = false;
            this.codecData = codecData;
            Model = codecData.Name;
            Manufacturer = codecData.Type;
            GUID = new Guid(codecData.GuId);
            numberToDial = string.Empty;
            CurrentState = eCallState.NotConnected;
            previousState = eCallState.NotConnected;
            clientControl = new SshClientControl();
            clientControl.RxEvent += SshClientHandler;
        }

        ~SshVtcCodec()
        {
            Dispose(false);
        }

        #region Public Methods
        /// <summary>
        /// Connect or Reconnect to the codec device if not currently connected.
        /// </summary>
        public void Connect()
        {
            if (!Connected)
            {
                Connected = clientControl.Connect(codecData.ComHostname, codecData.Port, codecData.ComUsername, codecData.ComPassword);
                if (Connected)
                {
                    ChangeState(eCallState.Idle);
                }
            }
        }

        /// <summary>
        /// Sends the recall preset command to the device for the requested preset.
        /// </summary>
        /// <param name="presetNum">The target preset number to recall</param>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void RecallPreset(int presetNum)
        {
            if (Connected)
            {
                clientControl.SendCommand(String.Format("vtc camera preset {0}\u000D\u000A", presetNum));
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// Save the current camera settings to the target preset number.
        /// </summary>
        /// <param name="presetNum">The preset numb to save the current camera position to.</param>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void SavePreset(int presetNum)
        {
            if (Connected)
            {
                clientControl.SendCommand(String.Format("vtc camera store preset {0}\u000D\u000A", presetNum));
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// Stores the given hostname or number that will be called by the BeginCall() method.
        /// </summary>
        /// <param name="host">the number, IP Address, or hostname of the target remote site.</param>
        /// <exception cref="InvalidOperationException">If the device is not yet initialized.</exception>
        /// <exception cref="ArgumentException">If "Host" is null or an empty string</exception>
        public void EnterRemoteNumber(string host)
        {
            if (host == null || host == string.Empty) throw new ArgumentException("Argument 'host' cannot be null or empty.");
            if (Connected)
            {
                numberToDial = host;
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// Attemp to connect to the remote site. This will trigger a state change event.
        /// If there is no response after 10 seconds then the system will cancel the call attempt.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void BeginCall()
        {
            if (numberToDial == null || numberToDial == string.Empty) throw new InvalidOperationException("Remote host information not yet saved. Use EnterRemoteNumber() to save remote connection.");
            if (Connected)
            {
                clientControl.SendCommand(String.Format("vtc dial {0}\u000D\u000A", numberToDial));
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// End the current call session if one is active. This will trigger a state change
        /// event if successful.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void HangUp()
        {
            if (Connected)
            {
                if (CurrentState == eCallState.InCall)
                {
                    clientControl.SendCommand("vtc hangup\u000D\u000A");
                }
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// Accept or reject an incomming call.
        /// </summary>
        /// <param name="response">TRUE = accept call, FALSE = reject call</param>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void RespondToIncomingCall(bool response)
        {
            if (Connected)
            {
                if (CurrentState == eCallState.IncomingCall && response)
                {
                    clientControl.SendCommand("vtc answer\u000D\u000A");
                }
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// send a 1-time camera control command to the device. This will not result in a continuous adjustment.
        /// </summary>
        /// <param name="cmd">The camera control action to perform</param>
        /// <exception cref="InvalidOperationException">If the device is not yet connected.</exception>
        public void AdjustCamera(eDpadButton cmd)
        {
            if (Connected)
            {
                switch (cmd)
                {
                    case eDpadButton.Up:
                        clientControl.SendCommand("vtc camera pan up\u000D\u000A");
                        break;
                    case eDpadButton.Down:
                        clientControl.SendCommand("vtc camera pan down\u000D\u000A");
                        break;
                    case eDpadButton.Left:
                        clientControl.SendCommand("vtc camera pan left\u000D\u000A");
                        break;
                    case eDpadButton.Right:
                        clientControl.SendCommand("vtc camera pan right\u000D\u000A");
                        break;
                    case eDpadButton.ZoomIn:
                        clientControl.SendCommand("vtc camera zoom in\u000D\u000A");
                        break;
                    case eDpadButton.ZoomOut:
                        clientControl.SendCommand("vtc camera zoom out\u000D\u000A");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                throw new InvalidOperationException("Codec device not connected.");
            }
        }

        /// <summary>
        /// Disconnect from the codec device if currently connected
        /// </summary>
        public void Disconnect()
        {
            if (Connected)
                clientControl.Disconnect();
        }

        /// <summary>
        /// Mark any internally dispoable objects for garbage collection and free all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Internal worker methods
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            clientControl.Disconnect();

            if (disposing)
            {
                clientControl.Dispose();
            }
            disposed = true;
        }

        private void ChangeState(eCallState newState)
        {
            if (newState != CurrentState)
            {
                previousState = CurrentState;
                CurrentState = newState;

                EventHandler<GenericEventArgs<eCallState>> temp = CallEvent;
                if (temp != null)
                    temp(this, new GenericEventArgs<eCallState>(CurrentState));
            }
        }

        private void SshClientHandler(object src, GenericEventArgs<string> args)
        {
            //TODO response to SSH Client notifications
            string incCallPattern = @"vtc ringing (?!(stopped)).+ fb";
            string stoppedCallPattern = @"vtc ringing stopped fb";
            string micMutePattern = @"vtc microphone privacy fb (?=(on|off))\w+";

            if (Regex.IsMatch(args.Value, incCallPattern))
            {
                string[] rx = args.Value.Split(' ');
                CrestronConsole.PrintLine("Incoming call from: {0}", rx[2]);
            }
            else if (Regex.IsMatch(args.Value, stoppedCallPattern))
            {
                CrestronConsole.PrintLine("Incoming call canceled.");
            }
            else if (Regex.IsMatch(args.Value, micMutePattern))
            {
                string[] rx = args.Value.Split(' ');
                CrestronConsole.PrintLine("Mic mute is {0}", rx[rx.Length - 1]);
            }
        }
        #endregion
    }
}