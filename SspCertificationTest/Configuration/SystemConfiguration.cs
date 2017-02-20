using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Configuration
{

    /// <summary>
    /// Rood JSON model that contains all of the room configuration mapping.
    /// </summary>
    public class SystemConfiguration
    {
        public RoomInfo RoomInfo { get; set; }
        public AvSwitcher AvSwitcher { get; set; }
        public List<Display> Displays { get; set; }
        public List<Source> Sources { get; set; }
        public List<Touchscreen> Touchscreens { get; set; }
        public List<RfGateway> RfGateways { get; set; }
        public List<LightingDevice> LightingDevices { get; set; }
        public List<LightingPreset> LightingPresets { get; set; }
    }

    /// <summary>
    /// General room information for the given system.
    /// </summary>
    public class RoomInfo
    {
        public string RoomName { get; set; }
        public int GuId { get; set; }
        public int FusionIpId { get; set; }
        public int Capacity { get; set; }
    }

    /// <summary>
    /// Data to configure an input slot on an AV Switch frame.
    /// </summary>
    public class InputCard
    {
        public string ClassName { get; set; }
        public int SlotNumber { get; set; }
    }

    /// <summary>
    /// Data used to configure an output slot on an AV Switch frame.
    /// </summary>
    public class OutputCard
    {
        public string ClassName { get; set; }
        public int SlotNumber { get; set; }
    }

    /// <summary>
    /// Data to map a room box to a given AV Switcher output.
    /// </summary>
    public class RoomBox
    {
        public int IpId { get; set; }
        public int OutputNumber { get; set; }
        public string ClassName { get; set; }
    }

    /// <summary>
    /// Defines the AV switcher for the given system. Contains card configuration data if the 
    /// switcher is a configurable frame.
    /// </summary>
    public class AvSwitcher
    {
        public string Library { get; set; }
        public string ClassName { get; set; }
        public int IpId { get; set; }
        public int FusionIpId { get; set; }
        public string GuId { get; set; }
        public bool IsConfigurable { get; set; }
        public List<InputCard> InputCards { get; set; }
        public List<OutputCard> OutputCards { get; set; }
        public List<RoomBox> RoomBoxes { get; set; }
    }

    /// <summary>
    /// Data used to create and map display hardware.
    /// </summary>
    public class Display
    {
        public string Name { get; set; }
        public string Library { get; set; }
        public string ClassName { get; set; }
        public string ComProtocol { get; set; }
        public int ComConnectionPoint { get; set; }
        public int Port { get; set; }
        public int SwitcherOutput { get; set; }
        public bool VolumeControl { get; set; }
    }

    /// <summary>
    /// Used to define an input source.
    /// </summary>
    public class Source
    {
        public string Name { get; set; }
        public string Library { get; set; }
        public string Type { get; set; }
        public string ClassName { get; set; }
        public int SwitcherInput { get; set; }
        public string GuId { get; set; }
    }

    /// <summary>
    /// Defines a user interface option that is used to control the system.
    /// </summary>
    public class Touchscreen
    {
        public string Name { get; set; }
        public string Library { get; set; }
        public string ClassName { get; set; }
        public int IpId { get; set; }
    }

    /// <summary>
    /// RF Gateway data if any exist in the system.
    /// </summary>
    public class RfGateway
    {
        public string Library { get; set; }
        public string ClassName { get; set; }
        public int IpId { get; set; }
    }

    /// <summary>
    /// Data for mapping any lighting units present in the system.
    /// </summary>
    public class LightingDevice
    {
        public string Name { get; set; }
        public string Library { get; set; }
        public string ClassName { get; set; }
        public int RfId { get; set; }
        public int GuId { get; set; }
    }

    /// <summary>
    /// A Load preset object that will be recalled. Maps to a lighting uint via the DeviceName property.
    /// </summary>
    public class Load
    {
        public string DeviceName { get; set; }
        public int LoadNumber { get; set; }
        public int Level { get; set; }
    }

    /// <summary>
    /// Defines a collection of hardware load settings to be recalled later
    /// </summary>
    public class LightingPreset
    {
        public string Name { get; set; }
        public List<Load> Load { get; set; }
    }
}