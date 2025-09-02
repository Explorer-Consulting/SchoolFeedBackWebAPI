import { useMemo, useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { toast } from "sonner";
import { Evaluation, EvaluationResponses } from "@/models/StudentContext"
import ClassroomSection from "@/components/feedback/sections/ClassroomSection"
import OutsideEducationSection from "@/components/feedback/sections/OutsideEducationSection"
import AttendanceSection from "./sections/AttendanceSection";
import { toBackendPayload } from "@/utils/toBackendPayload";
import { useReviews } from "@/hooks/useReviews";
import { useStudentContextStore } from "@/hooks/useStudentContext";
import { initialFeedbackForm, FeedbackFormState } from "../../utils/feedback-form.state";
import { useCallback } from "react";

type FeedbackFormProps = {
  subjects: string[];
  teachersBySubject: Record<string, string[]>;
  evaluations: Evaluation[];
  onAfterChange: () => void;
}

export function FeedbackForm({
  subjects,
  teachersBySubject,
  evaluations,
  onAfterChange }: FeedbackFormProps) {

  const {
    performQuestionnaireUpdate,
    isPerformQuestionnaireUpdating,
    performQuestionnaireSubmit,
    isPerformQuestionnaireSubmit } = useReviews();

  const {
    selectedSubject: subject,
    setSelectedSubject: setSubject,
    selectedTeacher: teacher,
    setSelectedTeacher: setTeacher, } = useStudentContextStore();

  const onSubjectChange = (s: string) => {
    setSubject(s);
    setTeacher(null);
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

  const [form, setForm] = useState<FeedbackFormState>(initialFeedbackForm);

  const setField = <K extends keyof FeedbackFormState>(key: K) =>
    (val: FeedbackFormState[K] | ((prev: FeedbackFormState[K]) => FeedbackFormState[K])) =>
      setForm(prev => {
        const nextVal =
          typeof val === "function" ? (val as (p: FeedbackFormState[K]) => FeedbackFormState[K])(prev[key]) : val
        return { ...prev, [key]: nextVal }
      })

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

  const applyResponses = useCallback((r?: Partial<EvaluationResponses>) => {
    if (!r) return;

    setForm(prev => {
      const next = { ...prev };

      for (const [key, val] of Object.entries(r)) {
        if (key === "q19" || key === "q20") {
          next[key as "q19" | "q20"] = Array.isArray(val)
            ? (val as string[])
            : prev[key as "q19" | "q20"];
        } else {
          next[key as keyof FeedbackFormState] = String(val ?? "") as any;
        }
      }

      return next;
    });
  }, [setForm]);

  useEffect(() => {
    if (!subject || !teacher) return;
    applyResponses(currentEvaluation?.responses);
  }, [subject, teacher, currentEvaluation, applyResponses]);

  const id = currentEvaluation?.id;
  const likertValues = ["1", "2", "3", "4", "5"];
  const qValues = useMemo(
    () => Array.from({ length: 17 }, (_, i) => form[`q${i}` as keyof FeedbackFormState] as string),
    [form]
  );
  const likerts = qValues;
  const setQValues = useMemo(
    () => Array.from({ length: 17 }, (_, i) => setField(`q${i}` as keyof FeedbackFormState)) as Array<(v: string) => void>,
    []
  );

  const isAttendingOutside = useMemo(
    () => form.q18 === "1" || form.q18 === "2",
    [form.q18]
  );

  const collectResponses = (): FeedbackFormState => ({
    ...form,
    q19: isAttendingOutside ? form.q19 : []
  });


  const validate = () => {
    if (!subject || !teacher) {
      toast("Kérjük, válaszd ki a tantárgyat és a tanárt.");
      return;
    }
    if (likerts.some((v) => !v)) {
      toast("Kérjük, töltsd ki az osztálytermi tevékenység minden kérdését (1–17).");
      return;
    }
    if (!form.q17) {
      toast("Kérjük, válaszolj a 18. kérdésre.");
      return;
    }
    if (!form.q18) {
      toast("Kérjük, válaszolj a 19. kérdésre.");
      return;
    }
    if (isAttendingOutside && form.q19.length === 0) {
      toast("Kérjük, jelöld meg legalább egy okot a 20. kérdésnél.");
      return;
    }
    if (form.q20.length === 0) {
      toast("Kérjük, válassz legalább egy lehetőséget a 21. kérdésnél.");
      return;
    }
    if (form.q21.length < 50) {
      toast("A 22. kérdésnél a válasznak legalább 50 karakternek kell lennie.");
      return;
    }
    if (form.q22.length < 50) {
      toast("A 23. kérdésnél a válasznak legalább 50 karakternek kell lennie.");
      return;
    }
    if (!form.q23 || !form.q24 || !form.q25) {
      toast("Kérjük, töltsd ki a jelenlétre és elmaradt tanórákra vonatkozó kérdéseket (24–26).");
      return;
    }
    return null;
  };

  const onSaveDraft = () => {
    if (!id) return;
    if (!subject || !teacher) {
      toast("Kérjük, válaszd ki a tantárgyat és a tanárt.");
      return;
    }

    if (!(form.q18 === "1" || form.q18 === "2")) {
      setForm(prev => ({ ...prev, q19: [] }));
    }

    const data = collectResponses();
    const payload = toBackendPayload(data);
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
    const err = validate();

    if (err !== null) return;

    const data = collectResponses();
    const payload = toBackendPayload(data);
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

  const toggleMulti = (value: string, setFn: (updater: (prev: string[]) => string[]) => void) => {
    setFn((prev) => (prev.includes(value) ? prev.filter((v) => v !== value) : [...prev, value]));
  };

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

        <ClassroomSection
          qValues={qValues}
          setQValues={setQValues}
          likertValues={likertValues}
        />

        <OutsideEducationSection
          q17={form.q17}
          setQ17={setField("q17")}
          q18={form.q18}
          setQ18={setField("q18")}
          q19={form.q19}
          setQ19={setField("q19")}
          q20={form.q20}
          setQ20={setField("q20")}
          q21={form.q21}
          setQ21={setField("q21")}
          q22={form.q22}
          setQ22={setField("q22")}
          isAttendingOutside={isAttendingOutside}
          toggleMulti={toggleMulti}
        />

        <AttendanceSection
          q23={form.q23} setQ23={setField("q23")}
          q24={form.q24} setQ24={setField("q24")}
          q25={form.q25} setQ25={setField("q25")}
        />

        <div className="mt-4 sm:mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4">
          <Button className="w-full sm:w-auto" variant="secondary" onClick={onSaveDraft} disabled={isPerformQuestionnaireUpdating}>Piszkozat mentése</Button>
          <Button className="w-full sm:w-auto" variant="default" onClick={onSubmit} disabled={isPerformQuestionnaireSubmit}>Beküldés</Button>
        </div>
      </CardContent>
    </Card>
  );
}
