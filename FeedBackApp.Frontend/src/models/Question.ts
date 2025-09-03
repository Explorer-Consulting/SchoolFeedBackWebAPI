export type QuestionType =
   | "LikertScaleOneToFive"
   | "MultinomialSingleChoiceOther"
   | "MultinomialSingleChoice"
   | "MultipleChoice"
   | "OpenEnded" ;

export type Question = {
    id:string;
    text: string;
    type:QuestionType;
    options?:string[];
}; 

export type AnswerValue= string | string[];

export type Answer= Record<string,AnswerValue>;