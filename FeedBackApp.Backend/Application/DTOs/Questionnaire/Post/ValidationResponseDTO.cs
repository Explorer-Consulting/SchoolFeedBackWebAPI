using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Questionnaire.Post
{
    public class ValidationResponseDTO : BaseResponseDTO
    {
        public ValidationResponseDTO(bool success, string message) : base(success, message) { }
    }
}
