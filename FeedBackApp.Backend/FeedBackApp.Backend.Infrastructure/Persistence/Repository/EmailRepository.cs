using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly AppDBContext _context;

        private static short HOURLY_EMAIL_LIMTI = 20;

        public EmailRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<string>> GetEmailsToSend()
        {
            var emails = await _context.EmailsToSend.FindAsync("emailsToSend");

            if (emails == null || emails.Emails == null)
                return Enumerable.Empty<string>();

            return emails.Emails.Take(HOURLY_EMAIL_LIMTI);
        }

        public async Task RemoveEmailsAsync(IEnumerable<string> emails)
        {
            var record = await _context.EmailsToSend.FindAsync("emailsToSend");
            if (record == null) return;

            foreach (var email in emails)
                record.Emails.Remove(email);

            await _context.SaveChangesAsync();
        }
    }
}
