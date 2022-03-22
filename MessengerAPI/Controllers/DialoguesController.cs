using MessengerAPI.Models;
using MessengerAPI.Models.LogicLayer;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DialoguesController : ControllerBase
    {
        private readonly MessengerAPIDbContext _context;
        private readonly CryptographyService _cryptograhpyService;
        private readonly IStringLocalizer<DialoguesController> _localizer;

        public DialoguesController(MessengerAPIDbContext context, CryptographyService cryptograhpyService, IStringLocalizer<DialoguesController> localizer)
        {
            _context = context;
            _cryptograhpyService = cryptograhpyService;
            _localizer = localizer;
        }

        /*// GET: api/Dialogues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dialogues>>> Get()
        {
            return await _context.Dialogues.ToListAsync();
        }*/

        // gets all conversations of current user
        // GET: api/Dialogues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Conversation>>> Get(int id)
        {
            var dialogues = await _context.Dialogues.Where(d => d.IndividualId == id).Include(d => d.DialoguesMessages).ThenInclude(dm => dm.Message).OrderByDescending(d => d.LastUpdate).ToListAsync();

            if (dialogues == null)
            {
                return NotFound();
            }

            List<Conversation> conversations = new List<Conversation>();
            foreach (var d in dialogues)
            {
                var conversation = await _context.Individuals.Where(i => i.Id == d.InterlocutorId).Select(i =>
                new Conversation { Id = i.PublicId, Name = _cryptograhpyService.DecryptString(i.Name) }).FirstOrDefaultAsync();
                if (conversation != null)
                {
                    conversation.Message = d.InterlocutorId;
                    conversation.LastMessage = d.DialoguesMessages
                        .Select(dm => new Message { SenderId = dm.Message.SenderId, DecryptedMessage = _cryptograhpyService.DecryptMessage(dm.Message.Message) }).LastOrDefault();
                }
                else
                {
                    conversation = new Conversation
                    {
                        Message = d.InterlocutorId,
                        Name = _localizer["deleted"],
                        LastMessage = d.DialoguesMessages
                           .Select(dm => new Message { SenderId = dm.Message.SenderId, DecryptedMessage = _cryptograhpyService.DecryptMessage(dm.Message.Message) }).LastOrDefault()
                    };
                }
                conversations.Add(conversation);
            }

            return conversations;
        }

        /*[HttpGet("{userId}/{interlocutorId}")]
        public async Task<ActionResult<List<Messages>>> Get(int userId, int interlocutorId)
        {
            var messages = await _context.DialoguesMessages.Where(dm => dm.DialogueIndividualId == userId && dm.DialogueInterlocutorId == interlocutorId).Include(dm => dm.Message)
                .Select(dm => new Messages { SenderId = dm.Message.SenderId, Date = dm.Message.Date, Message = dm.Message.Message }).ToListAsync();
            return messages;
        }*/

        // PUT: api/Dialogues/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /*[HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Dialogues dialogues)
        {
            if (id != dialogues.IndividualId)
            {
                return BadRequest();
            }

            _context.Entry(dialogues).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DialoguesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }*/

        //gets all messages of current dialogue
        [HttpPost]
        public async Task<ActionResult<List<Message>>> Post(Dialogues dialogue)
        {
            if (!DialoguesExists(dialogue))
            {
                dialogue.LastUpdate = DateTimeOffset.UtcNow;
                await _context.Dialogues.AddAsync(dialogue);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    throw;
                }
            }
            var messages = await _context.DialoguesMessages.Where(dm => dm.DialogueIndividualId == dialogue.IndividualId && dm.DialogueInterlocutorId == dialogue.InterlocutorId)
                .Include(dm => dm.Message).Select(dm => new Message
                {
                    SenderId = dm.Message.SenderId,
                    DecryptedDate = DateTime.Parse(_cryptograhpyService.DecryptMessage(dm.Message.Date)),
                    DecryptedMessage = _cryptograhpyService.DecryptMessage(dm.Message.Message)
                }).ToListAsync();
            return messages;
        }

        [HttpPost("del")]
        public async Task<ActionResult<Dialogues>> Delete(Dialogues dialogue)
        {
            var dialogueToDelete = await _context.Dialogues.FindAsync(dialogue.IndividualId, dialogue.InterlocutorId);
            if (dialogueToDelete == null)
            {
                return NotFound();
            }
            var isturnedDialogueExist = _context.Dialogues.Any(td => td.IndividualId == dialogueToDelete.InterlocutorId && td.InterlocutorId == dialogueToDelete.IndividualId);
            if (!isturnedDialogueExist)
            {
                var messages = await _context.DialoguesMessages.Where(dm => dm.Dialogue == dialogueToDelete).Select(dm => dm.Message).ToListAsync();
                foreach (var m in messages)
                {
                    _context.Messages.Remove(m);
                }
            }
            _context.Dialogues.Remove(dialogueToDelete);
            await _context.SaveChangesAsync();

            return dialogueToDelete;
        }

        private bool DialoguesExists(Dialogues dialogues)
        {
            return _context.Dialogues.Any(d => d == dialogues);
        }
    }
}
