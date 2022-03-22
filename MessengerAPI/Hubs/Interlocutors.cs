using System.Collections.Generic;
using System.Linq;

namespace MessengerAPI.Hubs
{
    public static class Interlocutors
    {
        //private static readonly Dictionary<string, string> _interlocutors = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _groups = new Dictionary<string, string>();

        /*public static void AddInterlocutors(string userId, string interlocutorId)
        {
            if (_interlocutors.ContainsKey(userId))
                _interlocutors[userId] = interlocutorId;
            else
                _interlocutors.Add(userId, interlocutorId);
        }

        public static void RemoveInterlocutors(string userId)
        {
            if (_interlocutors.ContainsKey(userId))
                _interlocutors.Remove(userId);
        }

        public static string GetInterlocutor(string userId)
        {
            string value;
            try
            {
                _interlocutors.TryGetValue(userId, out value);
            }
            catch
            {
                value = "0";
            }
            return value;
        }

        public static ValueCollection GetInterlocutors()
        {
            var collection = _interlocutors.Values;
            return collection;
        }*/


        public static string GetInterlocutorsGroupName(string userId, string interlocutorId)
        {
            string ids = interlocutorId + '.' + userId;
            string groupName;
            if (_groups.ContainsKey(ids))
                _groups.TryGetValue(ids, out groupName);
            else
                groupName = userId + '.' + interlocutorId;
            ids = userId + '.' + interlocutorId;
            if (!_groups.ContainsKey(ids))
                _groups.Add(ids, groupName);
            return groupName;
        }

        public static void RemoveInterlocutorsFromGroupName(string userId, string interlocutorId)
        {
            string ids = userId + '.' + interlocutorId;
            if (_groups.ContainsKey(ids))
                _groups.Remove(ids);
        }

        public static string GetGroupName(string userId, string interlocutorId)
        {
            string ids = userId + '.' + interlocutorId;
            string groupName;
            if (_groups.ContainsKey(ids))
                _groups.TryGetValue(ids, out groupName);
            else
            {
                ids = interlocutorId + '.' + userId;
                if (_groups.ContainsKey(ids))
                    _groups.TryGetValue(ids, out groupName);
                else
                    groupName = userId + '.' + interlocutorId;
                ids = userId + '.' + interlocutorId;
                if (!_groups.ContainsKey(ids))
                    _groups.Add(ids, groupName);
            }
            return groupName;
        }
        public static List<KeyValuePair<string, string>> GetInterlocutorsInGroup()
        {
            var collection = _groups.ToList();
            return collection;
        }
    }
}
