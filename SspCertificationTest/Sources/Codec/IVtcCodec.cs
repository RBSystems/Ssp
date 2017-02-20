using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Sources.Codec
{
    public interface IVtcCodec
    {
        public eCallState CallState { get; }
        public bool PrivacyOn { get; set; }
        public string Make { get; }
        public string Model { get; }

        public bool RecallPreset(int presetNum);
        public void EnterRemoteNumber(string host);
        public void BeginCall();
        public void HangUp();
        public void RespondToIncomingCall(bool response);
        public void AdjustCamera(eDpadButton cmd);
    }
}