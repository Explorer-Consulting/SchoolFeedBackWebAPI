import type { EvaluationResponses } from "@/models/StudentContext";

export type BackendAnswer = { question: string; answer: string };
export type BackendPayload = {status: string ; responses: BackendAnswer[];};

export function toBackendPayload(r: EvaluationResponses,status: string): BackendPayload {
  const responses: BackendAnswer[] = [];
  for (const [question, value] of Object.entries(r)) {
    const answer = Array.isArray(value)
      ? value.map((s) => String(s)).join(",")
      : String(value ?? "").trim();

    responses.push({ question, answer }); 
  }
  return {status,responses};
}
