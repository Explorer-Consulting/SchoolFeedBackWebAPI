import type { EvaluationResponses } from "@/models/StudentContext";

export type BackendAnswer = { question: string; answer: string };
export type BackendPayload = {responses: BackendAnswer[];};

export function toBackendPayload(r: EvaluationResponses): BackendPayload {
  const responses: BackendAnswer[] = [];
  for (const [question, value] of Object.entries(r)) {
    const answer = Array.isArray(value)
      ? value.map((s) => String(s)).join(",")
      : String(value ?? "").trim();

    responses.push({ question, answer }); 
  }
  return {responses};
}
