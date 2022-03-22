using System.ComponentModel.DataAnnotations;

namespace MessengerAPI.Models.LogicLayer
{
    public class ChangePassword
    {
        public string Login { get; set; }
        [Required(ErrorMessage = "passwordEmpty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "validPassword")]
        [MinLength(8, ErrorMessage = "minPassword")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "passwordEmpty")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).+$", ErrorMessage = "validPassword")]
        [MinLength(8, ErrorMessage = "minPassword")]
        public string Password { get; set; }
        [Required(ErrorMessage = "emptyConfirm")]
        [Compare("Password", ErrorMessage = "notMatch")]
        public string Confirm { get; set; }
    }
}
