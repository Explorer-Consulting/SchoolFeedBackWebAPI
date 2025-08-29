using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly AppDBContext _context;

        public EmailRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<string>> GetEmailsToSend()
        {
            var emails = await _context.EmailsToSend.FindAsync("emailsToSend");

            if (emails == null || emails.EmailToSend == null)
                return Enumerable.Empty<string>();

            return emails.EmailToSend.Take(20);
        }
    }
}
