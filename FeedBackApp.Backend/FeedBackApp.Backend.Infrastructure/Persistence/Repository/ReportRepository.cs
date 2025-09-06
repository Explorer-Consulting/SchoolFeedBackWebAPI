using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public sealed class ReportRepository(AppDBContext context) : IReportRepository
    {
        private readonly AppDBContext _context = context;
        public async Task CompileAndStoreEvaluationReports()
        {
            // 1) Kérdőívek lekérdezése és Teacher rekordokba csomagolás
            var rows = await _context.Questionnaires
                .AsNoTracking()
                .Where(q => q.Status)
                .Select(q => new
                {
                    Teacher = new Teacher(q.TeacherEmail, q.SubjectName),
                    Results = q.QuestionnaireResults
                        .Select(r => new QuestionAnswer
                        {
                            QuestionId = r.QuestionId,
                            Answer = r.Answer
                        })
                })
                .ToListAsync();

            // 2) ImmutableDictionary<Teacher, ImmutableArray<QuestionAnswer>>
            var answerCollection = rows
                .GroupBy(x => x.Teacher)
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results).ToImmutableArray()
                );

            // 3) Kérdés-sablonok
            string surveyID = "questiontemplates_8daeb772-15d1-4e0a-a75b-4c033d1dc319";
            var questions = (await _context.QuestionnaireTemplates
                    .AsNoTracking()
                    .Where(qt => qt.Id == surveyID)
                    .SelectMany(qt => qt.QuestionTemplates)
                    .ToListAsync())
                .ToImmutableArray();

            // 4) Administrator lista → ImmutableList<Administrator>
            var administratorData = Environment.GetEnvironmentVariable("AdminEmails");
            ArgumentNullException.ThrowIfNull(administratorData);

            var administrators = administratorData
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(email => new Administrator(email))
                .ToImmutableArray();

            // 5) Jelentések generálása
            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, administrators))
            {
                ReportMetadata metaData = document.Metadata;
                // I. mentjuk a metadatat;
                // II. mentjuk a BLOB-ba
            }
        }
    }
}
