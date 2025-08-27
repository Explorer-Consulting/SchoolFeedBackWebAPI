using Application.DTOs.Evaluation;

namespace Application.Services.Interfaces
{
    public interface IEvaluationService
    {
        public Task<UpdateResponseDTO> UpdateQuestionnaire(string id, UpdateQuestionnaireDTO dto);
    }
}
