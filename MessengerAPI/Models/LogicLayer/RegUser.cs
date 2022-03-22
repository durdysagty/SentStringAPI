using System.ComponentModel.DataAnnotations;

namespace MessengerAPI.Models.LogicLayer
{
    public class RegUser
    {
        [Required(ErrorMessage = "emailEmpty")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "validEmail")]
        [MinLength(6, ErrorMessage = "minEmail")]
        public string Email { get; set; }
        [Required(ErrorMessage = "passwordEmpty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "validPassword")]
        [MinLength(8, ErrorMessage = "minPassword")]
        public string Password { get; set; }
        [Required(ErrorMessage = "emptyConfirm")]
        [Compare("Password", ErrorMessage = "notMatch")]
        public string Confirm { get; set; }
        [Required(ErrorMessage = "nameEmpty")]
        [RegularExpression(@"\S+", ErrorMessage = "spaces")]
        [MinLength(4, ErrorMessage = "minName")]
        public string Name { get; set; }
    }
}
