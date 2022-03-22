using System.Collections.Generic;
using System.Linq;

namespace MessengerAPI.Hubs
{
    public static class InConversations
    {
        private static readonly Dictionary<int, int> _list = new Dictionary<int, int>();

        public static void SetToList(int id)
        {
            if (_list.ContainsKey(id))
                _list[id]++;
            else
                _list.Add(id, 1);
        }

        public static void RemoveFromList(int id)
        {
            if (_list.ContainsKey(id))
                if (_list[id] < 2)
                    _list.Remove(id);
                else
                    _list[id]--;
        }

        public static bool IdExists(int id) => _list.ContainsKey(id);
    }
}
