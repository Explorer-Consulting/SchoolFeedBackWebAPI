import { useEffect, useState } from "react";
import { CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useReviews } from "@/hooks/useReviews";
import { parseExcel } from "@/utils/parseExcel";
import { useAuthStore } from "@/hooks/useAuth";
import { Navigate } from "react-router-dom";

export default function AdminDashboard() {
  const [startDate, setStartDate] = useState<Date | undefined>();
  const user = useAuthStore((s) => s.user);
  const [endDate, setEndDate] = useState<Date | undefined>();
  const [selectedQuestionnaireId, setSelectedQuestionnaireId] = useState<string | undefined>();
  const [title, setTitle] = useState<string>("");

  const {
    createQuestionnaires,
    isCreatingQuestionnaire,
    deleteQuestionnaire,
    isDeletingQuestionnaire,

    exportTeacherEvaluations,
    isExportingTeacher,
    exportGlobalSummary,
    isExportingSummary,

    adminSurveys,
    isLoadingAdminSurveys,
    isErrorAdminSurveys,
    refetchAdminSurveys,
  } = useReviews();

  const displayedQuestionnaires = adminSurveys;
  const [file, setFile] = useState<File | null>(null);

  if (!user) return <Navigate to="/" replace />;
  if (user.role !== "Admin") {
    return <Navigate to="/no-access" replace />
  } else {
    refetchAdminSurveys();
  }


  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!/\.(xlsx|xls|xlsm)$/i.test(file.name)) {
      toast.warning("Kérlek, tölts fel egy érvényes Excel fájlt (.xlsx vagy .xls).");
      return;
    }
    setFile(file);
  };

  const sendQuestionnaires = async () => {
    if (!startDate || !endDate) {
      toast.warning("Kérlek, állítsd be mind a kezdő-, mind a záró dátumot.");
      return;
    }

    if (startDate >= endDate) {
      toast.warning("A kezdő dátumnak korábbinak kell lennie, mint a záró dátum.");
      return;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (startDate < today) {
      toast.warning("A kezdő dátum nem lehet korábbi, mint a mai nap.");
      return;
    }

    if (!title) {
      toast.warning("Kérlek, add meg a címet.");
      return;
    }

    if (!file) {
      toast.warning("Kérlek, tölts fel egy Excel-fájlt.");
      return;
    }

    let payload;
    try {
      payload = await parseExcel(file, startDate.toISOString().split("T")[0], endDate.toISOString().split("T")[0], title);
    } catch (err) {
      console.error("Failed to parse Excel:", err);
    }

    createQuestionnaires(payload, {
      onSuccess: () => {
        setStartDate(undefined);
        setEndDate(undefined);
        setTitle("");
        toast.success("A kérdőívek létrehozva!");
        refetchAdminSurveys();
      },
      onError: () => toast.error("A kérdőívek létrehozása sikertelen."),
    });
  };

  const deleteSelectedQuestionnaire = () => {
    if (!selectedQuestionnaireId) {
      toast.warning("Először válassz ki egy kérdőívet!");
      return;
    }
    deleteQuestionnaire(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("A kérdőív törölve!");
        refetchAdminSurveys();
        setSelectedQuestionnaireId("");
      },
      onError: () => {
        toast.error("A kérdőív törlése sikertelen.");
      }
    });
  };

  const handleExportTeacher = () => {
    if (!selectedQuestionnaireId) {
      toast.warning("Először válassz ki egy kérdőívet!");
      return;
    }
    exportTeacherEvaluations(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("A tanári értékelések exportálva!");
        setSelectedQuestionnaireId("");
      },
      onError: () => toast.error("A tanári értékelések exportálása sikertelen.")
    });
  };

  const handleExportSummary = () => {
    if (!selectedQuestionnaireId) {
      toast.warning("Először válassz ki egy kérdőívet!");
      return;
    }
    exportGlobalSummary(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("Az összesített összefoglaló exportálva!");
        setSelectedQuestionnaireId("");
      },
      onError: () => toast.error("Az összesített összefoglaló exportálása sikertelen.")
    });
  };

  return (
    <main className="container mx-auto px-4 sm:px-6 py-6 sm:py-10">
      <header className="mb-6 sm:mb-8 text-center sm:text-left">
        <h1 className="text-2xl sm:text-3xl font-bold">Adminisztrációs irányítófelület</h1>
        <p className="text-sm sm:text-base text-muted-foreground">
          Kezeld a visszajelzési időablakokat, a hozzáférést és az exportálást.
        </p>
      </header>

      <CardContent>
        <label className="block mb-1">Kezdő dátum:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={startDate ? startDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setStartDate(new Date(e.target.value))}
        />

        <label className="block mb-1">Záró dátum:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={endDate ? endDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setEndDate(new Date(e.target.value))}
        />

        <label className="block mb-1">Kérdőív címe:</label>
        <input
          type="text"
          className="border rounded p-2 w-full mb-4"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Enter questionnaire title"
        />

        <label className="block mb-1">Excel feltöltése:</label>
        <input
          type="file"
          accept=".xlsx, .xls, .xlsm"
          className="border rounded p-2 w-full mb-4"
          onChange={handleFileChange}
        />
      </CardContent>

      <div className="mt-4 sm:mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4">
        <Button
          className="w-full sm:w-auto"
          onClick={sendQuestionnaires}
          disabled={isCreatingQuestionnaire || !endDate || !startDate || !file || !title}
        >
          Kérdőívek létrehozása
        </Button>
      </div>

      <br />

      <CardContent>
        {isLoadingAdminSurveys && <p>Kérdőívek betöltése…</p>}
        {isErrorAdminSurveys && <p>Hiba a kérdőívek betöltése közben.</p>}
        <select
          className="border rounded p-2 w-full"
          value={selectedQuestionnaireId}
          onChange={(e) => setSelectedQuestionnaireId(e.target.value)}
        >
          <option value="">-- Válassz egy kérdőívet --</option>
          {displayedQuestionnaires?.map((q: any) => (
            <option key={q.id} value={q.id}>
              {q.title || q.id}
            </option>
          ))}
        </select>
      </CardContent>

      <div className="mt-4 sm:mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4">
        <Button
          className="w-full sm:w-auto"
          onClick={handleExportTeacher}
          disabled={!selectedQuestionnaireId || isExportingTeacher || isLoadingAdminSurveys}
        >
          Tanári értékelések exportálása
        </Button>

        <Button
          className="w-full sm:w-auto"
          onClick={handleExportSummary}
          disabled={!selectedQuestionnaireId || isExportingSummary || isLoadingAdminSurveys}
        >
          Összesített összefoglaló exportálása
        </Button>

        <Button
          className="w-full sm:w-auto"
          onClick={deleteSelectedQuestionnaire}
          disabled={!selectedQuestionnaireId || isDeletingQuestionnaire || isLoadingAdminSurveys}
        >
          Kijelölt kérdőív törlése
        </Button>
      </div>
    </main>
  );
}