using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }

        public virtual ICollection<DialoguesMessages> DialoguesMessages { get; set; }
    }
}
