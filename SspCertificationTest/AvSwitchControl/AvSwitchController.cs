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

        private EndpointReceiverBase[] RoomBoxes;   // Tracking the endpoints that have been assigned to the AV Switch
        private Switch avSwitch;                    // The Crestron switcher object representing the physical hardware
        private CrestronControlSystem master;       // The control system driving the given room
        private AvSwitcher configData;              // JSON object defined in the configuration file.

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
        }

        /// <summary>
        /// Runs the Switch construction based on the data supplied during instantiation.
        /// </summary>
        /// <returns>TRUE if initialization was successful, FALSE otherwise.</returns>
        public bool Initialize()
        {
            //TODO Build AV Switch object via reflection
            return false;
        }

        /// <summary>
        /// Assign a roombox/endpoint to the given output channel.
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

        public ComPort[] GetEndpointComports(uint output)
        {
            if (IsInitialized)
            {

                return null;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        public void Route(uint input, uint output)
        {
            if (IsInitialized)
            {

            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        public void ClearAllRoutes()
        {
            if (IsInitialized)
            {

            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            }
        }

        public uint GetCurrentRoute(uint output)
        {
            if (IsInitialized)
            {
                return 0;
            }
            else
            {
                throw new InvalidOperationException("AvSwitchController has not been initialized.");
            } 
        }
    }
}