using System.ComponentModel.DataAnnotations;

namespace MessengerAPI.Models
{
    public class Confirmations
    {
        [Key]
        public string ToConfirm { get; set; }
        public int Confirmator { get; set; }
    }
}
