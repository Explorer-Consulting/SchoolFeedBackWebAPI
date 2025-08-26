export type EvaluationResponses ={
    [key: `q${number}`]: string | string[];
}

export type Evaluation = {
    id: string;
    subject: string;
    teacher: string;
    responses: EvaluationResponses;
}

export type StudentContext = {
    class: string;
    subjects: string[];
    teachersBySubject:Record<string,string[]>;
    evaluations: Evaluation[];
}