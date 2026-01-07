
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public class WhitelistRepository : IWhitelistRepository
    {
        private readonly AppDBContext _context;

        public WhitelistRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<StudentWhitelist> GetStudentWhitelistAsync()
        {
            var whitelist = await _context.StudentWhitelist
                .SingleOrDefaultAsync(s => s.Id == "StudentWhitelist");

            if (whitelist == null)
            {
                whitelist = new StudentWhitelist();
                _context.Add(whitelist);
                await _context.SaveChangesAsync();
            }

            return whitelist;
        }

        public async Task<IReadOnlyList<string>> GetStudentEmailsAsync(string id = "StudentWhitelist", CancellationToken ct = default)
        {
            var doc = await _context.Set<StudentWhitelistDoc>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (doc.StudentEmails == null)
            {
                return Array.Empty<string>();
            }
            return doc.StudentEmails;
        }

        public async Task UpdateStudentWhitelistAsync(StudentWhitelist studentWhitelist)
        {
            _context.StudentWhitelist.Update(studentWhitelist);
            await _context.SaveChangesAsync();
        }
    }
    
    public class StudentWhitelistDoc
    {
        public string Id { get; set; } = "StudentWhitelist";
        public string DocumentType { get; set; } = "StudentWhitelist";
        public List<string> StudentEmails { get; set; } = new();
    }
}
