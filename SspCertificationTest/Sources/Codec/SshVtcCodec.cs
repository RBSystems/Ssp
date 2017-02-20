using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SspCertificationTest.Configuration;
using SspCertificationTest.Utilities;

namespace SspCertificationTest.Sources.Codec
{
    public class SshVtcCodec : IVtcCodec
    {
        public event EventHandler<GenericEventArgs<eCallState>> CallEvent;
        public event EventHandler<GenericEventArgs<bool>> DeviceOnlineEvent;

        public eCallState CallState { get; private set; }
        public bool PrivacyOn { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool Connected { get; private set; }
        public Guid GUID { get; private set; }
        public bool IsInitialized { get; private set; }

        private SshClientControl clientControl;
        private Source codecData;
        private string numberToDial;
        private eCallState currentState;

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