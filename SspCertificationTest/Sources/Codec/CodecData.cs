using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Sources.Codec
{
    public enum eCallState
    {
        Idle,
        InCall,
        Dialing,
        IncommingCall,
        NotConnected,
        HangingUp
    }

    public enum eCodecCommand
    {
        Unused,
        AnswerCall,
        HangUp,
        PrivacyOn,
        PrivacyOff,
        RecallPreset,
        AdjustCamera,
        DialNumber
    }
}