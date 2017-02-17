using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Display;
using Crestron.ThirdPartyCommon.Class;

namespace SspCompanyVideoDisplayTcp
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
        public override bool SupportsFeedback { get { return true; } }
        public override string Version { get { return "1.0.0.0.0"; } }
        public override DateTime VersionDate { get { return new DateTime(2017,2,16); } }
    }
}