using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using MessengerAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using MessengerAPI.Models.LogicLayer;
using MessengerAPI.Services;
using Microsoft.Extensions.Localization;

namespace MessengerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly MessengerAPIDbContext _context;
        //private readonly IHubContext<SignalRHub> _signal;
        private readonly CryptographyService _cryptograhpyService;
        private readonly IStringLocalizer<FriendsController> _localizer;

        public FriendsController(MessengerAPIDbContext context, /*IHubContext<SignalRHub> signal, */CryptographyService cryptograhpyService, IStringLocalizer<FriendsController> localizer)
        {
            _context = context;
            //_signal = signal;
            _cryptograhpyService = cryptograhpyService;
            _localizer = localizer;
        }

        // returns friends in contacts, returns friends to remove
        // GET: api/Friends/5
        [HttpGet("{id}/{skipper}")]
        public async Task<ActionResult<List<Contact>>> Get(int id, int skipper)
        {
            var friends = await _context.Friends.Where(f => f.IndividualId == id).Skip(30 * skipper).Take(30).ToListAsync();

            List<Contact> contacts = new List<Contact>();
            foreach (var f in friends)
            {
                var individual = await _context.Individuals.Where(i => i.Id == f.FriendId).FirstOrDefaultAsync();
                var contact = new Contact
                {
                    Message = f.FriendId,
                    Name = individual != null ? _cryptograhpyService.DecryptString(individual.Name) : _localizer["deleted"],
                    Id = f.FriendsPublicId
                };
                contacts.Add(contact);
            }

            return contacts;
        }

        //returns contacts to add
        [HttpGet("add/{id}/{skipper}")]
        public async Task<ActionResult<List<Contact>>> Post(int id, int skipper)
        {
            var ids = await _context.Friends.Where(f => f.IndividualId == id).Select(f => f.FriendId).ToListAsync();
            ids.Add(id);
            var contacts = await _context.Individuals.Where(f => !ids.Any(i => i == f.Id)).Skip(30 * skipper).Take(30).Select(i => new Contact { Message = i.Id, Name = _cryptograhpyService.DecryptString(i.Name), Id = i.PublicId }).ToListAsync();
            return contacts;
        }

        //adds friends
        // POST: api/Friends
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task Post(Friends friends)
        {
            if (!FriendsExists(friends))
                await _context.Friends.AddAsync(friends);
            var turnedDialogue = new Dialogues { IndividualId = friends.FriendId, InterlocutorId = friends.IndividualId, LastUpdate = DateTimeOffset.UtcNow };
            if (!DialoguesExists(turnedDialogue))
                await _context.Dialogues.AddAsync(turnedDialogue);
            //var friendsInterlocutor = Interlocutors.GetInterlocutor(friends.FriendId.ToString());
            //var message = "Added you to contacts";
            //if (friendsInterlocutor == friends.IndividualId.ToString())
            //    await _signal.Clients.User(friends.FriendId.ToString()).SendAsync("Send", message);
            /*var newMessage = new Messages
            {
                Message = _cryptograhpyService.EncryptMessage(message),
                SenderId = friends.IndividualId,
                Date = _cryptograhpyService.EncryptMessage(DateTimeOffset.UtcNow.ToString())
            };*/
            //await _context.Messages.AddAsync(newMessage);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            /*await _context.DialoguesMessages.AddAsync(new DialoguesMessages
            {
                DialogueIndividualId = turnedDialogue.IndividualId,
                DialogueInterlocutorId = turnedDialogue.InterlocutorId,
                MessageId = newMessage.Id
            });
            try
            {
                await _context.SaveChangesAsync();
                return "Excelent!";
            }
            catch (DbUpdateException)
            {
                throw;
            }*/
        }

        // DELETE: api/Friends/5
        [HttpDelete]
        public async Task<ActionResult<Friends>> Delete(Friends friends)
        {
            _context.Friends.Remove(friends);
            await _context.SaveChangesAsync();

            return friends;
        }

        [HttpPost("added")]
        public bool FriendsExists(Friends friends)
        {
            return _context.Friends.Any(f => f == friends);
        }

        private bool DialoguesExists(Dialogues dialogues)
        {
            return _context.Dialogues.Any(d => d == dialogues);
        }
    }
}
