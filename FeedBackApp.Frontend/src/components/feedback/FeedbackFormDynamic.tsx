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
const isOpen = (q: Question) => q.type === "OpenEnded";

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

    const id = currentEvaluation?.id;

    function validateAll(questions: Question[], answers: EvaluationResponses):string | null {
        for (let i = 0; i < questions.length; i++) {
            const q=questions[i];
            const ind=i+1;
            const v = answers[q.id];

            if (isMulti(q)) {
                if (!Array.isArray(v) || v.length === 0) {
                    return `Kérjük, válaszolj a(z) ${ind}. kérdésre.`;
                }
            } else if (isOpen(q)) {
                const s = String(v ?? "");
                if (s.trim().length < 20) {
                    return `A ${ind}. kérdésnél a válasz legyen legalább 20 karakter.`;
                }
            } else {
                const s = String(v ?? "");
                if (!s) {
                    return `Kérjük, válaszolj a(z) ${ind}. kérdésre."`;
                }
            }
        }
        return null;
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

        const payload = toBackendPayload(answers);
        console.log(payload);
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

        const err = validateAll(currentEvaluation.questions, answers);
        if(err) {
            console.log(err);
            toast.error(err);
            return;
        }

        const payload = toBackendPayload(answers);
        console.log(payload);
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
                        {currentEvaluation.questions.map((q,idx) => (
                            <DynamicQuestion
                                key={q.id}
                                q={q}
                                index={idx +1}
                                value={answers[q.id] ?? (isMulti(q) ? [] : "")}
                                onChange={(val) =>
                                    setAnswers((prev) => ({ ...prev, [q.id as QuestionID]: val }))
                                }
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
