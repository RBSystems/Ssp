using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SspCertificationTest.Configuration;
using SspCertificationTest.Utilities;

namespace SspCertificationTest.Sources.Codec
{
    /// <summary>
    /// SSH implementation of the IVtcCodec interface. Uses an SSH client to connect to the hardware.
    /// NOTE: the Initialize() method must be run before any methods can be called.
    /// </summary>
    public class SshVtcCodec : IVtcCodec
    {
        /// <summary>
        /// Triggered when the call state of the codec changes, either from an incoming call
        /// or when the user begins a call.
        /// </summary>
        public event EventHandler<GenericEventArgs<eCallState>> CallEvent;
        /// <summary>
        /// Triggered when the internal SSH client connects or disconnects from the codec device
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> DeviceOnlineEvent;

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
        /// TRUE = user has run the Initialize() method. False otherwise
        /// </summary>
        public bool IsInitialized { get; private set; }

        private SshClientControl clientControl;
        private Source codecData;
        private string numberToDial;
        private eCallState currentState;

        /// <summary>
        /// Assign internal properties to default values and prepare for initialization.
        /// </summary>
        /// <param name="codecData">the data object created when parsing a JSON config file.</param>
        public SshVtcCodec(Source codecData)
        {
            this.codecData = codecData;
            Model = codecData.Name;
            Make = codecData.Type;
            GUID = new Guid(codecData.GuId);
            numberToDial = string.Empty;
            IsInitialized = false;
        }

        public bool Initialize(uint port, string hostname, string username, string password)
        {
            //TODO set up client connection and attempt to connect to the codec
            return false;
        }

        public bool RecallPreset(int presetNum)
        {
            //TODO Recall the requested preset
            return false;
        }

        public void EnterRemoteNumber(string host)
        {
            //TODO Save number to global variable
        }

        public void BeginCall()
        {
            //TODO If there is a number to dial, attempt to call that remote site
        }

        public void HangUp()
        {
            //TODO end call if one is currently active
        }

        public void RespondToIncomingCall(bool response)
        {
            //TODO accept/reject a call if it is incoming
        }

        public void AdjustCamera(eDpadButton cmd)
        {
            //TODO Send command to device
            switch (cmd)
            {
                case eDpadButton.Unused:
                    break;
                case eDpadButton.Up:
                    break;
                case eDpadButton.Down:
                    break;
                case eDpadButton.Left:
                    break;
                case eDpadButton.Right:
                    break;
                case eDpadButton.Center:
                    break;
                case eDpadButton.ZoomIn:
                    break;
                case eDpadButton.ZoomOut:
                    break;
                default:
                    break;
            }
        }

        public void Connect()
        {
            //TODO Connect to codec device
        }

        public void Disconnect()
        {
            //TODO Disconnect from codec device
        }

        public void Dispose()
        {
            //Properly dispose of all disposable objects (SssClientControl)
        }
    }
}