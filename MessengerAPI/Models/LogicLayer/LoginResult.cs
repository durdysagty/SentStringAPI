using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models.LogicLayer
{
    public class LoginResult
    {
        public bool IsLogedIn { get; set; }
        public bool IsNotFound { get; set; }
        public string LoginText { get; set; }
        public string Loged { get; set; }
    }
}
