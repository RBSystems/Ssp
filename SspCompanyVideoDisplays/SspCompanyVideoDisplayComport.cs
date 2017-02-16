using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.ThirdPartyCommon.Interfaces;
using ProTransports;
using Crestron.ThirdPartyCommon.Transports;
using Newtonsoft.Json;
using Crestron.Display;

namespace SspCompanyVideoDisplayComport
{
    public class SspCompanyVideoDisplayComport : SspCompanyVideoDisplay, ITcp
    {
        public ComPortSpec ComSpec { get; private set; }

        private bool InternalSupportsDisconnect;
        public override bool SupportsDisconnect { get { return InternalSupportsDisconnect; } }

        private bool InternalSupportsReconnect;
        public override bool SupportsReconnect { get { return InternalSupportsReconnect; } }

        private SimplTransport transport;

        public SspCompanyVideoDisplayComport()
        {
            LoadComSettings();
        }

        public void Initialize(IComPort comPort)
        {
            InternalSupportsDisconnect = false;
            InternalSupportsReconnect = false;
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            DisplayProtocol = new SspCompanyVideoDisplayProtocol(ConnectionTransport, Id);
            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.LoadDriver(DataFile);
            DisplayProtocol.RxOut += SendRxOut;
        }

        public SimplTransport Initialize(Action<string, object[]> send)
        {
            InternalSupportsDisconnect = false;
            InternalSupportsReconnect = false;

            transport = new SimplTransport { Send = send };

            ConnectionTransport = transport;
            DisplayProtocol = new SspCompanyVideoDisplayProtocol(ConnectionTransport, Id);
            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.LoadDriver(DataFile);
            return transport;
        }

        public void LoadComSettings()
        {
            try
            {
                var json = DataFile;
                var driverData = JsonConvert.DeserializeObject<RootObject>(json);
                if (driverData != null)
                {
                    if (driverData.CrestronSerialDeviceApi != null)
                    {
                        ComSpec = new ComPortSpec
                        {
                            BaudRate = driverData.CrestronSerialDeviceApi.Api.Communication.Baud,
                            DataBits = driverData.CrestronSerialDeviceApi.Api.Communication.DataBits,
                            HardwareHandShake = driverData.CrestronSerialDeviceApi.Api.Communication.HwHandshake,
                            Parity = driverData.CrestronSerialDeviceApi.Api.Communication.Parity,
                            Protocol = driverData.CrestronSerialDeviceApi.Api.Communication.Protocol,
                            StopBits = driverData.CrestronSerialDeviceApi.Api.Communication.StopBits,
                            SoftwareHandshake = driverData.CrestronSerialDeviceApi.Api.Communication.SwHandshake
                        };
                    }
                }
            }
            catch
            {
                Log("Attempt to load unknown com settings.");
            }
        }
    }
}