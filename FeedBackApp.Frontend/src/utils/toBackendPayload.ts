import type { EvaluationResponses } from "@/models/StudentContext";

export type BackendAnswer = { questionId: string; answer: string };
export type BackendPayload = { questionnaireResult: BackendAnswer[]; };

export function toBackendPayload(r: EvaluationResponses): BackendPayload {
  const questionnaireResult: BackendAnswer[] = [];
  for (const [questionId, value] of Object.entries(r)) {
    const answer = Array.isArray(value)
      ? value.map((s) => String(s)).join("-")
      : String(value ?? "").trim();

    questionnaireResult.push({ questionId, answer });
  }
  return { questionnaireResult };
}