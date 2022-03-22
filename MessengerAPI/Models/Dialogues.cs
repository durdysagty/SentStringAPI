using System;
using System.Collections.Generic;

namespace MessengerAPI.Models
{
    public class Dialogues
    {
        public int IndividualId { get; set; }
        public int InterlocutorId { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public virtual Individuals Individual { get; set; }
        public virtual ICollection<DialoguesMessages> DialoguesMessages { get; set; }
    }
}
