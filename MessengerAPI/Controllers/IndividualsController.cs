using MessengerAPI.Models;
using MessengerAPI.Models.LogicLayer;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IndividualsController : ControllerBase
    {
        private readonly MessengerAPIDbContext _context;
        private readonly CryptographyService _cryptographyService;
        private readonly IStringLocalizer<IndividualsController> _localizer;

        public IndividualsController(MessengerAPIDbContext context, CryptographyService cryptographyService, IStringLocalizer<IndividualsController> localizer)
        {
            _context = context;
            _cryptographyService = cryptographyService;
            _localizer = localizer;
        }

        /*[HttpGet]
        public async Task<ActionResult<IEnumerable<Individuals>>> Get()
        {
            return await _context.Individuals.ToListAsync();
        }*/

        [HttpGet]
        public ActionResult Get()
        {
            return null;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileUser>> Get(int id)
        {
            var individual = await _context.Individuals.FindAsync(id);

            if (individual == null)
            {
                return NotFound();
            }

            return new ProfileUser { Id = individual.PublicId, Email = _cryptographyService.DecryptString(individual.Email), Name = _cryptographyService.DecryptString(individual.Name) };
        }

        // register new User
        [HttpPost]
        public async Task<ActionResult> Post(RegUser regUser)
        {
            if (ModelState.IsValid)
            {
                if (!_context.Individuals.Any(i => i.Email == _cryptographyService.EncryptString(regUser.Email)))
                {
                    int publicId = 0;
                    for (int i = 100; i <= 1000000000; i *= 10)
                    {
                        publicId = _cryptographyService.CreateNumber(0, i);
                        if (!_context.Individuals.Any(i => i.PublicId == publicId))
                            break;
                    }
                    var individual = new Individuals
                    {
                        Email = _cryptographyService.EncryptString(regUser.Email.ToLower()),
                        Password = _cryptographyService.EncryptString(regUser.Password),
                        Name = _cryptographyService.EncryptString(regUser.Name),
                        PublicId = publicId,
                        // this lines to be deleted on production
                        /*Country = regUser.Email,
                        Address = regUser.Password*/
                    };
                    await _context.Individuals.AddAsync(individual);
                    var confirmation = new Confirmations
                    {
                        ToConfirm = _cryptographyService.EncryptString(regUser.Email),
                        Confirmator = _cryptographyService.CreateNumber(0, 1000000000)
                    };
                    await _context.Confirmations.AddAsync(confirmation);
                    await _context.SaveChangesAsync();
                    await EmailService.SendEmailAsync(regUser.Email, _localizer["emailText"], "<table cellspacing=\"0\" style=\"margin:0;padding:0;border:0;width:100%;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI','Roboto','Oxygen','Ubuntu','Cantarell','Fira Sans','Droid Sans','Helvetica Neue',sans-serif;font-size:14px;line-height:23px\"><tbody style=\"margin:0;padding:0;border:0;\"><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"top\" style=\"background-color: #e9ecef; margin: 0;padding: 0;border: 0;text-align: left;vertical-align: top;padding-bottom: 7%;\"></td></tr><tr style=\"margin:0;padding:0;border:0;\"><td style=\"background-color:#e9ecef;margin:0;padding:0;border:0;\"><div style=\"display:inline-block;color:#495057;background-color:white;border-radius:15px;padding-top:30px;padding-left:5%;padding-right:5%;padding-bottom:25px;margin-top:0;margin-left:5%;margin-right:5%;margin-bottom:0;\">" + _localizer["thanks"] + "<div style=\"color:#343a40;font-size: 18px;font-weight: 500;display:inline-block;margin: 10px 0 20px;padding: 10px 35px;border-radius: 15px;border: 2px dashed#e9ecef\">" + confirmation.Confirmator + "</div><br>" + _localizer["enter"] + "<br><br><br><br></div></td></tr><tr style =\"margin:0;padding:0;border:0;\"><td valign=\"bottom\"style=\"background-color: #e9ecef;margin:0;padding:0;border:0;height:100px;color:white;font-size: 13px;vertical-align:bottom;padding-top:0;padding-left:10%;padding-right:10%;\"><p class=\"pt_mr_css_attr\"style=\"color:#495057\">" + _localizer["safe"] + "</p></td></tr></tbody></table>");
                    string text = _localizer["congratulations"];
                    return CreatedAtAction("Get", null, text);
                }
                else
                {
                    string text = _localizer["emailExist"];
                    return CreatedAtAction("Get", null, text);
                }
            }
            else
            {
                var errors = new Dictionary<string, string>();
                foreach (var k in ModelState.Keys)
                {
                    errors[k.ToLower()] = ModelState.Where(m => m.Key == k).Select(m => m.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()).FirstOrDefault();
                }
                return CreatedAtAction("Get", null, errors);
            }
        }

        [HttpPost("confirm")]
        public async Task<ActionResult> Post([FromForm]string toConfirm, [FromForm]int confirmator)
        {
            toConfirm = _cryptographyService.EncryptString(toConfirm);
            var confirmation = await _context.Confirmations.FindAsync(toConfirm);
            if (confirmation == null)
            {
                return CreatedAtAction("Get", null, false);
            }
            else
            {
                if (confirmation.Confirmator == confirmator)
                {
                    _context.Individuals.Where(i => i.Email == toConfirm).FirstOrDefault().IsEmailConfirmed = true;
                    _context.Confirmations.Remove(confirmation);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction("Get", null, true);
                }
                else
                {
                    return CreatedAtAction("Get", null, JsonSerializer.Serialize(_localizer["tryAgain"].ToString()));
                }
            }
        }

        [Authorize]
        [HttpPost("change")]
        public async Task<ActionResult> Post(ChangePassword changePassword)
        {
            if (ModelState.IsValid)
            {
                changePassword.Login = _cryptographyService.EncryptString(changePassword.Login.ToLower());
                changePassword.OldPassword = _cryptographyService.EncryptString(changePassword.OldPassword);
                var user = _context.Individuals.Where(i => (i.Email == changePassword.Login && i.Password == changePassword.OldPassword) || (i.PhoneNumber == changePassword.Login && i.Password == changePassword.OldPassword)).FirstOrDefault();

                if (user == null)
                    return CreatedAtAction("Get", null, _localizer["invalidOld"].ToString());

                user.Password = _cryptographyService.EncryptString(changePassword.Password);
                await _context.SaveChangesAsync();
                return CreatedAtAction("Get", null, _localizer["changed"].ToString());
            }
            else
            {
                var errors = new Dictionary<string, string>();
                foreach (var k in ModelState.Keys)
                {
                    errors[k.ToLower()] = ModelState.Where(m => m.Key == k).Select(m => m.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()).FirstOrDefault();
                }
                return CreatedAtAction("Get", null, errors);
            }
        }

        // delete user
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Individuals>> Delete(int id)
        {
            var dialogues = await _context.Dialogues.Where(d => d.IndividualId == id).ToListAsync();
            foreach (var d in dialogues)
            {
                var isturnedDialogueExist = _context.Dialogues.Any(td => td.IndividualId == d.InterlocutorId && td.InterlocutorId == d.IndividualId);
                if (isturnedDialogueExist)
                    continue;
                else
                {
                    var messages = await _context.DialoguesMessages.Where(dm => dm.Dialogue == d).Select(dm => dm.Message).ToListAsync();
                    foreach (var m in messages)
                    {
                        _context.Messages.Remove(m);
                    }
                }
            }
            var individuals = await _context.Individuals.FindAsync(id);
            if (individuals == null)
            {
                return NotFound();
            }

            _context.Individuals.Remove(individuals);
            await _context.SaveChangesAsync();

            return individuals;
        }

        /*private bool IndividualsExists(int id)
        {
            return _context.Individuals.Any(e => e.Id == id);
        }*/
        //for test
        /*[HttpGet("convert")]
        public string GetMe()
        {
            var time = DateTimeOffset.UtcNow;
            var timeString = time.ToString();
            var tested = Convert.ToDateTime(timeString);
            var result = JsonSerializer.Serialize(time + " -- " + tested);
            return result;
        }*/
    }
}
