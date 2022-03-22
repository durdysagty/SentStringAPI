using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models
{
    public class DialoguesMessages
    {
        public int DialogueIndividualId { get; set; }
        public int DialogueInterlocutorId { get; set; }
        public int MessageId { get; set; }
        public virtual Dialogues Dialogue { get; set; }
        public virtual Messages Message { get; set; }
    }
}
