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


type FeedbackFormProps = {
  subjects: string[];
  teachersBySubject: Record<string, string[]>;
  evaluations: Evaluation[];
  onAfterChange?: () => void;
}

export function FeedbackForm({ subjects, teachersBySubject, evaluations, onAfterChange }: FeedbackFormProps) {

  const [subject, setSubject] = useState<string>("");
  const [teacher, setTeacher] = useState<string>("");

  const [q1, setQ1] = useState("");
  const [q2, setQ2] = useState("");
  const [q3, setQ3] = useState("");
  const [q4, setQ4] = useState("");
  const [q5, setQ5] = useState("");
  const [q6, setQ6] = useState("");
  const [q7, setQ7] = useState("");
  const [q8, setQ8] = useState("");
  const [q9, setQ9] = useState("");
  const [q10, setQ10] = useState("");
  const [q11, setQ11] = useState("");
  const [q12, setQ12] = useState("");
  const [q13, setQ13] = useState("");
  const [q14, setQ14] = useState("");
  const [q15, setQ15] = useState("");
  const [q16, setQ16] = useState("");
  const [q17, setQ17] = useState("");
  const [q18, setQ18] = useState("");
  const [q19, setQ19] = useState("");
  const [q20, setQ20] = useState<string[]>([]);
  const [q21, setQ21] = useState<string[]>([]);
  const [q22, setQ22] = useState("");
  const [q23, setQ23] = useState("");
  const [q24, setQ24] = useState("");
  const [q25, setQ25] = useState("");
  const [q26, setQ26] = useState("");

  const teachersForSubject = useMemo(
    () => (subject ? (teachersBySubject[subject] ?? []) : []),
    [subject, teachersBySubject]
  );

  useEffect(() => {
    if (teacher && !teachersForSubject.includes(teacher)) {
      setTeacher("");
    }
  }, [teachersForSubject, teacher]);

  const currentEvaluation = useMemo(
    () =>
      evaluations?.find(
        (e) => e.subject === subject && e.teacher === teacher
      ),
    [evaluations, subject, teacher]
  );

  const applyResponses = (r?: Partial<EvaluationResponses>) => {
    setQ1(String(r?.q1 ?? "")); setQ2(String(r?.q2 ?? "")); setQ3(String(r?.q3 ?? ""));
    setQ4(String(r?.q4 ?? "")); setQ5(String(r?.q5 ?? "")); setQ6(String(r?.q6 ?? ""));
    setQ7(String(r?.q7 ?? "")); setQ8(String(r?.q8 ?? "")); setQ9(String(r?.q9 ?? ""));
    setQ10(String(r?.q10 ?? "")); setQ11(String(r?.q11 ?? "")); setQ12(String(r?.q12 ?? ""));
    setQ13(String(r?.q13 ?? "")); setQ14(String(r?.q14 ?? "")); setQ15(String(r?.q15 ?? ""));
    setQ16(String(r?.q16 ?? "")); setQ17(String(r?.q17 ?? ""));
    setQ18(String(r?.q18 ?? "")); setQ19(String(r?.q19 ?? ""));
    setQ20(Array.isArray(r?.q20) ? (r?.q20 as string[]) : []);
    setQ21(Array.isArray(r?.q21) ? (r?.q21 as string[]) : []);
    setQ22(String(r?.q22 ?? "")); setQ23(String(r?.q23 ?? ""));
    setQ24(String(r?.q24 ?? "")); setQ25(String(r?.q25 ?? "")); setQ26(String(r?.q26 ?? ""));
  };

  useEffect(() => {
    if (!subject || !teacher) return;
    applyResponses(currentEvaluation?.responses);
  }, [subject, teacher, currentEvaluation]);

  const id = currentEvaluation?.id;
  const likertValues = ["1", "2", "3", "4", "5"];

  const qValues = useMemo(
    () => [q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q12, q13, q14, q15, q16, q17],
    [q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q12, q13, q14, q15, q16, q17]
  );
  const setQValues = useMemo(
    () => [setQ1, setQ2, setQ3, setQ4, setQ5, setQ6, setQ7, setQ8, setQ9, setQ10, setQ11, setQ12, setQ13, setQ14, setQ15, setQ16, setQ17],
    []
  );

  const isAttendingOutside = useMemo(
    () => q19 === "1" || q19 === "2",
    [q19]
  );

  const collectResponses = (): EvaluationResponses => {
    const normalizedQ20 = (q19 === "1" || q19 === "2") ? q20 : [];
    return {
      q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q12, q13, q14, q15, q16, q17,
      q18, q19, q20: normalizedQ20, q21, q22, q23, q24, q25, q26
    };
  };

  const validate = () => {
    if (!subject || !teacher) {
      toast("Kérjük, válaszd ki a tantárgyat és a tanárt.");
      return;
    }

    const likerts = [
      q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q12, q13, q14, q15, q16, q17,
    ];
    if (likerts.some((v) => !v)) {
      toast("Kérjük, töltsd ki az osztálytermi tevékenység minden kérdését (1–17).");
      return;
    }

    if (!q18) {
      toast("Kérjük, válaszolj a 18. kérdésre.");
      return;
    }

    if (!q19) {
      toast("Kérjük, válaszolj a 19. kérdésre.");
      return;
    }
    if (isAttendingOutside && q20.length === 0) {
      toast("Kérjük, jelöld meg legalább egy okot a 20. kérdésnél.");
      return;
    }

    if (q21.length === 0) {
      toast("Kérjük, válassz legalább egy lehetőséget a 21. kérdésnél.");
      return;
    }

    if (q22.length < 50) {
      toast("A 22. kérdésnél a válasznak legalább 50 karakternek kell lennie.");
      return;
    }

    if (q23.length < 50) {
      toast("A 23. kérdésnél a válasznak legalább 50 karakternek kell lennie.");
      return;
    }

    if (!q24 || !q25 || !q26) {
      toast("Kérjük, töltsd ki a jelenlétre és elmaradt tanórákra vonatkozó kérdéseket (24–26).");
      return;
    }
    return null;
  };

  const onSaveDraft = () => {
    if (!subject || !teacher) {
      toast("Kérjük, válaszd ki a tantárgyat és a tanárt.");
      return;
    }

    if (!(q19 === "1" || q19 === "2")) {
      setQ20([]);
    }

    const data = collectResponses();
    const payload = toBackendPayload(id, data, "Unsubmitted");
    console.log("Draft saved:", JSON.stringify(payload, null, 2));
    onAfterChange?.();
  };

  const onSubmit = () => {
    const err = validate();
    if (err !== null) return;
    toast("Küldésre kész. Supabase engedélyezésével anonim módon tudjuk tárolni.");
    const data = collectResponses();
    const payload = toBackendPayload(id, data, "Submitted");
    console.log("submit saved:", JSON.stringify(payload, null, 2));
    onAfterChange?.();
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
            <Select value={subject} onValueChange={setSubject}>
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
            <Select value={teacher} onValueChange={setTeacher}>
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
          q18={q18} setQ18={setQ18}
          q19={q19} setQ19={setQ19}
          q20={q20} setQ20={setQ20}
          q21={q21} setQ21={setQ21}
          q22={q22} setQ22={setQ22}
          q23={q23} setQ23={setQ23}
          isAttendingOutside={isAttendingOutside}
          toggleMulti={toggleMulti}
        />

        <AttendanceSection
          q24={q24} setQ24={setQ24}
          q25={q25} setQ25={setQ25}
          q26={q26} setQ26={setQ26}
        />

        <div className="flex gap-3">
          <Button variant="secondary" onClick={onSaveDraft}>Piszkozat mentése</Button>
          <Button variant="default" onClick={onSubmit}>Beküldés</Button>
        </div>
      </CardContent>
    </Card>
  );
}
