import { useEffect, useState } from "react";
import { CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useReviews } from "@/hooks/useReviews";
import { parseExcel } from "@/hooks/useExcel";
import { useAuthStore } from "@/stores/useAuthStore";
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
  if (user.role !== "Admin") { return <Navigate to="/no-access" replace /> }
  else { refetchAdminSurveys(); }


  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!/\.(xlsx|xls)$/i.test(file.name)) {
      toast.error("Please upload a valid Excel file (.xlsx or .xls).");
      return;
    }
    setFile(file);
  };

  const sendQuestionnaires = async () => {
    if (!startDate || !endDate) {
      toast.error("Please set both start and end date.");
      return;
    }

    if (startDate >= endDate) {
      toast.error("Start date must be sooner than end date.");
      return;
    }

    if (!title) {
      toast.error("Please enter a title.");
      return;
    }

    if (!file) {
      toast.error("Please upload an Excel file.");
      return;
    }

    let payload;
    try {
      payload = await parseExcel(file, startDate.toISOString().split("T")[0], endDate.toISOString().split("T")[0], title);
      console.log("Final payload:", payload);
    } catch (err) {
      console.error("Failed to parse Excel:", err);
    }

    console.log("Payload to send:", payload);
    createQuestionnaires(payload, {
      onSuccess: () => {
        toast.success("Questionnaires created!");
        refetchAdminSurveys();
      },
      onError: () => toast.error("Failed to create questionnaires."),
    });
  };

  const deleteSelectedQuestionnaire = () => {
    if (!selectedQuestionnaireId) {
      toast.error("Select a questionnaire first!");
      return;
    }
    console.log("delete: ", selectedQuestionnaireId);
    deleteQuestionnaire(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("Deleted questionnaire!");
        refetchAdminSurveys();
        setSelectedQuestionnaireId("");
      },
      onError: () => {
        toast.error("Failed to delete questionnaire.");
      }
    });
  };

  const handleExportTeacher = () => {
    if (!selectedQuestionnaireId) {
      toast.error("Select a questionnaire first!");
      return;
    }
    console.log("export teacher: ", selectedQuestionnaireId);
    exportTeacherEvaluations(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("Teacher evaluations exported!");
        setSelectedQuestionnaireId("");
      },
      onError: () => toast.error("Failed to export teacher evaluations.")
    });
  };

  const handleExportSummary = () => {
    if (!selectedQuestionnaireId) {
      toast.error("Select a questionnaire first!");
      return;
    }
    console.log("export summary: ", selectedQuestionnaireId);
    exportGlobalSummary(selectedQuestionnaireId, {
      onSuccess: () => {
        toast.success("Global summary exported!");
        setSelectedQuestionnaireId("");
      },
      onError: () => toast.error("Failed to export global summary.")
    });
  };

  return (
    <main className="container mx-auto px-4 sm:px-6 py-6 sm:py-10">
      <header className="mb-6 sm:mb-8 text-center sm:text-left">
        <h1 className="text-2xl sm:text-3xl font-bold">Admin Dashboard</h1>
        <p className="text-sm sm:text-base text-muted-foreground">
          Manage feedback windows, access, and exports.
        </p>
      </header>

      <CardContent>
        <label className="block mb-1">Start Date:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={startDate ? startDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setStartDate(new Date(e.target.value))}
        />

        <label className="block mb-1">End Date:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={endDate ? endDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setEndDate(new Date(e.target.value))}
        />

        <label className="block mb-1">Title:</label>
        <input
          type="text"
          className="border rounded p-2 w-full mb-4"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Enter questionnaire title"
        />

        <label className="block mb-1">Upload Excel:</label>
        <input
          type="file"
          accept=".xlsx, .xls"
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
          Create Questionnaires
        </Button>
      </div>

      <br />

      <CardContent>
        {isLoadingAdminSurveys && <p>Loading surveys...</p>}
        {isErrorAdminSurveys && <p>Error loading surveys.</p>}
        <select
          className="border rounded p-2 w-full"
          value={selectedQuestionnaireId}
          onChange={(e) => setSelectedQuestionnaireId(e.target.value)}
        >
          <option value="">-- Select a questionnaire --</option>
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
          Export Teacher Evaluations
        </Button>

        <Button
          className="w-full sm:w-auto"
          onClick={handleExportSummary}
          disabled={!selectedQuestionnaireId || isExportingSummary || isLoadingAdminSurveys}
        >
          Export Global Summary
        </Button>

        <Button
          className="w-full sm:w-auto"
          onClick={deleteSelectedQuestionnaire}
          disabled={!selectedQuestionnaireId || isDeletingQuestionnaire || isLoadingAdminSurveys}
        >
          Delete Selected Questionnaire
        </Button>
      </div>
    </main>
  );
}