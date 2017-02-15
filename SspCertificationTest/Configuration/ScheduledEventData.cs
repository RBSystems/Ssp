using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Configuration
{
    public class ScheduledEvent
    {
        public string EventName { get; set; }
        public List<bool> SelectedDays { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public bool IsAm { get; set; }
        public int PresetNumber { get; set; }
    }

    public class ScheduleEventData
    {
        public List<ScheduledEvent> ScheduledEvents { get; set; }
    }
}