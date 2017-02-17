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
        /// <summary>
        /// The IP-ID used for connecting with the control system.
        /// </summary>
        public uint IpId { get; set;}

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
        /// Indicates that the AVSwitchController has been successfully configured.
        /// True = ready to respond to switching requests, False = All methods will end without running.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Event thrown when a routing change is detected on the AV Switcher device.
        /// Argument package: Target = output, Value = input.
        /// </summary>
        public event EventHandler<GenericEventArgs<uint,uint>> SourceChangeEvent;

        /// <summary>
        /// Event thrown when the AV Switch goes offline or comes online.
        /// Argument Package: Value = true when device online, false when device offline.
        /// </summary>
        public event EventHandler<GenericEventArgs<bool>> DeviceOnlineStatusEvent;

        private List<EndpointReceiverBase> RoomBoxes;   // Tracking the endpoints that have been assigned to the AV Switch
        private Switch avSwitch;                        // The Crestron switcher object representing the physical hardware
        private CrestronControlSystem master;           // The control system driving the given room
        private AvSwitcher configData;                  // JSON object defined in the configuration file.

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

        /// <summary>
        /// Runs the Switch construction based on the data supplied during instantiation.
        /// </summary>
        /// <returns>TRUE if initialization was successful, FALSE otherwise.</returns>
        public bool Initialize()
        {
            Assembly dll = Assembly.LoadFrom(configData.Library);
            if (dll != null)
            {
                //List<CardDevice> inputCards = new List<CardDevice>();
                //List<CardDevice> outputCards = new List<CardDevice>();

                // Create defined AV Frame
                CType switchType = dll.GetType(configData.ClassName);
                ConstructorInfo ciSwitch = switchType.GetConstructor(new CType[] {typeof(UInt32),typeof(CrestronControlSystem)});
                object swInstance = ciSwitch.Invoke(new object[] {Convert.ToUInt32(configData.IpId), master });
                avSwitch = (Switch)swInstance;

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

                //TODO Add endpoints to given output channels
                foreach (RoomBox roomBox in configData.RoomBoxes)
                {
                    CType epType = dll.GetType(roomBox.ClassName);
                    ConstructorInfo cInfo = epType.GetConstructor(new CType[] { typeof(UInt32), typeof(DMOutput) });
                    RoomBoxes.Add((EndpointReceiverBase)cInfo.Invoke(new object[] { Convert.ToUInt32(roomBox.IpId), avSwitch.Outputs[Convert.ToUInt32(roomBox.OutputNumber)] }));
                }

                //TODO Register Switcher
                if (avSwitch.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failed to register AV switch: {0}", avSwitch.RegistrationFailureReason);
                    return false;
                }

                //TODO Register Endpoints
            }
            else
            {
                ErrorLog.Error("Failed to load assembly information.");
            }
            return false;
        }

        /// <summary>
        /// Assign a roombox/endpoint to the given output channel. Index starts at 1.
        /// </summary>
        /// <param name="output">The target output channel for the endpoint</param>
        /// <param name="endpoint">the endpoint that will be assigned to the target output.</param>
        /// <exception cref="ArgumentException">if "output" is greater than the number of supported outputs, or if "endpoint" is null.</exception>
        public void AddEndpoint(uint output, EndpointReceiverBase endpoint)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of defined outputs.");
            if (endpoint == null) throw new ArgumentException("Argument 'endpoint' cannot be null.");

            if (IsInitialized)
            {
                //TODO Add endpoint to the target output channel
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Get the ComPort collection from the endpoint at the given output channel.
        /// If there is no endpoint or if the endpoint does not support ComPorts then NULL is returned.
        /// </summary>
        /// <param name="output">The output channel connected to the target endpoint</param>
        /// <returns>a collection of ComPorts on that output or NULL if there is no endpoint or comports are not supported.</returns>
        /// <exception cref="ArgumentException">If "output" is greater than the number of output channels on the AV switch device.</exception>
        public ComPort[] GetEndpointComports(uint output)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of output channels.");
            if (IsInitialized)
            {
                //TODO check the given output for existing endpoint, see if the endpoint supports ComPorts, return the ComPort collection for that endpoint or null.
                return null;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// Route the target AV input to the given output channel. Indexing starts at 1.
        /// </summary>
        /// <param name="input">The input that will be routed</param>
        /// <param name="output">The target output channel on the AV Switch</param>
        /// <exception cref="ArgumentException">If input > NumInputs or if output > NumOutputs</exception>"
        public void Route(uint input, uint output)
        {
            if (input > NumInputs || output > NumOutputs) throw new ArgumentException("Arguments input & output cannot be greater than the collection of AV input/output channels.");
            if (IsInitialized)
            {
                //TODO Implement routing functionality
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
                //TODO Set all output channels to input 0.
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        /// <summary>
        /// retreive the current input number that is routed to the target output.
        /// Arguments & return value are 1-indexed.
        /// </summary>
        /// <param name="output">The target output to get input routing from</param>
        /// <returns>The index of the routed input, or 0 if no input is routed.</returns>
        /// <exception cref="ArgumentException">if output > NumOutputs</exception>"
        public uint GetCurrentRoute(uint output)
        {
            if (output > NumOutputs) throw new ArgumentException("Argument 'output' cannot be greater than the number of output channels.");
            if (IsInitialized)
            {
                //TODO Return the value of current route
                return 0;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            } 
        }
    }
}