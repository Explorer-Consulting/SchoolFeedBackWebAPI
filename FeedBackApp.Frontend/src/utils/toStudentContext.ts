import type {
    StudentContext,
    Evaluation,
    EvaluationResponses,
    Question,
    QuestionID,
    QuestionType,
} from "@/models/StudentContext";

const type_map: Record<number, QuestionType> = {
    1: "LikertScaleOneToFive",
    2: "MultinomialSingleChoice",
    3: "MultinomialSingleChoiceOther",
    4: "MultipleChoice",
    5: "OpenEnded",
};

type RawQuestion = {
    questionID: QuestionID;
    question: string;
    type: number;
    answerOptions: string[];
    answer: string;
};

type RawTeacher = {
    name: string;
    id: string;
    questions: RawQuestion[];
};

type RawSubject = {
    name: string;
    teachers: RawTeacher[];
};

type RawPayload = {
    class: string;
    subjects: RawSubject[];
};

export function toStudentContext(raw: RawPayload): StudentContext {
    const evaluations: Evaluation[] = [];

    const subjectToTeachers = new Map<string, Set<string>>();

    const subjectList: RawSubject[] = Array.isArray(raw?.subjects) ? raw.subjects : [];

    for (const subj of subjectList) {
        const subjectName = typeof subj?.name === "string" ? subj.name : "";
        if (!subjectName) continue;

        const teacherSet = subjectToTeachers.get(subjectName) ?? new Set<string>();
        const teacherList: RawTeacher[] = Array.isArray(subj?.teachers) ? subj.teachers : [];

        for (const t of teacherList) {
            const teacherName = typeof t?.name === "string" ? t.name : "";
            if (!teacherName) continue;

            teacherSet.add(teacherName);

            const rawQs: RawQuestion[] = Array.isArray(t?.questions) ? t.questions : [];

            const questions: Question[] = [];
            const responses: EvaluationResponses = {};

            rawQs.forEach((rq) => {
                const id = rq?.questionID;
                const tpe = type_map[rq?.type as number];
                const text = typeof rq?.question === "string" ? rq.question : "";
                if (!id || !tpe || !text) return;

                questions.push({
                    id,
                    text,
                    type: tpe,
                    options:
                        tpe === "MultinomialSingleChoice" ||
                            tpe === "MultinomialSingleChoiceOther" ||
                            tpe === "MultipleChoice"
                            ? (Array.isArray(rq?.answerOptions) ? rq.answerOptions : [])
                            : undefined,
                });

                const ansRaw = typeof rq?.answer === "string" ? rq.answer : "";
                responses[id] =
                    tpe === "MultipleChoice"
                        ? (ansRaw ? ansRaw.split("-").filter(Boolean) : [])
                        : ansRaw;
            });

            evaluations.push({
                id: String(t?.id ?? ""),
                subject: subjectName,
                teacher: teacherName,
                questions,
                responses,
            });
        }

        if (teacherSet.size > 0) {
            subjectToTeachers.set(subjectName, teacherSet);
        }
    }

    const subjects = Array.from(subjectToTeachers.keys()); 
    const teachersBySubject = Object.fromEntries(
        Array.from(subjectToTeachers.entries()).map(([subj, set]) => [subj, Array.from(set)])
    );

    return {
        class: String(raw?.class ?? ""),
        subjects,
        teachersBySubject,
        evaluations,
    };
}
