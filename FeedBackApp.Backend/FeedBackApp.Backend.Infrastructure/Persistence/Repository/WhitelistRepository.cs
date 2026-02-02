using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository that manages the single <see cref="StudentWhitelist"/> document
    /// stored in Cosmos DB via EF Core.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Storage model</b><br/>
    /// A single whitelist document is maintained with a fixed identifier
    /// (<c>"StudentWhitelist"</c>). If it does not exist, this repository will create it
    /// on first access to guarantee idempotent reads for callers.
    /// </para>
    /// <para>
    /// <b>Usage</b><br/>
    /// Use <see cref="GetStudentWhitelistAsync"/> to read (and lazily create) the whitelist,
    /// mutate the returned instance (e.g., add/remove emails), then persist the changes with
    /// <see cref="UpdateStudentWhitelistAsync(StudentWhitelist)"/>.
    /// </para>
    /// </remarks>
    public class WhitelistRepository : IWhitelistRepository
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initializes a new instance of the repository with the given EF Core context.
        /// </summary>
        /// <param name="context">Cosmos-configured application DbContext.</param>
        public WhitelistRepository(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the singleton student whitelist document, creating it if missing.
        /// </summary>
        /// <remarks>
        /// If no document with id <c>"StudentWhitelist"</c> is found, a new
        /// <see cref="StudentWhitelist"/> is instantiated, added to the context, and saved,
        /// ensuring subsequent calls always return a persistent instance.
        /// </remarks>
        /// <returns>
        /// The existing or newly created <see cref="StudentWhitelist"/> entity.
        /// </returns>
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

        /// <summary>
        /// Persists changes made to the provided student whitelist entity.
        /// </summary>
        /// <param name="studentWhitelist">The whitelist instance to update.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
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
