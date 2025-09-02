using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.ReportDocumentTypes;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public sealed class ReportRepository(AppDBContext context) : IReportRepository
    {
        private readonly AppDBContext _context = context;
        public async Task CompileAndSaveEvaluationReports()
        {
            /*
             * I. elso megoldas
             * lekerjuk a valaszokat egy Dictionarybe
             * ez batch-elt beolvasas, de a vegen amugy is minden benne lesz a memoriaba, valo igaz.
             * allitolag ez jo lesz.
             */

            var questionnaireAnswerCollection = await _context.Questionnaires
                .AsNoTracking()
                .Select(q => new
                {
                    q.TeacherEmail,
                    q.SubjectName,
                    Results = q.QuestionnaireResults
                        .Select(r => new QuestionAnswer
                        {
                            QuestionId = r.QuestionId,
                            Answer = r.Answer
                        })
                        .ToList()
                })
                .ToListAsync();

            var groupedCollection = questionnaireAnswerCollection
                .GroupBy(x => (x.TeacherEmail, x.SubjectName))
                .ToImmutableDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.Results)
                            .Where(r => !string.IsNullOrWhiteSpace(r.Answer))
                            .ToImmutableList()
                );
            await foreach(var a in EvaluationReportCompiler.RenderReports())
            {

            }
        }

        public async Task DeleteAllEvaluationReports()
        {
            /*
             ugyanaz mind mashol csak maskepp
             */
            throw new NotImplementedException();
        }

        public async Task DeleteEvaluationReport(string id)
        {
            /*
             I. megkeressuk a megfelelo id-ju DocumentMetadata objektumot
            II. megkeressuk a BLOB-t....blablabla
             */
            throw new NotImplementedException();
        }

        // ide nem byte[] lesz hanem egy abstract dokumentum tipus (byte[] tombot terit vissza)
        public async IAsyncEnumerable<ReportDocument> RetrieveAllEvaluationReports()
        {
            /*
             I. itt kinyerjuk az osszes DocumentMetadata id-t
            II. itt kinyerjuk a megfelelo BLOB-okat
             */
            throw new NotImplementedException();
        }

        public async Task<ReportDocument> RetrieveEvaluationReport(string id)
        {
            /*
            I.  eloszor a CosmosDB-ben megkeressuk a megfelelo azonositoju DocumentMetadata objektumot
            II. a kinyert id alapjan megkeressuk a megfelelo BLOB-t.
             */
            throw new NotImplementedException();
        }

        public async Task StoreEvaluationReport(ReportDocument document)
        {
            /*
             I. itt majd lesz egy ReportDocument.ReportMetadata
             ezt el kell menteni CosmosDB-be.
             */
            /*
             II. itt majd el kell menteni BLOB-ba magat a dokumentumot
             */
            await _context.SaveChangesAsync();
        }
    }
}
