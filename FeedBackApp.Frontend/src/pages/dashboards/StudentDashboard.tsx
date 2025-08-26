import { FeedbackForm } from "@/components/feedback/FeedbackForm";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useReviews } from "../../hooks/useReviews";
import { toast } from "sonner";
import { useAuthStore } from '@/stores/useAuthStore'
import { useCallback, useEffect, useState } from "react";
import { useStudentContextStore } from "@/stores/useStudentContextStore";
import { toStudentContext } from "@/utils/toStudentContext";


export default function StudentDashboard() {
  const user = useAuthStore((state) => state.user);
  const { context, setContext } = useStudentContextStore();

  const refreshContext = useCallback(async () => {
    try {
      const res = await fetch("/mock/form.json", { cache: "no-store" });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const raw = await res.json();
      console.log(raw);
      const ctx = toStudentContext(raw);
      console.log(ctx);
      setContext(ctx);
    } catch (e: any) {
      console.log(e?.message ?? "Ismeretlen hiba");
    }
  }, [setContext]);

  useEffect(() => {
    if(!context) refreshContext();
  },[context,refreshContext]);

   if (!context) {
    return (
      <main className="container mx-auto px-6 py-10">
        <h1 className="text-2xl">Betöltés…</h1>
      </main>
    );
  }

/*const {
  form,
  isLoadingForm,
  isErrorForm,
  errorForm
} = useReviews(user.email);

useEffect(() => {
  if(form) {
    setContext(form);
  }
},[form,setContext]);

if (isLoadingForm && !context) {
  return (
    <main className="container mx-auto px-6 py-10">
      <h1 className="text-2xl">Loading student context…</h1>
    </main>
  )
}

if ((isErrorForm || !context) && !isLoadingForm) {
  return (
    <main className="container mx-auto px-6 py-10">
      <h1 className="text-2xl">Hiba történt a betöltéskor.</h1>
      {String((errorForm as any)?.message || '')}
    </main>
  )
}*/

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

    <section className="mb-10">
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

    </section>
    <section>
      <FeedbackForm
        subjects={context.subjects}
        teachersBySubject={context.teachersBySubject}
        evaluations={context.evaluations} 
        onAfterChange={refreshContext}/>
    </section>
  </main>
);
}
