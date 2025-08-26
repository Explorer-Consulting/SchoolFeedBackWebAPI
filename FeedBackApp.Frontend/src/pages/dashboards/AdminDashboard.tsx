import { useState } from "react";
import { CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useReviews } from "@/hooks/useReviews";

export default function AdminDashboard() {
  const [startDate, setStartDate] = useState<Date | undefined>();
  const [endDate, setEndDate] = useState<Date | undefined>();
  const [selectedQuestionnaireId, setSelectedQuestionnaireId] = useState<string | undefined>();
  const [title, setTitle] = useState<string>("");

  const {
    createQuestionnaires,
    isCreatingQuestionnaire,
    questionnairesSummary,
    isLoadingQuestionnairesSummary,
    deleteQuestionnaire,
    isDeletingQuestionnaire,
    startQuestionnaire,
    isStartingQuestionnaire,

    exportTeacherEvaluations,
    isExportingTeacher,
    exportGlobalSummary,
    isExportingSummary
  } = useReviews();

  const mockQuestionnaires = [
    { id: "q1", title: "Math Evaluation Spring 2025" },
    { id: "q2", title: "Physics Midterm Survey" },
    { id: "q3", title: "Chemistry Final Feedback" }
  ];

  const [localQuestionnaires, setLocalQuestionnaires] = useState(mockQuestionnaires);

  const displayedQuestionnaires = questionnairesSummary || localQuestionnaires;

  const [file, setFile] = useState<File | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
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
    /*const payload = {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      title,
      file
    };*/

    const payload = {
    startDate: startDate.toISOString().split("T")[0],
    endDate: endDate.toISOString().split("T")[0],
    title,
    studentSets: [
      { setId: "XI. A", studentEmails: ["a1@example.com","a2@example.com"] },
      { setId: "XI. B", studentEmails: ["b1@example.com","b2@example.com"] }
    ],
    questionnaireTemplate: [
      { question: "How satisfied are you with the course?", type: "LikertScaleOneToFive" },
      { question: "Do you like the course?", type: "MultinomialSingleChoice", answerOptions: ["igen", "nem"] },
      { question: "Any comments?", type: "OpenEnded" }
    ],
    teachers: [
      { email: "kovacs.maria@gimi.ro", name: "Kovács Mária" }
    ],
    questionnaireCreationParams: [
      { teacherEmail: "kovacs.maria@gimi.ro", subjectName: "Matematika", studentSetIds: ["XI. A", "XI. B"] }
    ]
  };

    console.log("Payload to send:", payload);
    createQuestionnaires(payload, {
      onSuccess: () => {
        toast.success("Questionnaires created!");
        setLocalQuestionnaires(prev => [...prev, { id:Date.now().toString(), title }]);
      },
      onError: () => toast.error("Failed to create questionnaires."),
    });
  };

  const handleStartQuestionnaire = () => {
    if (!selectedQuestionnaireId) {
      toast.error("Select a questionnaire first!");
      return;
    }
    console.log("start: ", selectedQuestionnaireId);
    startQuestionnaire(selectedQuestionnaireId, {
      onSuccess: () => toast.success("Questionnaire started!"),
      onError: () => toast.error("Failed to start questionnaire.")
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
      onSuccess: () => toast.success("Teacher evaluations exported!"),
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
      onSuccess: () => toast.success("Global summary exported!"),
      onError: () => toast.error("Failed to export global summary.")
    });
  };

  return (
    <main className="container mx-auto px-6 py-10">
      <header className="mb-8">
        <h1 className="text-3xl font-bold">Admin Dashboard</h1>
        <p className="text-muted-foreground">Manage feedback windows, access, and exports.</p>
      </header>

      <CardContent>
        <label>Start Date:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={startDate ? startDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setStartDate(new Date(e.target.value))}
        />

        <label>End Date:</label>
        <input
          type="date"
          className="border rounded p-2 w-full mb-4"
          value={endDate ? endDate.toISOString().split("T")[0] : ""}
          onChange={(e) => setEndDate(new Date(e.target.value))}
        />

        <label>Title:</label>
        <input
          type="text"
          className="border rounded p-2 w-full mb-4"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Enter questionnaire title"
        />

        <label>Upload Excel:</label>
        <input
          type="file"
          accept=".xlsx, .xls"
          className="border rounded p-2 w-full mb-4"
          onChange={handleFileChange}
        />
      </CardContent>
      <div className="mt-6 flex flex-row gap-4">
        <Button onClick={sendQuestionnaires} disabled={isCreatingQuestionnaire || !endDate || !startDate || !file || !title}>Create Questionnaires</Button>
      </div>
      <br />
      <CardContent>
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

      <div className="mt-6 flex flex-row gap-4">
        <Button onClick={handleStartQuestionnaire} disabled={!selectedQuestionnaireId || isStartingQuestionnaire}>Start Questionnaire</Button>
        <Button onClick={handleExportTeacher} disabled={!selectedQuestionnaireId || isExportingTeacher}>Export Teacher Evaluations</Button>
        <Button onClick={handleExportSummary} disabled={!selectedQuestionnaireId || isExportingSummary}>Export Global Summary</Button>
        <Button onClick={deleteSelectedQuestionnaire} disabled={!selectedQuestionnaireId || isDeletingQuestionnaire}>Delete Selected Questionnaire</Button>
      </div>
    </main>
  );
}
