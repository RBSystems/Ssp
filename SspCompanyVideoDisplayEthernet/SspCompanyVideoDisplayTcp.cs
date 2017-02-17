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
        private bool InternalSupportsDisconnect;
        public override bool SupportsDisconnect { get { return InternalSupportsDisconnect; } }

        private bool InternalSupportsReconnect;
        public override bool SupportsReconnect { get { return InternalSupportsReconnect; } }

        private SimplTransport transport;

        public SspCompanyVideoDisplayTcp() {}

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
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DriverData);
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
            DisplayProtocol.Initialize(DriverData);
            return transport;
        }
    }
}