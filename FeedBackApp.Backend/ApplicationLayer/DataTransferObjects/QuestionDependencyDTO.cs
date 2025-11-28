namespace ApplicationLayer.DataTransferObjects
{
    public record QuestionDependencyDTO
    {
        public required string QuestionID { get; init; }// ID of the question this dependency refers to

        public required ICollection<string> ExpectedAnswerIndexes { get; init; } // Answers that trigger this
    }
}
