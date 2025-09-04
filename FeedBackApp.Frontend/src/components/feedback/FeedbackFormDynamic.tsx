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

function isNewCategory(list: Question[], idx: number) {
    const curr = list[idx];
    if (!curr.category) return false;
    if (idx === 0) return true;
    const prev = list[idx - 1];
    return prev?.category !== curr.category;
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

    // subject automatikus választása
    useEffect(() => {
        if (subjects.length > 0) {
            if (!subject || !subjects.includes(subject)) {
                setSubject(subjects[0]);
            }
        } else {
            setSubject(null);
        }
    }, [subjects, subject, setSubject]);

    // teacher automatikus választása, ha subject változik
    useEffect(() => {
        if (subject) {
            const teachers = teachersBySubject[subject] ?? [];
            if (teachers.length > 0) {
                if (!teacher || !teachers.includes(teacher)) {
                    setTeacher(teachers[0]);
                }
            } else {
                setTeacher(null);
            }
        } else {
            setTeacher(null);
        }
    }, [subject, teachersBySubject, teacher, setTeacher]);

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

    const onSaveDraft = () => {
        if (!id) return;

        if (!subject || !teacher) {
            toast.warning("Kérjük, válaszd ki a tantárgyat és a tanárt.");
            return;
        }

        const cleaned = deleteHiddenAnswers(currentEvaluation.questions, visibleQuestions, answers);
        const payload = toBackendPayload(cleaned);
        performQuestionnaireUpdate(
            { id, payload },
            {
                onSuccess: () => {
                    toast.success("Piszkozat sikeresen mentve!");
                    document.getElementById("topList")?.scrollIntoView({
                        behavior: "smooth",
                        block: "start"
                    });
                    onAfterChange();
                },
                onError: () => { toast.error("Hiba történt a piszkozat mentése közben!"); }
            }
        )
    };

    const onSubmit = () => {
        if (!id) return;

        const confirmed = window.confirm("Biztosan be szeretnéd küldeni a kérdőívet?");
        if (!confirmed) return;

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
                    toast.success("Kérdőív beküldve!");
                    document.getElementById("topList")?.scrollIntoView({
                        behavior: "smooth",
                        block: "start"
                    });
                    setTeacher(null);
                    setSubject(null);
                    onAfterChange();
                },
                onError: () => { toast.error("Hiba történt a beküldés közben!"); }
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
        <>
            <Card>
                <CardHeader>
                    <CardTitle>Oktatási visszajelzés</CardTitle>
                </CardHeader>
                <CardContent className="space-y-8">
                    {/* Tantárgy és tanár kiválasztás */}
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
                </CardContent>
            </Card>

            {/* Kérdések – csak akkor jelennek meg, ha van subject és teacher */}
            {
                subject && teacher && (
                    <Card>
                        <CardHeader>
                            <CardTitle>Kérdések</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {currentEvaluation ? (
                    <section className="space-y-2">
                        {visibleQuestions.map((q, idx) => {
                            const showCategory = isNewCategory(visibleQuestions, idx);
                            return (
                                <div key={q.id} className="space-y-2">
                                    {showCategory && (
                                        <div className="pt-6">
                                            <h3 className="text-2xl font-semibold">{q.category}</h3>
                                            {q.description && (
                                                <p className="text-sm text-muted-foreground">{q.description}</p>
                                            )}
                                        </div>
                                    )}
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
                                </div>
                                    ))}
                            );
                        })}
                                </section>
                            ) : (
                                <div className="text-muted-foreground">
                                    Válassz tantárgyat és tanárt a kérdőív megjelenítéséhez.
                                </div>
                            )}

                            <div className="mt-4 sm:mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4">
                                <Button className="w-full sm:w-auto" variant="secondary" onClick={onSaveDraft} disabled={isPerformQuestionnaireUpdating}>
                                    Piszkozat mentése
                                </Button>
                                <Button className="w-full sm:w-auto" variant="default" onClick={onSubmit} disabled={isPerformQuestionnaireSubmit}>
                                    Beküldés
                                </Button>
                            </div>
                        </CardContent>
                    </Card>
                )
            }
        </>
    );
}
