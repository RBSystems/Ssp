using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Sources.Codec
{
    /// <summary>
    /// Identifies the current state of the VTC Codec device.
    /// </summary>
    public enum eCallState
    {
        Idle,
        InCall,
        Dialing,
        IncommingCall,
        NotConnected,
        HangingUp
    }

    /// <summary>
    /// Currently supported commands recognized by the VTC Codec
    /// </summary>
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