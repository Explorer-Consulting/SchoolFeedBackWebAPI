using FeedBackApp.Backend.Infrastructure.Configuration;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using FeedBackApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Repository
{
    public sealed class ReportRepository(
        AppDBContext context, 
        IBlobContext blob,
        IOptions<InstitutionOptions> institutionOptions) : IReportRepository
    {
        private readonly AppDBContext _context = context;
        private readonly IBlobContext _blob = blob;

        private readonly IOptions<InstitutionOptions> _institutionOptions =  institutionOptions;

        public async Task CompileAndStoreEvaluationReports(string fullTemplateId)
        {
            const string prefix = "questiontemplates_";
            if (!fullTemplateId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid template ID format.", nameof(fullTemplateId));

            if (!Guid.TryParse(fullTemplateId[prefix.Length..], out var surveyGuid))
                throw new ArgumentException("Invalid GUID in template ID.", nameof(fullTemplateId));

            var surveyId = surveyGuid.ToString("D");
            var templateDocId = fullTemplateId;

            #region FIXING RESULT QUERIES
            var questionnaires = await _context.Questionnaires
                .AsNoTracking()
                .Where(q => q.SurveyId == surveyId && q.Status)
                .ToListAsync();

            ImmutableDictionary<Teacher, ImmutableArray<QuestionnaireSubmission>> answerCollection =
                questionnaires
                    .GroupBy(q => new Teacher(
                        q.TeacherEmail,
                        q.SubjectName
                    ))
                    .ToImmutableDictionary(
                        g => g.Key,
                        g => g.Select(q => new QuestionnaireSubmission
                        {
                            IsValidate = q.IsValidate,
                            QuestionnaireResults = q.QuestionnaireResults.ToList(),
                        }).ToImmutableArray()
                    );
            
            ImmutableArray<QuestionTemplate> questions;
            {
                var template = await _context.QuestionnaireTemplates
                    .AsNoTracking()
                    .Where(x => x.Id == templateDocId)
                    .SingleOrDefaultAsync();
                questions = [.. (template?.QuestionTemplates ?? [])];
            }
            #endregion
            // 3) Generálás + feltöltés

            await foreach (var document in EvaluationReportCompiler.CompileReports(answerCollection, questions, surveyId, _institutionOptions.Value.DisplayName))
            {
                var fileName = $"{surveyId}_{document.Metadata.FileName}";

                if (document.Recipient is null)
                {
                    await _blob.UploadAdminAsync(fileName, document.Data, document.Metadata.MimeType);
                }
                else
                {
                    await _blob.UploadTeacherAsync(document.Recipient.EmailAddress, fileName, document.Data, document.Metadata.MimeType);
                }
            }
            
        }
    }
}
