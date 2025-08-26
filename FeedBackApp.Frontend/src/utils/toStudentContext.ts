import type { StudentContext, EvaluationResponses } from "@/models/StudentContext";

const multiQuestions = new Set(["q20", "q21"]);

export function toStudentContext(raw: any): StudentContext {
    const subjects: string[] = [];
    const teachersBySubject: Record<string, string[]> = {};
    const evaluations: StudentContext["evaluations"] = [];

    if (!raw || !Array.isArray(raw.subjects)) {
        return {
            class: String(raw?.class ?? ""),
            subjects: [],
            teachersBySubject: {},
            evaluations: []
        };
    }

    for (const subj of raw.subjects) {
        const subjectName = typeof subj?.name === "string" ? subj.name : "";
        if (!subjectName) continue;

        subjects.push(subjectName);
        teachersBySubject[subjectName] = [];

        const teacherList = Array.isArray(subj.teachers) ? subj.teachers : [];
        for (const teacher of teacherList) {
            const teacherName = typeof teacher?.name === "string" ? teacher.name : "";
            if (!teacherName) continue;

            teachersBySubject[subjectName].push(teacherName);

            const responses: Record<string, string | string[]> = {};
            const answers = Array.isArray(teacher?.answers) ? teacher.answers : [];

            for (const answer of answers) {
                const qid = typeof answer?.questionId === "string" ? answer.questionId : "";
                if (!qid) continue;
                const ans = typeof answer?.answer === "string" ? answer.answer : "";

                responses[qid] = multiQuestions.has(qid)
                    ? (ans ? ans.split(",").map(s => s) : [])
                    : ans;
            }
            evaluations.push({
                id: String(teacher?.id ?? ""),
                subject: subjectName,
                teacher: teacherName,
                responses: responses as EvaluationResponses
            });
        }
    }

    return {
        class: String(raw.class ?? ""),
        subjects,
        teachersBySubject,
        evaluations,
    }
}