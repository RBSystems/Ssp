using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SspCertificationTest.Configuration;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharp.Reflection;
using SspCertificationTest.Utilities;
using Crestron.SimplSharpPro.DM.Cards;

namespace SspCertificationTest.AvSwitchControl
{
    /// <summary>
    /// Wrapper class that manages all AV switching functionality.
    /// </summary>
    public class AvSwitchController
    {
        #region Properties
        /// <summary>
        /// The IP-ID used for connecting with the control system.
        /// </summary>
        public uint IpId { get; private set;}

        /// <summary>
        /// Unique identifier used by fusion to manage this device
        /// </summary>
        public Guid GUID { get; private set;}

        /// <summary>
        /// IP-ID used by fusion
        /// </summary>
        public uint FusionIpId { get; private set; }

        /// <summary>
        /// Returns the fully qualified name of the AV Switching object that was defined in the configuration data package.
        /// </summary>
        public string SwitchType { get; private set; }
        
        /// <summary>
        /// The number of input channels defined in the configuration package.
        /// </summary>
        public uint NumInputs { get; private set; }

        /// <summary>
        /// The number of output channels defined in the configuration package.
        /// </summary>
        public uint NumOutputs { get; private set; }

        /// <summary>
        /// Indicates whether or not the underlying AV Switch is online. TRUE = device online, FALSE = device offline
        /// </summary>
        public bool Online
        {
            get
            {
                if (IsInitialized)
                    return avSwitch.IsOnline;
                else
                    throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Indicates that the AVSwitchController has been successfully configured.
        /// True = ready to respond to switching requests, False = All methods will end without running.
        /// </summary>
        public bool IsInitialized { get; private set; }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Event thrown when a video routing change is detected on the AV Switcher device.
        /// Argument package: Target = output, Value = input.
        /// </summary>
        public event EventHandler<GenericEventArgs<uint,uint>> VideoOutputSourceChangeEvent;

        /// <summary>
        /// Event thrown when an audio routing change is detected on the AV Switcher device.
        /// Argument package: Target = output, Value = input.
        /// </summary>
        public event EventHandler<GenericEventArgs<uint, uint>> AudioOutputSourceChangeEvent;

        /// <summary>
        /// Notifies subscribers when a change to the video sync of any input changes.
        /// Target = input index that changed, Value = true if sync detected, false if sync not detected.
        /// </summary>
        public event EventHandler<GenericEventArgs<uint, bool>> InputVideoSyncEvent;

        /// <summary>
        /// Event thrown when the AV Switch goes offline or comes online.
        /// Argument Package: Value = true when device online, false when device offline.
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> DeviceOnlineStatusEvent;
        #endregion

        private List<EndpointReceiverBase> RoomBoxes;   // Tracking the endpoints that have been assigned to the AV Switch
        private Switch avSwitch;                        // The Crestron switcher object representing the physical hardware
        private CrestronControlSystem master;           // The control system driving the given room
        private AvSwitcher configData;                  // JSON object created from the configuration file.

        /// <summary>
        /// Prepare controller for creating the AV Switcher hardware representation.
        /// </summary>
        /// <param name="avData">The switch information gathered by parsing the JSON config file</param>
        /// <param name="control">The control processor that will manage all devices</param>
        /// <exception cref="ArgumentException">if any parameter is given a null argument.</exception>
        public AvSwitchController(AvSwitcher avData, CrestronControlSystem control)
        {
            if (avData == null || control == null) throw new ArgumentException("Constructor arguments cannot be null.");
            master = control;
            configData = avData;
            RoomBoxes = new List<EndpointReceiverBase>();
        }

        #region Public Methods
        /// <summary>
        /// Runs the Switch construction based on the data supplied during instantiation.
        /// </summary>
        /// <returns>TRUE if initialization was successful, FALSE otherwise.</returns>
        public bool Initialize()
        {
            Assembly dll = Assembly.LoadFrom(configData.Library);
            if (dll != null)
            {
                // assign data to public properties
                GUID = new Guid(configData.GuId);
                FusionIpId = Convert.ToUInt32(configData.FusionIpId);
                SwitchType = configData.ClassName;
                IpId = (uint)configData.IpId;

                // Create defined AV Frame
                CType switchType = dll.GetType(configData.ClassName);
                ConstructorInfo ciSwitch = switchType.GetConstructor(new CType[] {typeof(UInt32),typeof(CrestronControlSystem)});
                object swInstance = ciSwitch.Invoke(new object[] {Convert.ToUInt32(configData.IpId), master });
                avSwitch = (Switch)swInstance;
                avSwitch.DMOutputChange += new DMOutputEventHandler(avSwitch_VideoDMOutputChange);
                avSwitch.OnlineStatusChange += new OnlineStatusChangeEventHandler(avSwitch_OnlineStatusChange);
                avSwitch.DMInputChange += new DMInputEventHandler(avSwitch_DMInputChange);

                if (configData.IsConfigurable)
                {
                    // Create all defined input cards
                    foreach (var card in configData.InputCards)
                    {
                        CType input = dll.GetType(card.ClassName);
                        ConstructorInfo cInfo = input.GetConstructor(new CType[] { typeof(UInt32), typeof(Switch) });
                        object cInstance = cInfo.Invoke(new object[] { Convert.ToUInt32(card.SlotNumber), avSwitch });
                    }

                    // Create all defined output cards
                    foreach (var card in configData.OutputCards)
                    {
                        CType output = dll.GetType(card.ClassName);
                        ConstructorInfo cInfo = output.GetConstructor(new CType[] {typeof(UInt32), typeof(Switch) });
                        object oInstance = cInfo.Invoke(new object[] {Convert.ToUInt32(card.SlotNumber), avSwitch });
                    }
                }

                NumInputs = (uint)avSwitch.NumberOfInputs;
                NumOutputs = (uint)avSwitch.NumberOfOutputs;

                // Add endpoints to given output channels
                foreach (RoomBox roomBox in configData.RoomBoxes)
                {
                    CType epType = dll.GetType(roomBox.ClassName);
                    ConstructorInfo cInfo = epType.GetConstructor(new CType[] { typeof(UInt32), typeof(DMOutput) });
                    RoomBoxes.Add((EndpointReceiverBase)cInfo.Invoke(new object[] { Convert.ToUInt32(roomBox.IpId), avSwitch.Outputs[Convert.ToUInt32(roomBox.OutputNumber)] }));
                }

                // Register Switcher
                if (avSwitch.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failed to register AV switch: {0}", avSwitch.RegistrationFailureReason);
                    return false;
                }

                // Register Endpoints
                foreach (var box in RoomBoxes)
                {
                    if (!box.Registered && box.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    {
                        ErrorLog.Error("Failed to register room box for input {0} - {1}", box.DMOutput.Number, box.RegistrationFailureReason);
                        return false;
                    }
                }
                IsInitialized = true;
                return true;
            }
            else
            {
                ErrorLog.Error("Failed to load assembly information.");
                return false;
            }
        }

        /// <summary>
        /// Get the ComPort collection from the endpoint at the given output channel.
        /// If there is no endpoint or if the endpoint does not support ComPorts then NULL is returned.
        /// </summary>
        /// <param name="output">The output channel connected to the target endpoint</param>
        /// <returns>a collection of ComPorts on that output or NULL if there is no endpoint or comports are not supported.</returns>
        /// <exception cref="ArgumentException">If "output" is greater than the number of output channels on the AV switch device.</exception>
        public CrestronCollection<ComPort> GetEndpointComports(uint output)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of output channels.");
            if (IsInitialized)
            {
                try
                {
                    GenericBase obj = avSwitch.Outputs[output].Endpoint;
                    CType endpointType = obj.GetType();
                    PropertyInfo endProp = endpointType.GetProperty("ComPorts");
                    return (CrestronCollection<ComPort>)endProp.GetValue(obj, new object[] { });
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Problem retreiving endpoint ComPort: {0} -- {1}", e.Message, e.StackTrace);
                    return null;
                }
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Route the target AV input to the given output channel (Audio and Video). Indexing starts at 1.
        /// </summary>
        /// <param name="input">The input that will be routed</param>
        /// <param name="output">The target output channel on the AV Switch</param>
        /// <exception cref="ArgumentException">If input > NumInputs or if output > NumOutputs</exception>"
        public void Route(uint input, uint output)
        {
            if (input > NumInputs || output > NumOutputs) throw new ArgumentException("Arguments input & output cannot be greater than the collection of AV input/output channels.");
            if (IsInitialized)
            {
                // Implement routing functionality
                avSwitch.Outputs[output].VideoOut = avSwitch.Inputs[input];
                avSwitch.Outputs[output].AudioOut = avSwitch.Inputs[input];
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Route the target video input to the given output channel. Indexing starts at 1.
        /// </summary>
        /// <param name="input">The input that will be routed</param>
        /// <param name="output">The target output channel on the AV Switch</param>
        /// <exception cref="ArgumentException">If input > NumInputs or if output > NumOutputs</exception>"
        public void RouteVideo(uint input, uint output)
        {
            if (input > NumInputs || output > NumOutputs) throw new ArgumentException("Arguments input & output cannot be greater than the collection of AV input/output channels.");
            if (IsInitialized)
            {
                // Implement routing functionality
                avSwitch.Outputs[output].VideoOut = avSwitch.Inputs[input];
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Route the target audio input to the given output channel. Indexing starts at 1.
        /// </summary>
        /// <param name="input">The input that will be routed</param>
        /// <param name="output">The target output channel on the AV Switch</param>
        /// <exception cref="ArgumentException">If input > NumInputs or if output > NumOutputs</exception>"
        public void RouteAudio(uint input, uint output)
        {
            if (input > NumInputs || output > NumOutputs) throw new ArgumentException("Arguments input & output cannot be greater than the collection of AV input/output channels.");
            if (IsInitialized)
            {
                // Implement routing functionality
                avSwitch.Outputs[output].AudioOut = avSwitch.Inputs[input];
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Remove any matrix routing on the AV Switch device
        /// </summary>
        public void ClearAllRoutes()
        {
            if (IsInitialized)
            {
                foreach (var output in avSwitch.Outputs)
                {
                    output.VideoOut = null;
                    output.AudioOut = null;
                }
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// retreive the current video input number that is routed to the target output.
        /// Arguments & return value are 1-indexed.
        /// </summary>
        /// <param name="output">The target output to get input routing from</param>
        /// <returns>The index of the routed video input, or 0 if no input is routed.</returns>
        /// <exception cref="ArgumentException">if output > NumOutputs</exception>"
        public uint GetCurrentVideoRoute(uint output)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of output channels.");
            if (IsInitialized)
            {
                // Return the value of current route
                return avSwitch.Outputs[output].VideoOutFeedback.Number;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            } 
        }

        /// <summary>
        /// retreive the current audio input number that is routed to the target output.
        /// Arguments & return value are 1-indexed.
        /// </summary>
        /// <param name="output">The target output to get input routing from</param>
        /// <returns>The index of the routed audio input, or 0 if no input is routed.</returns>
        /// <exception cref="ArgumentException">if output > NumOutputs</exception>"
        public uint GetCurrentAudioRoute(uint output)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of output channels.");
            if (IsInitialized)
            {
                // Return the value of current route
                return avSwitch.Outputs[output].AudioOutFeedback.Number;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }
        #endregion

        #region Internal handling of DM events
        private void avSwitch_VideoDMOutputChange(Switch device, DMOutputEventArgs args)
        {
            // report the changed output and the new input to the event handler
            EventHandler<GenericEventArgs<uint, uint>> temp = VideoOutputSourceChangeEvent;
            if (temp != null && args.EventId == DMOutputEventIds.VideoOutEventId)
            {
                temp(this, new GenericEventArgs<uint,uint>(args.Number, avSwitch.Outputs[args.Number].VideoOutFeedback.Number));
            }
        }

        private void avSwitch_AudioDMOutputChange(Switch device, DMOutputEventArgs args)
        {
            // report the changed output and the new input to the event handler
            EventHandler<GenericEventArgs<uint, uint>> temp = AudioOutputSourceChangeEvent;
            if (temp != null && args.EventId == DMOutputEventIds.AudioOutEventId)
            {
                temp(this, new GenericEventArgs<uint, uint>(args.Number, avSwitch.Outputs[args.Number].AudioOutFeedback.Number));
            }
        }

        private void avSwitch_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            // report online/offline status to event handler
            EventHandler<GenericEventArgs<bool>> temp = DeviceOnlineStatusEvent;
            if (temp != null)
            {
                temp(this, new GenericEventArgs<bool>(args.DeviceOnLine));
            }
        }

        private void avSwitch_DMInputChange(Switch device, DMInputEventArgs args)
        {
            // Report video sync to event handler
            EventHandler<GenericEventArgs<uint, bool>> temp = InputVideoSyncEvent;
            if (temp != null && args.EventId == DMInputEventIds.VideoDetectedEventId)
            {
                temp(this,new GenericEventArgs<uint,bool>(args.Number,avSwitch.Inputs[args.Number].VideoDetectedFeedback.BoolValue));
            }
        }
        #endregion
    }
}