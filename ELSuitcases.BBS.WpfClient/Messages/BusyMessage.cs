using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class BusyMessage : ValueChangedMessage<bool>
    {
        public bool IsBusy { get; private set; } = false;
        public string Message { get; set; } = string.Empty;
        public object? Sender { get; private set; } = null;



        public BusyMessage(object? sender, bool isBusy, string message) : base(isBusy)
        {
            Sender = sender;
            IsBusy = isBusy;
            Message = message;
        }
    }
}
