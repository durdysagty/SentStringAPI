using MessengerAPI.Models;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Generic.Dictionary<string, string>;

namespace MessengerAPI.Hubs
{
    [Authorize]
    public class SignalRHub : Hub
    {
        private readonly MessagesService _messagesService;

        public SignalRHub(MessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        /* //set interlocutor of current user
         public void SetInterlocutor(string inter) => Interlocutors.AddInterlocutors(Context.UserIdentifier, inter);
         public void RemoveInterlocutor() => Interlocutors.RemoveInterlocutors(Context.UserIdentifier);
         // get all users with current interlocutors. for testing purposes
         public ValueCollection GetInterlocutors() => Interlocutors.GetInterlocutors();

         public async Task Send(string message, string interlocutor)
         {
             await _messagesService.AddMessage(int.Parse(Context.UserIdentifier), int.Parse(interlocutor), message);
             // get interlocutor of current user interlocutor
             var userInterlocutor = Interlocutors.GetInterlocutor(interlocutor);
             var date = DateTimeOffset.UtcNow;
             if (userInterlocutor == Context.UserIdentifier)
             {
                 await Clients.User(interlocutor).SendAsync("Send", message, int.Parse(Context.UserIdentifier), date);
             }
             await Clients.User(Context.UserIdentifier).SendAsync("Send", message, int.Parse(Context.UserIdentifier), date);
         }*/


        public void AddToInConversations(int id) => InConversations.SetToList(id);
        public void RemoveFromInConversations(int id) => InConversations.RemoveFromList(id);

        public async Task AddToGroup(string interlocutorId)
        {
            string groupName = Interlocutors.GetInterlocutorsGroupName(Context.UserIdentifier, interlocutorId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string interlocutorId)
        {
            string groupName = Interlocutors.GetGroupName(Context.UserIdentifier, interlocutorId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Interlocutors.RemoveInterlocutorsFromGroupName(Context.UserIdentifier, interlocutorId);
        }

        public List<KeyValuePair<string, string>> GetInterlocutorsInGroup() => Interlocutors.GetInterlocutorsInGroup();

        public async Task Send(string message, string interlocutor)
        {
            await _messagesService.AddMessage(int.Parse(Context.UserIdentifier), int.Parse(interlocutor), message);
            string groupName = Interlocutors.GetGroupName(Context.UserIdentifier, interlocutor);
            var date = DateTimeOffset.UtcNow;
            await Clients.Group(groupName).SendAsync("Send", message, int.Parse(Context.UserIdentifier), date);
            if (InConversations.IdExists(int.Parse(interlocutor)))
                await Clients.User(interlocutor).SendAsync("updateChatsList", _messagesService.CreateConversation(int.Parse(Context.UserIdentifier), int.Parse(Context.UserIdentifier), message));
            if (InConversations.IdExists(int.Parse(Context.UserIdentifier)))
                await Clients.User(Context.UserIdentifier).SendAsync("updateChatsList", _messagesService.CreateConversation(int.Parse(interlocutor), int.Parse(Context.UserIdentifier), message));
        }
    }
}
