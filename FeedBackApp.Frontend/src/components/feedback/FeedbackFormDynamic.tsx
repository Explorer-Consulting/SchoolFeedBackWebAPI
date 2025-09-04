import { useMemo, useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { toast } from "sonner";
import { Evaluation, EvaluationResponses, Question, QuestionID } from "@/models/StudentContext"
import { toBackendPayload } from "@/utils/toBackendPayload";
import { useReviews } from "@/hooks/useReviews";
import { useStudentContextStore } from "@/hooks/useStudentContext";
import DynamicQuestion from "@/components/feedback/DynamicQuestion"

const isMulti = (q: Question) => q.type === "MultipleChoice";

function ensureInitialAnswers(
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

function shouldShowQuestion(q: Question, answers: EvaluationResponses): boolean {
    if (!q.dependency) return true;

    const { id, answerConditions } = q.dependency;
    const raw = answers[id];

    const chosen: string[] = Array.isArray(raw)
        ? raw.map(String)
        : raw ? [String(raw)] : [];
    return chosen.some(v => answerConditions.map(String).includes(v));
}

function deleteHiddenAnswers(all: Question[], vis: Question[], answers: EvaluationResponses) {
    const visibleIds = new Set(vis.map(q => q.id));
    const out = { ...answers };
    for (const q of all) {
        if (!visibleIds.has(q.id)) {
            out[q.id] = isMulti(q) ? [] : "";
        }
    }
    return out;
}

type FeedbackFormDynamicProps = {
    subjects: string[];
    teachersBySubject: Record<string, string[]>;
    evaluations: Evaluation[];
    onAfterChange: () => void;
}

export function FeedbackFormDynamic({
    subjects,
    teachersBySubject,
    evaluations,
    onAfterChange }: FeedbackFormDynamicProps) {

    const {
        performQuestionnaireUpdate,
        isPerformQuestionnaireUpdating,
        performQuestionnaireSubmit,
        isPerformQuestionnaireSubmit
    } = useReviews();

    const {
        selectedSubject: subject,
        setSelectedSubject: setSubject,
        selectedTeacher: teacher,
        setSelectedTeacher: setTeacher
    } = useStudentContextStore();

    const onSubjectChange = (s: string) => {
        setSubject(s);
        setTeacher(null);
    };

    const [answers, setAnswers] = useState<EvaluationResponses>({});
    const [invalidIds, setInvalidIds] = useState<Set<QuestionID>>(new Set());


    const teachersForSubject = useMemo(
        () => (subject ? (teachersBySubject[subject] ?? []) : []),
        [subject, teachersBySubject]
    );

    const currentEvaluation = useMemo(
        () =>
            evaluations?.find(
                (e) => e.subject === subject && e.teacher === teacher
            ),
        [evaluations, subject, teacher]
    );

    const visibleQuestions = useMemo(
        () =>
            currentEvaluation
                ? currentEvaluation.questions.filter(q => shouldShowQuestion(q, answers))
                : [],
        [currentEvaluation, answers]
    );

    const id = currentEvaluation?.id;

    function validateAll(questions: Question[], answers: EvaluationResponses): { msg: string | null; invalid: Set<QuestionID> } {
        const invalid = new Set<QuestionID>();
        let msg: string | null = null;

        for (let i = 0; i < questions.length; i++) {
            const q = questions[i];
            const ind = i + 1;
            const val = answers[q.id];

            let isInvalid = false;

            if (q.type === "MultipleChoice") {
                isInvalid = !Array.isArray(val) || val.length === 0;
            } else if (q.type === "OpenEnded") {
                const text = String(val ?? "").trim();
                isInvalid = text.length < 20;
            } else {
                const text = String(val ?? "").trim();
                isInvalid = text === "";
            }

            if (isInvalid) {
                invalid.add(q.id);
                if (!msg) {
                    msg = q.type === "OpenEnded"
                        ? `A ${ind}. kérdésnél a válasz legyen legalább 20 karakter.`
                        : `Kérjük, válaszolj a(z) ${ind}. kérdésre.`;
                }
            }
        }
        return { msg, invalid };
    }

    useEffect(() => {
        if (!currentEvaluation) {
            setAnswers({} as EvaluationResponses);
            return;
        }
        setAnswers(ensureInitialAnswers(currentEvaluation.questions, currentEvaluation.responses));
    }, [currentEvaluation]);

    const onSaveDraft = () => {
        if (!id) return;
        if (!subject || !teacher) {
            toast("Kérjük, válaszd ki a tantárgyat és a tanárt.");
            return;
        }

        const cleaned = deleteHiddenAnswers(currentEvaluation.questions, visibleQuestions, answers);
        const payload = toBackendPayload(cleaned);
        performQuestionnaireUpdate(
            { id, payload },
            {
                onSuccess: () => {
                    toast("Piszkozat sikeresen mentve!");
                    onAfterChange();
                },
                onError: () => { toast("Hiba történt a piszkozat mentése közben!"); }
            }
        )
    };

    const onSubmit = () => {
        if (!id) return;

        const { msg, invalid } = validateAll(visibleQuestions, answers);
        if (msg) {
            setInvalidIds(invalid);
            toast.error(msg);
            return;
        }

        const cleaned = deleteHiddenAnswers(currentEvaluation.questions, visibleQuestions, answers);
        const payload = toBackendPayload(cleaned);
        performQuestionnaireSubmit(
            { id, payload },
            {
                onSuccess: () => {
                    toast("Kérdőív beküldve!");
                    onAfterChange();
                },
                onError: () => { toast("Hiba történt a beküldés közben!"); }
            }
        )
    };

    useEffect(() => {
        const handleBeforeUnload = (e: BeforeUnloadEvent) => {
            onSaveDraft();
            e.preventDefault();
        };
        window.addEventListener("beforeunload", handleBeforeUnload);
        return () => {
            window.removeEventListener("beforeunload", handleBeforeUnload);
        };
    });

    return (
        <Card>
            <CardHeader>
                <CardTitle>Oktatási visszajelzés</CardTitle>
            </CardHeader>
            <CardContent className="space-y-8">
                <section className="grid gap-4 md:grid-cols-3">
                    <div className="space-y-2">
                        <Label htmlFor="subject">Tantárgy</Label>
                        <Select value={subject ?? ""} onValueChange={onSubjectChange}>
                            <SelectTrigger id="subject">
                                <SelectValue placeholder="Válassz tantárgyat" />
                            </SelectTrigger>
                            <SelectContent>
                                {subjects.map((s) => (
                                    <SelectItem key={s} value={s}>{s}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="teacher">Tanár</Label>
                        <Select value={teacher ?? ""} onValueChange={setTeacher} disabled={!subject}>
                            <SelectTrigger id="teacher">
                                <SelectValue placeholder="Válassz tanárt" />
                            </SelectTrigger>
                            <SelectContent>
                                {teachersForSubject.map((t) => (
                                    <SelectItem key={t} value={t}>{t}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                </section>

                {currentEvaluation ? (
                    <section className="space-y-6">
                        {visibleQuestions.map((q, idx) => (
                            <DynamicQuestion
                                key={q.id}
                                q={q}
                                index={idx + 1}
                                value={answers[q.id] ?? (isMulti(q) ? [] : "")}
                                isInvalid={invalidIds.has(q.id)}
                                onChange={(val) => {
                                    setAnswers((prev) => ({ ...prev, [q.id as QuestionID]: val }));

                                    setInvalidIds((prev) => {
                                        if (!prev.size) return prev;
                                        if (!prev.has(q.id)) return prev;
                                        const next = new Set(prev);
                                        next.delete(q.id as QuestionID);
                                        return next;
                                    });
                                }}
                            />
                        ))}
                    </section>
                ) : (
                    <div className="text-muted-foreground">
                        Válassz tantárgyat és tanárt a kérdőív megjelenítéséhez.
                    </div>
                )}

                <div className="mt-4 sm:mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4">
                    <Button className="w-full sm:w-auto" variant="secondary" onClick={onSaveDraft} disabled={isPerformQuestionnaireUpdating}>Piszkozat mentése</Button>
                    <Button className="w-full sm:w-auto" variant="default" onClick={onSubmit} disabled={isPerformQuestionnaireSubmit}>Beküldés</Button>
                </div>
            </CardContent>
        </Card>
    );
}
