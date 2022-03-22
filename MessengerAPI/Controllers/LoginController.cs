using MessengerAPI.Models;
using MessengerAPI.Models.LogicLayer;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace MessengerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly MessengerAPIDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CryptographyService _cryptographyService;
        private readonly IStringLocalizer<LoginController> _localizer;
        private readonly TimerService _timerService;

        public LoginController(MessengerAPIDbContext context, IConfiguration configuration, CryptographyService cryptographyService, IStringLocalizer<LoginController> localizer, TimerService timerService)
        {
            _context = context;
            _configuration = configuration;
            _cryptographyService = cryptographyService;
            _localizer = localizer;
            _timerService = timerService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return null;
        }

        //login
        [HttpPost]
        public async Task<ActionResult> Post(LoginModel loginModel)
        {
            string email = loginModel.Login;
            loginModel.Login = _cryptographyService.EncryptString(loginModel.Login.ToLower());
            loginModel.Password = _cryptographyService.EncryptString(loginModel.Password);
            var user = _context.Individuals.Where(i => (i.Email == loginModel.Login && i.Password == loginModel.Password) || (i.PhoneNumber == loginModel.Login && i.Password == loginModel.Password)).FirstOrDefault();
            if (user == null)
                return CreatedAtAction("Get", null, new LoginResult { IsLogedIn = false, IsNotFound = true, LoginText = _localizer["invalid"] });

            if (user.IsEmailConfirmed == false)
            {
                var confirmation = await _context.Confirmations.FindAsync(loginModel.Login);
                if (confirmation != null)
                    confirmation.Confirmator = _cryptographyService.CreateNumber(0, 1000000000);
                else
                {
                    confirmation = new Confirmations
                    {
                        ToConfirm = loginModel.Login,
                        Confirmator = _cryptographyService.CreateNumber(0, 30000000)
                    };
                    await _context.Confirmations.AddAsync(confirmation);
                }
                await _context.SaveChangesAsync();
                await EmailService.SendEmailAsync(email, _localizer["emailText"], "<table cellspacing=\"0\" style=\"margin:0;padding:0;border:0;width:100%;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Oxygen','Ubuntu','Cantarell','Fira Sans','Droid Sans','Helvetica Neue',sans-serif;font-size:14px;line-height:23px\"><tbody style=\"margin:0;padding:0;border:0;\"><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"top\" style=\"background-color: #e9ecef; margin: 0;padding: 0;border: 0;text-align: left;vertical-align: top;padding-bottom: 7%;\"></td></tr><tr style=\"margin:0;padding:0;border:0;\"><td style=\"background-color:#e9ecef;margin:0;padding:0;border:0;\"><div style=\"display:inline-block;color:#495057;background-color:white;border-radius:15px;padding-top:30px;padding-left:5%;padding-right:5%;padding-bottom:25px;margin-top:0;margin-left:5%;margin-right:5%;margin-bottom:0;\">" + _localizer["thanks"] + "<div style=\"color:#343a40;font-size: 18px;font-weight: 500;display:inline-block;margin: 10px 0 20px;padding: 10px 35px;border-radius: 15px;border: 2px dashed#e9ecef\">" + confirmation.Confirmator + "</div><br>" + _localizer["enter"] + "<br><br><br><br></div></td></tr><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"bottom\"style=\"background-color: #e9ecef;margin:0;padding:0;border:0;height:100px;color:white;font-size: 13px;vertical-align:bottom;padding-top:0;padding-left:10%;padding-right:10%;\"><p class=\"pt_mr_css_attr\"style=\"color:#495057\">" + _localizer["safe"] + "</p></td></tr></tbody></table>");
                return CreatedAtAction("Get", null, new LoginResult { IsLogedIn = false, IsNotFound = false, LoginText = _localizer["notVerified"] });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JWTSettings:SecretKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(14),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var authUser = new AuthUser
            {
                Message = user.Id,
                Avatar = tokenHandler.WriteToken(token)
            };
            return CreatedAtAction("Get", null, new LoginResult { IsLogedIn = true, IsNotFound = false, Loged = _cryptographyService.EncryptString(JsonSerializer.Serialize(authUser)) });
        }

        //is token valid
        [HttpPost("isv")]
        public ActionResult Post([FromBody] string value)
        {
            var authUser = JsonSerializer.Deserialize<AuthUser>(_cryptographyService.DecryptString(value));
            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(authUser.Avatar, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JWTSettings:SecretKey"))),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken securityToken);
            }
            catch
            {
                return CreatedAtAction("Get", null, new LoginResult { LoginText = "Invalid" });
            }
            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JWTSettings:SecretKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, principal.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").FirstOrDefault().Value),
                    new Claim(ClaimTypes.NameIdentifier, principal.Claims.Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault().Value)
                }),
                Expires = DateTime.UtcNow.AddDays(14),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            authUser.Avatar = tokenHandler.WriteToken(token);
            return CreatedAtAction("Get", null, new LoginResult { LoginText = JsonSerializer.Serialize(authUser), Loged = _cryptographyService.EncryptString(JsonSerializer.Serialize(authUser)) });
        }

        [HttpPost("forgot")]
        public async Task<ActionResult> PostForgot([FromBody]string login)
        {
            var loginEncrypted = _cryptographyService.EncryptString(login.ToLower());
            var user = _context.Individuals.Where(i => (i.Email == loginEncrypted) || (i.PhoneNumber == loginEncrypted)).FirstOrDefault();
            if (user == null)
            {
                return CreatedAtAction("Get", null, false);
            }
            string query = _cryptographyService.EncryptForgot(loginEncrypted.Take(12) + ";kome99KediN;" + loginEncrypted + ";Ya1lny4Shdym");
            //string url = "http://localhost:3000/restore?data=" + query;
            string url =  "https://sentstring.com/restore?data=" + query;
            await EmailService.SendEmailAsync(login, _localizer["pasrec"], "<table cellspacing=\"0\" style=\"margin:0;padding:0;border:0;width:100%;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Oxygen','Ubuntu','Cantarell','Fira Sans','Droid Sans','Helvetica Neue',sans-serif;font-size:14px;line-height:23px\"><tbody style=\"margin:0;padding:0;border:0;\"><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"top\" style=\"background-color: #e9ecef; margin: 0;padding: 0;border: 0;text-align: left;vertical-align: top;padding-bottom: 7%;\"></td></tr><tr style=\"margin:0;padding:0;border:0;\"><td style=\"background-color:#e9ecef;margin:0;padding:0;border:0;\"><div style=\"display:inline-block;color:#495057;background-color:white;border-radius:15px;padding-top:30px;padding-left:5%;padding-right:5%;padding-bottom:25px;margin-top:0;margin-left:5%;margin-right:5%;margin-bottom:0;\">" + _localizer["recoveryLetter"] + "<div style=\"color:#343a40;font-size: 18px;font-weight: 500;display:inline-block;margin: 10px 0 20px;padding: 10px 35px;border-radius: 15px;border: 2px dashed#e9ecef\"><a href=\"" + url + "\">" + _localizer["restore"] + "</a></div><br><br><br></div></td></tr><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"bottom\"style=\"background-color: #e9ecef;margin:0;padding:0;border:0;height:100px;color:white;font-size: 13px;vertical-align:bottom;padding-top:0;padding-left:10%;padding-right:10%;\"><p class=\"pt_mr_css_attr\"style=\"color:#495057\">" + _localizer["safe"] + "</p></td></tr></tbody></table>");
            user.IsRestoring = true;
            await _context.SaveChangesAsync();
            _timerService.CancelUsersPasswordRestore(user.Id);
            return CreatedAtAction("Get", null, true);
        }

        [HttpPost("restore")]
        public async Task<ActionResult> PostRestore([FromBody]string data)
        {
            string decryptedString = _cryptographyService.DecryptForgot(data);
            string login = decryptedString.Split(';')[2];
            var user = _context.Individuals.Where(i => (i.Email == login) || (i.PhoneNumber == login)).FirstOrDefault();
            if (user == null)
            {
                return CreatedAtAction("Get", null, 2);
            }
            if (!user.IsRestoring)
            {
                return CreatedAtAction("Get", null, 0);
            }
            string newPassword = _cryptographyService.CreateString(8);
            user.Password = _cryptographyService.EncryptString(newPassword);
            user.IsRestoring = false;
            await _context.SaveChangesAsync();
            await EmailService.SendEmailAsync(_cryptographyService.DecryptString(user.Email), _localizer["pasrec"], "<table cellspacing=\"0\" style=\"margin:0;padding:0;border:0;width:100%;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Oxygen','Ubuntu','Cantarell','Fira Sans','Droid Sans','Helvetica Neue',sans-serif;font-size:14px;line-height:23px\"><tbody style=\"margin:0;padding:0;border:0;\"><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"top\" style=\"background-color: #e9ecef; margin: 0;padding: 0;border: 0;text-align: left;vertical-align: top;padding-bottom: 7%;\"></td></tr><tr style=\"margin:0;padding:0;border:0;\"><td style=\"background-color:#e9ecef;margin:0;padding:0;border:0;\"><div style=\"display:inline-block;color:#495057;background-color:white;border-radius:15px;padding-top:30px;padding-left:5%;padding-right:5%;padding-bottom:25px;margin-top:0;margin-left:5%;margin-right:5%;margin-bottom:0;\">" + _localizer["newPas"] + "<div style=\"color:#343a40;font-size: 18px;font-weight: 500;display:inline-block;margin: 10px 0 20px;padding: 10px 35px;border-radius: 15px;border: 2px dashed#e9ecef\">" + newPassword + "</div><br><br>" + _localizer["reccoment"] + "<br></div></td></tr><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"bottom\"style=\"background-color: #e9ecef;margin:0;padding:0;border:0;height:100px;color:white;font-size: 13px;vertical-align:bottom;padding-top:0;padding-left:10%;padding-right:10%;\"><p class=\"pt_mr_css_attr\"style=\"color:#495057\">" + _localizer["safe"] + "</p></td></tr></tbody></table>");
            return CreatedAtAction("Get", null, 1);
        }

        /*// DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
