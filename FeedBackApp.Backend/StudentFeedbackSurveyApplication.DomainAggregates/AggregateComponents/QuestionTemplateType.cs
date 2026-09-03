namespace StudentFeedbackSurveyApplication.Domain.AggregateComponents
{
    /// <summary>
    /// Specifies the types of question templates that can be used in a survey or questionnaire.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the format of a question, such as single choice, multiple
    /// choice, open-ended, or Likert scale. The values help determine how responses are collected and presented to
    /// users.</remarks>
    public enum QuestionTemplateType
    {
        SingleChoice,
        MultipleChoice,
        OpenEnded,
        LikertScale,
        SingleChoiceWithOpenEnded
    }
}
