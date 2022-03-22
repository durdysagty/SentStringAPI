using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MessengerAPI.Models
{
    public class Individuals
    {
        public int Id { get; set; }
        [Required]
        public int PublicId { get; set; }
        [Required]
        [MaxLength(300)]
        public string Email { get; set; }
        [MaxLength(300)]
        public string Password { get; set; }
        [MaxLength(500)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string PhoneNumber { get; set; }
        public DateTime? BirthDay { get; set; }
        [MaxLength(200)]
        public string Country { get; set; }
        [MaxLength(1000)]
        public string Address { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public bool IsRestoring { get; set; }
        public virtual ICollection<Friends> Friends { get; set; }
        public virtual ICollection<Dialogues> Dialogues { get; set; }
    }
}
