using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;

namespace SspCertificationTest.Utilities
{
    public class SshClientControl
    {
        public event EventHandler<GenericEventArgs<string>> RxEvent;

        private SshClient client;
        private ShellStream comStream;
        private CrestronQueue<string> cmdQueue;

        public SshClientControl()
        {
            CrestronInvoke.BeginInvoke(ProcessResponse);
        }

        public bool Connect(string host, int port, string userName, string password)
        {
            try
            {
                if (client != null && client.IsConnected)
                    return true;

                client = new SshClient(host, port, userName, password);
                client.Connect();

                try
                {
                    comStream = client.CreateShellStream("VtcStream", 30, 24, 800, 600, 1024);
                    comStream.DataReceived += new EventHandler<Crestron.SimplSharp.Ssh.Common.ShellDataEventArgs>(comStream_DataReceived);
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Failed to create Tx/Rx stream: {0} -- {1}", e.Message, e.StackTrace);
                }

            }
            catch (Exception e)
            {
                ErrorLog.Error("Failed to connect SSH Client: {0} -- {1}", e.Message, e.StackTrace);
            }
            return false;
        }

        public bool SendCommand(string cmd)
        {
            try
            {
                SshCommand tx = client.CreateCommand(cmd);
                cmdQueue.Enqueue(tx.Execute());
                return true;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Failed to send command: {0} -- {1}", e.Message, e.StackTrace);
            }
            return false;
        }

        public bool Disconnect()
        {
            if (client != null && client.IsConnected)
            {
                try
                {
                    client.Disconnect();
                    return !client.IsConnected;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("failed to disconnect SSH client: {0} -- {1}", e.Message, e.StackTrace);
                }
            }
            return false;
        }

        private void ProcessResponse(object obj)
        {
            string rxString = string.Empty;
            while (true)
            {
                try
                {
                    rxString = cmdQueue.Dequeue();
                    triggerRxEvent(rxString);
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Error while processing command response: {0} -- {1}", e.Message, e.StackTrace);
                }
            }
        }

        void comStream_DataReceived(object sender, Crestron.SimplSharp.Ssh.Common.ShellDataEventArgs e)
        {
            StringBuilder rxBuilder = new StringBuilder();
            var stream = (ShellStream)sender;
            while (stream.DataAvailable)
            {
                rxBuilder.Append(stream.Read());
            }
            triggerRxEvent(rxBuilder.ToString());
        }

        private void triggerRxEvent(string rx)
        {
            EventHandler<GenericEventArgs<string>> temp = RxEvent;
            if (temp != null)
            {
                temp(this, new GenericEventArgs<string>(rx));
            }
        }
    }
}