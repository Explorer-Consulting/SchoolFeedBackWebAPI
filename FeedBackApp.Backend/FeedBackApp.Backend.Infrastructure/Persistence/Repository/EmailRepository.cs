using FeedBackApp.Core.Model;
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
        public async Task<EmailsToSend?> GetEmailsDocumentAsync()
        {
            return await _context.EmailsToSend
                .FirstOrDefaultAsync(e => e.Id == "emailsToSend");
        }

        public async Task UpdateEmailsDocumentAsync(EmailsToSend doc)
        {
            _context.EmailsToSend.Update(doc);
            await _context.SaveChangesAsync();
        }
    }
}
