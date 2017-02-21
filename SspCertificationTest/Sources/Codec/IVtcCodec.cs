using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Sources.Codec
{
    public interface IVtcCodec
    {
        eCallState CallState { get; }
        bool PrivacyOn { get; set; }
        string Manufacturer { get; }
        string Model { get; }

        void RecallPreset(int presetNum);
        void EnterRemoteNumber(string host);
        void BeginCall();
        void HangUp();
        void RespondToIncomingCall(bool response);
        void AdjustCamera(eDpadButton cmd);
    }
}