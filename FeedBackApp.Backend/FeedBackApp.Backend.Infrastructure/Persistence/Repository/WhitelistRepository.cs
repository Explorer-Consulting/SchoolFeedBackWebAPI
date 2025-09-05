
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
            var whitelist = await _context.Set<StudentWhitelist>()
                .SingleOrDefaultAsync(s => s.Id == "StudentWhitelist");

            if (whitelist == null)
            {
                whitelist = new StudentWhitelist();
                _context.Add(whitelist);
                await _context.SaveChangesAsync();
            }

            return whitelist;
        }

        public async Task UpdateStudentWhitelistAsync(StudentWhitelist studentWhitelist)
        {
            _context.Set<StudentWhitelist>().Update(studentWhitelist);
            await _context.SaveChangesAsync();
        }
    }
}
