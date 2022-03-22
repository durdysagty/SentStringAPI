using System.Collections.Generic;

namespace MessengerAPI.Models
{
    public class Friends
    {
        public int IndividualId { get; set; }
        public int FriendId { get; set; }
        public int FriendsPublicId { get; set; }
        public virtual Individuals Individual { get; set; }
    }
}
