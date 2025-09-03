using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Questionnaire.Post
{
    public class DependencyDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("answerConditions")]        
        public List<int> AnswerConditions { get; set; } = new List<int>();
    }
}
