using MessengerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MessengerAPI.Services
{
    public class TimerService
    {
        private readonly IConfiguration _configuration;

        public TimerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CancelUsersPasswordRestore(int id)
        {
            var timer = new Timer(1800000) { AutoReset = false, Enabled = true };
            timer.Elapsed += async (sender, e) =>
            {
                var contextOptions = new DbContextOptionsBuilder<MessengerAPIDbContext>()
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                    .Options;
                using (MessengerAPIDbContext _context = new MessengerAPIDbContext(contextOptions))
                {
                    var user = await _context.Individuals.FindAsync(id);
                    if (user != null && user.IsRestoring == true)
                    {
                        user.IsRestoring = false;
                        await _context.SaveChangesAsync();
                    }
                };
            };
        }
    }
}
