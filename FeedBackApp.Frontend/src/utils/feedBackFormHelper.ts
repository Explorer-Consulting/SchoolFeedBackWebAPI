import { EvaluationResponses, Question } from "@/models/StudentContext"

export const isMulti = (q: Question) => q.type === "MultipleChoice";

export function ensureInitialAnswers(
    questions: Question[],
    responses?: EvaluationResponses,
): EvaluationResponses {
    const out: EvaluationResponses = {} as EvaluationResponses;
    for (const q of questions) {
        const existing = responses?.[q.id];
        if (existing !== undefined) {
            out[q.id] = existing;
        } else {
            out[q.id] = isMulti(q) ? [] : "";
        }
    }
    return out;
}

export function shouldShowQuestion(q: Question, answers: EvaluationResponses): boolean {
    if (!q.dependency) return true;

    const { id, answerConditions } = q.dependency;
    const raw = answers[id];

    const chosen: string[] = Array.isArray(raw)
        ? raw.map(String)
        : raw ? [String(raw)] : [];
    return chosen.some(v => answerConditions.map(String).includes(v));
}

export function deleteHiddenAnswers(all: Question[], vis: Question[], answers: EvaluationResponses) {
    const visibleIds = new Set(vis.map(q => q.id));
    const out = { ...answers };
    for (const q of all) {
        if (!visibleIds.has(q.id)) {
            out[q.id] = isMulti(q) ? [] : "";
        }
    }
    return out;
}

export function isNewCategory(list: Question[], idx: number) {
    const curr = list[idx];
    if (!curr.category) return false;
    if (idx === 0) return true;
    const prev = list[idx - 1];
    return prev?.category !== curr.category;
}