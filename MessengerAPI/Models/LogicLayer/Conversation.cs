using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models.LogicLayer
{
    public class Conversation
    {
        // publicId to show
        public int Id { get; set; }
        public string Name { get; set; }
        // real id
        public int Message { get; set; }
        public Message LastMessage { get; set; }
    }
}
