export type QuestionType =
  | "LikertScaleOneToFive"          
  | "MultinomialSingleChoice"       
  | "MultinomialSingleChoiceOther"  
  | "MultipleChoice"                
  | "OpenEnded";

export type QuestionID = `q${number}`;

export type QuestionDependency = {
  id:QuestionID;
  answerConditions: string[];
}

export type Question = {
  id: QuestionID;          
  text: string;        
  type: QuestionType;
  dependency?:QuestionDependency
  options?: string[];
  category:string;
  description?:string;
};

export type Survey = {
    id: string
    title: string
    endDate: string
}

export type EvaluationResponses = Record<QuestionID, string | string[]>;
export type Evaluation = {
    id: string;
    subject: string;
    teacher: string;
    questions:Question[];
    responses: EvaluationResponses;
}

export type StudentContext = {
    class: string;
    subjects: string[];
    teachersBySubject: Record<string, string[]>;
    evaluations: Evaluation[];
}