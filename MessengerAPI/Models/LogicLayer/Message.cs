using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models.LogicLayer
{
    public class Message
    {
        public int SenderId { get; set; }
        public string DecryptedMessage { get; set; }
        public DateTimeOffset DecryptedDate { get; set; }
    }
}
