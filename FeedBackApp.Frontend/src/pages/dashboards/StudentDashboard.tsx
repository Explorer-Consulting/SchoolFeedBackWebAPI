import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useReviews } from "../../hooks/useReviews";
import { useAuthStore } from '@/hooks/useAuth'
import { useEffect } from "react";
import { useStudentContextStore } from "@/hooks/useStudentContext";
import { toStudentContext } from "@/utils/toStudentContext";
import { Navigate } from "react-router-dom";
import { FeedbackFormDynamic } from "@/components/feedback/FeedbackFormDynamic";

export default function StudentDashboard() {
  const user = useAuthStore((state) => state.user);

  const {
    selectedSurveyId,
    setSelectedSurveyId,
    context,
    setContext,
  } = useStudentContextStore();


  const {
    querySurveys,
    isLoadingSurveys,
    isErrorSurveys,
    refetchSurveys,
    questionnaires,
    isLoadingQuestionnaire,
    isErrorQuestionnaire,
    refetchQuestionnaires } = useReviews(selectedSurveyId ?? undefined);

  useEffect(() => {
    if (!questionnaires) return;
    const ctx = toStudentContext(questionnaires);
    setContext(ctx);
  }, [questionnaires, setContext]);

  useEffect(() => {
    refetchSurveys();
  }, [refetchSurveys]);

  if (!user) return <Navigate to="/" replace />;
  if (user.role !== "Student") return <Navigate to="/no-access" replace />

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
              Kérünk, válaszolj néhány kérdésre a Tamási Áron Gimnázium oktatási tevékenységére vonatkozóan.
              A felmérés célja az oktatásra vonatkozó tapasztalatok felmérése, illetve ezekre alapozva a megfelelő
              stratégiák kidolgozása.
            </p>

            <p>
              Válaszaid nagyon fontosak számunkra, köszönjük, hogy kitöltöd az alábbi rövid kérdőívet!
              Kérünk, hogy figyelmesen olvasd el a kérdéseket, mielőtt válaszolsz. Fontos, hogy a
              visszajelzések objektívek legyenek, a nyelvezet tisztességes legyen, a kifejtett vélemények pedig
              indokoltak legyenek.
            </p>

            <p>
              Ez az űrlap névtelenül és elektronikusan tölthető ki. A válaszokat bizalmasan kezeljük.
            </p>

            <p>
              További esetleges kérdésekkel bátran fordulj az osztályotok szülői bizottsági képviselőjéhez.
            </p>

            <p>
              Jelen kérdőív a Hivatalos Közlöny 2024. augusztus 12-i, 795. számában megjelent, a 2024. augusztus 1-jei
              5707. számú tanügyminiszteri rendelettel jóváhagyott Tanulók Statútumának 1. számú melléklete alapján készült.
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader id="topList">
            <CardTitle>Kérdőívek listája</CardTitle>
          </CardHeader>
          <CardContent>
            {!isLoadingSurveys && !isErrorSurveys && querySurveys && (
              <ul className="space-y-2">
                {querySurveys
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
                {querySurveys.length === 0 && (
                  <li className="text-sm text-muted-foreground">Jelenleg nincs aktív kérdőív.</li>
                )}
              </ul>
            )}
          </CardContent>
        </Card>
      </section>

      <section>
        {!selectedSurveyId ? (
          <Card>
            <CardHeader>
              <CardTitle>Nincs kiválasztott kérdőív</CardTitle>
            </CardHeader>
            <CardContent className="text-muted-foreground">
              Kérjük, válassz ki egy kérdőívet !
            </CardContent>
          </Card>
        ) : context && !isLoadingQuestionnaire && !isErrorQuestionnaire && context.subjects.length > 0 ? (
          <FeedbackFormDynamic
            subjects={context.subjects}
            teachersBySubject={context.teachersBySubject}
            evaluations={context.evaluations}
            onAfterChange={() => {
              refetchQuestionnaires();
            }} />
        ) : !isLoadingQuestionnaire && (
          <Card>
            <CardHeader>
              <CardTitle>Kérdőív kitöltve</CardTitle>
            </CardHeader>
            <CardContent className="text-muted-foreground">
              Ezt a kérdőívet már kitöltötted. Köszönjük a visszajelzést!
            </CardContent>
          </Card>
        )}
      </section>
    </main>
  );
}