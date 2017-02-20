using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Display;
using Crestron.ThirdPartyCommon.Class;

namespace SspCompanyVideoDisplayEthernet
{
    public class SspCompanyVideoDisplay : ABasicVideoDisplay
    {
        public SspCompanyVideoDisplay()
        {
            InputDetails.Add(new InputDetail(VideoConnections.Hdmi1, VideoConnectionTypes.Hdmi, "HDMI 1"));
            InputDetails.Add(new InputDetail(VideoConnections.Hdmi2, VideoConnectionTypes.Hdmi, "HDMI 2"));
            DataFile = DataFileString.Json;
        }

        public override string Description { get { return "Dynamic display driver implementation for SimplSharpPro certification test."; } }
        public override Guid Guid { get { return new Guid(); } }
        public override string Manufacturer { get { return "SSP Company"; } }
        public override string Model { get { return "VideoDisplay"; } }
        public override string Version { get { return "1.0.0.0.0"; } }
        public override DateTime VersionDate { get { return new DateTime(2017, 2, 16); } }
        public override bool SupportsFeedback { get { return true; } }
        public override bool SupportsDisconnect { get { return false; } }
        public override bool SupportsReconnect { get { return false; } }
        public override bool SupportsChangeVolume { get { return true; } }
        public override bool SupportsSetVolume { get { return true; } }
        public override bool SupportsVolumePercentFeedback { get { return true; } }
        public override bool SupportsMuteFeedback { get { return true; } }
        public override bool SupportsDiscreteMute { get { return true; } }
        public override bool SupportsTogglePower { get { return true; } }
        public override bool SupportsDiscretePower { get { return true; } }
        public override bool SupportsPowerFeedback { get { return true; } }
        public override bool SupportsInputFeedback { get { return true; } }
        public override bool SupportsSetInputSource { get { return true; } }
        public override bool SupportsLampHours { get { return true; } }
        public override bool SupportsCoolDownTime { get { return true; } }
        public override bool SupportsWarmUpTime { get { return true; } }
    }
}