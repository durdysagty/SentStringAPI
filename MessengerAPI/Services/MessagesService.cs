using MessengerAPI.Models;
using MessengerAPI.Models.LogicLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Services
{
    public class MessagesService
    {
        private readonly MessengerAPIDbContext _context;
        private readonly CryptographyService _cryptographyService;
        public MessagesService(MessengerAPIDbContext context, CryptographyService cryptographyService)
        {
            _context = context;
            _cryptographyService = cryptographyService;
        }

        public async Task AddMessage(int userId, int interlocutorId, string message)
        {
            var dialogue = await _context.Dialogues.FindAsync(userId, interlocutorId);
            dialogue.LastUpdate = DateTimeOffset.UtcNow;
            if (IndividualExists(interlocutorId))
            {
                var turnedDialogue = await _context.Dialogues.FindAsync(interlocutorId, userId);
                if (turnedDialogue != null)
                    turnedDialogue.LastUpdate = dialogue.LastUpdate;
                else
                {
                    turnedDialogue = new Dialogues
                    {
                        IndividualId = interlocutorId,
                        InterlocutorId = userId,
                        LastUpdate = dialogue.LastUpdate
                    };
                    await _context.Dialogues.AddAsync(turnedDialogue);
                    await _context.SaveChangesAsync();
                }
            }
            var sentMessage = new Messages
            {
                SenderId = userId,
                Message = _cryptographyService.EncryptMessage(message),
                Date = _cryptographyService.EncryptMessage(DateTimeOffset.UtcNow.ToString("u"))
            };
            await _context.Messages.AddAsync(sentMessage);
            await _context.SaveChangesAsync();
            await _context.DialoguesMessages.AddAsync(new DialoguesMessages
            {
                DialogueIndividualId = userId,
                DialogueInterlocutorId = interlocutorId,
                MessageId = sentMessage.Id
            });
            await _context.DialoguesMessages.AddAsync(new DialoguesMessages
            {
                DialogueIndividualId = interlocutorId,
                DialogueInterlocutorId = userId,
                MessageId = sentMessage.Id
            });
            await _context.SaveChangesAsync();
        }

        public Conversation CreateConversation(int conversationId, int senderId, string lastMessage)
        {
            var conversation = new Conversation
            {
                Message = conversationId,
                Name = _cryptographyService.DecryptString(_context.Individuals.Find(conversationId).Name),
                LastMessage = new Message { SenderId = senderId, DecryptedMessage = lastMessage }
            };
            return conversation;
        }

        private bool IndividualExists(int id)
        {
            return _context.Individuals.Any(i => i.Id == id);
        }
        /*private bool DialoguesExists(Dialogues dialogues)
        {
            return _context.Dialogues.Any(d => d == dialogues);
        }*/
    }
}
