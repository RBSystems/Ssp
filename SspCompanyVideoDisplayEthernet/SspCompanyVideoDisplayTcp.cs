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

namespace SspCompanyVideoDisplayTcp
{
    public class SspCompanyVideoDisplayTcp : SspCompanyVideoDisplay, ITcp
    {
        public int Port { get; private set; }

        private bool InternalSupportsDisconnect;
        public override bool SupportsDisconnect { get { return InternalSupportsDisconnect; } }

        private bool InternalSupportsReconnect;
        public override bool SupportsReconnect { get { return InternalSupportsReconnect; } }

        private SimplTransport transport;

        public SspCompanyVideoDisplayTcp()
        {
            LoadComSettings();
        }

        public void Initialize(IPAddress ipAddress, int port)
        {
            InternalSupportsDisconnect = false;
            InternalSupportsReconnect = false;

            TcpTransport tcpTransport = new TcpTransport
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };
            tcpTransport.Initialize(ipAddress, port);
            ConnectionTransport = tcpTransport;

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
                        Port = driverData.CrestronSerialDeviceApi.Api.Communication.Port;
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