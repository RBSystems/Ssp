using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Display;
using Crestron.ThirdPartyCommon.StandardCommands;
using Crestron.ThirdPartyCommon.Transports;

namespace SspCompanyVideoDisplayEthernet
{
    public class SspCompanyVideoDisplayProtocol : ADisplayProtocol
    {
        public SspCompanyVideoDisplayProtocol(ISerialTransport transport, byte ID)
            : base(transport, ID)
        {
            // Set response validation object
            ResponseValidation = new ResponseValidator(ValidatedData);

            // Set up polling sequence when device powers on and powers off
            ValidatedData.PowerOnPollingSequence = new[] { StandardCommandsEnum.PowerPoll, StandardCommandsEnum.VolumePoll, StandardCommandsEnum.InputPoll };
            ValidatedData.PowerOffPollingSequence = new[] { StandardCommandsEnum.PowerPoll };

            // How often to poll the device
            PollingInterval = 1000;
        }

        protected override void PrepareStringThenSend(ADisplayProtocol.CommandSet commandSet)
        {
            // Append command delimiter
            commandSet.Command = commandSet.Command + "\u000D\u000A";

            // Continue calling sequence as normal
            base.PrepareStringThenSend(commandSet);
        }
    }

    public class ResponseValidator : ResponseValidation
    {
        public const string delimiter = "\u000D\u000A";

        public ResponseValidator(DataValidation dataValidation)
            : base(dataValidation)
        { }

        public override ValidatedRxData ValidateResponse(string response, SupportedCommandGroups commandGroup)
        {
            ValidatedRxData validatedData = new ValidatedRxData(false, string.Empty);

            // response has delimiter? If not, excape validation
            if (!response.Contains(delimiter))
                return validatedData;

            // remove modified delimter from command before sending it to the base validation
            response = response.Substring(0, response.Length - delimiter.Length);
            return base.ValidateResponse(response, commandGroup);
        }
    }
}