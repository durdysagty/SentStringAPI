using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerAPI.Models
{
    public class MessengerAPIDbContext : DbContext
    {
        public MessengerAPIDbContext(DbContextOptions<MessengerAPIDbContext> options)
     : base(options)
        {
        }

        public virtual DbSet<Individuals> Individuals { get; set; }
        public virtual DbSet<Friends> Friends { get; set; }
        public virtual DbSet<Dialogues> Dialogues { get; set; }
        public virtual DbSet<DialoguesMessages> DialoguesMessages { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<Confirmations> Confirmations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Individuals>().HasIndex(i => i.PublicId).IsUnique();
            builder.Entity<Friends>().HasKey(f => new { f.IndividualId, f.FriendId });
            builder.Entity<Dialogues>().HasKey(d => new { d.IndividualId, d.InterlocutorId });
            builder.Entity<DialoguesMessages>().HasKey(d => new { d.DialogueIndividualId, d.DialogueInterlocutorId, d.MessageId });
        }
    }
}
