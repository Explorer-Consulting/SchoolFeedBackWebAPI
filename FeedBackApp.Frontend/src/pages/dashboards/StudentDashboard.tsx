import { FeedbackForm } from "@/components/feedback/FeedbackForm";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useReviews } from "../../hooks/useReviews";
import { toast } from "sonner";
import { useAuthStore } from '@/stores/useAuthStore'
import { useEffect, useState } from "react";
import { useStudentContextStore } from "@/stores/useStudentContextStore";
import { toStudentContext } from "@/utils/toStudentContext";
import { Navigate } from "react-router-dom";


export default function StudentDashboard() {
  const user = useAuthStore((state) => state.user);


  const [selectedSurveyId, setSelectedSurveyId] = useState<string>();

  const { surveys, isLoadingSurveys, isErrorSurveys, errorSurveys,
    questionnaires, isLoadingQuestionnaire, isErrorQuestionnaire, errorQuestionnaire } = useReviews(selectedSurveyId);

  useEffect(() => {
    console.log("questionnaires", questionnaires);
  }, [questionnaires]);


  const { context, setContext } = useStudentContextStore();

  useEffect(() => {
    if (!questionnaires) return;
    try {
      const ctx = toStudentContext(questionnaires);
      setContext(ctx);
    } catch (e) {
      console.error("Konvertalasi hiba: ", e);
    }
  }, [questionnaires, setContext]);

  if (!user) return <Navigate to="/" replace />;
  if (user.role !== "Student") return <Navigate to="/no-access" replace />

  //console.log(context.evaluations)
  return (
    <main className="container mx-auto px-6 py-10">
      <header className="mb-8">
        <div className="flex items-center justify-between gap-3 md:gap-6">
          <h1 className="text-2xl sm:text-3xl md:text-4xl font-bold tracking-tight text-zinc-800">
            Üdv, <span className="text-primary">{user.firstName}</span>!
          </h1>

          <img
            src="/Image.png"
            className=" block shrink-0 object-contain h-auto
            w-[120px]  sm:w-[180px]  md:w-[260px]  lg:w-[320px]  xl:w-[380px]  mr-0 md:mr-10"
          />
        </div>
      </header>

      <section className="mb-10 space-y-6">
        <Card>
          <CardContent className="space-y-3 text-muted-foreground py-6">
            <p>
              Kérünk, válaszoljatok néhány kérdésre a Tamási Áron Gimnázium oktatási tevékenységére vonatkozóan.
              A felmérés célja az oktatásra vonatkozó tapasztalatok felmérése, illetve ezekre alapozva a megfelelő
              stratégiák kidolgozása.
            </p>

            <p>
              Válaszaitok nagyon fontosak számunkra, köszönjük, hogy kitöltitek az alábbi rövid kérdőívet!
              Kérünk, hogy figyelmesen olvassátok el a kérdéseket, mielőtt válaszolnátok. Fontos, hogy a
              visszajelzések objektívek legyenek, a nyelvezet tisztességes legyen, a kifejtett vélemények pedig
              indokoltak legyenek.
            </p>

            <p>
              Ez az űrlap névtelenül és elektronikusan tölthető ki. A válaszokat bizalmasan kezeljük.
            </p>

            <p>
              További esetleges kérdésekkel bátran forduljatok az osztályotok szülői bizottsági képviselőjéhez.
            </p>

            <p>
              Jelen kérdőív a Hivatalos Közlöny 2024. augusztus 12-i, 795. számában megjelent, a 2024. augusztus 1-jei
              5707. számú tanügyminiszteri rendelettel jóváhagyott Tanulók Statútumának 1. számú melléklete alapján készült.
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Kérdőívek listája</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoadingSurveys && <p>Betöltés…</p>}
            {isErrorSurveys && (
              <p className="text-red-600">
                Hiba a kérdőívek betöltésekor: {String((errorSurveys as any)?.message || '')}
              </p>
            )}

            {!isLoadingSurveys && surveys && (
              <ul className="space-y-2">
                {surveys
                  .filter(s => {
                    if (!s.endDate) return true;
                    const end = new Date(s.endDate);
                    const today = new Date();
                    end.setHours(23, 59, 59, 999);
                    return end >= today;
                  })
                  .map(s => {
                    const selected = selectedSurveyId === s.id;
                    return (
                      <li key={s.id}>
                        <button
                          type="button"
                          onClick={() => setSelectedSurveyId(s.id)}
                          className={
                            "w-full text-left px-3 py-2 rounded-md transition " +
                            (selected
                              ? "bg-primary text-white"
                              : "bg-muted hover:bg-accent")
                          }
                        >
                          <div className="font-medium">{s.title}</div>
                          {s.endDate && (
                            <div className="text-xs opacity-80">
                              Lejárat: {new Date(s.endDate).toLocaleDateString()}
                            </div>
                          )}
                        </button>
                      </li>
                    );
                  })}
                {surveys.filter(s => {
                  if (!s.endDate) return true;
                  const end = new Date(s.endDate);
                  const today = new Date();
                  end.setHours(23, 59, 59, 999);
                  return end >= today;
                }).length === 0 && (
                    <li className="text-sm text-muted-foreground">Jelenleg nincs aktív kérdőív.</li>
                  )}
              </ul>
            )}

            {selectedSurveyId && isLoadingQuestionnaire && (
              <p className="mt-3 text-sm">Űrlap betöltése…</p>
            )}
            {selectedSurveyId && isErrorQuestionnaire && (
              <p className="mt-3 text-sm text-red-600">
                Hiba az űrlap betöltésekor: {String((errorQuestionnaire as any)?.message || "")}
              </p>
            )}
          </CardContent>
        </Card>
      </section>

      <section>
        {context && (
          <FeedbackForm
            subjects={context.subjects}
            teachersBySubject={context.teachersBySubject}
            evaluations={context.evaluations}
            onAfterChange={() => { }} />
        )}
      </section>
    </main>
  );
}
