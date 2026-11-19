import { EvaluationResponses, Question, QuestionID } from "@/models/StudentContext"

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

export function clearFilteredChildren(
  questions: Question[],
  changedId: QuestionID,
  answers: EvaluationResponses
): EvaluationResponses {
  const out = { ...answers };
  const queue: QuestionID[] = [changedId];

  while (queue.length > 0) {
    const parentId = queue.shift()!;
    for (const q of questions) {
      if (q.type === "FilteredDropDown" && q.parentQuestionId === parentId) {
        out[q.id] = "";
        queue.push(q.id);
      }
    }
  }
  return out;
}

export function getVisibleQuestions(
  questions: Question[],
  answers: EvaluationResponses
): Question[] {
  const byId = new Map(questions.map((q) => [q.id, q] as const));
  const memo = new Map<QuestionID, boolean>();

  const isVisible = (id: QuestionID): boolean => {
    const cached = memo.get(id);
    if (cached !== undefined) return cached;

    const q = byId.get(id);
    let visible = !!q; // ismeretlen id → nem látszik

    if (q?.dependency) {
      // csak akkor látszik, ha a forráskérdés is látszik ÉS a feltétel teljesül
      visible = isVisible(q.dependency.id) && shouldShowQuestion(q, answers);
    }
    if (visible && q?.parentQuestionId) {
      // szűrt dropdown: ha a szülő rejtett, a gyerek is az
      visible = isVisible(q.parentQuestionId);
    }

    memo.set(id, visible);
    return visible;
  };

  return questions.filter((q) => isVisible(q.id));
}