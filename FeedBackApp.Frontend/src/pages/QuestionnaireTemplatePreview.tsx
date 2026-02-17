import { useEffect, useMemo, useState, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import DynamicQuestion from "@/components/feedback/DynamicQuestion";

import type { Question, QuestionID, QuestionType } from "@/models/StudentContext";
import { isNewCategory, isMulti } from "@/utils/feedBackFormHelper";

import { GetQuestionnaireTemplatePreview, SelfOptIn } from "@/api/ReviewApi";  
import { toast } from "sonner"; 
import { useAuthStore } from "@/hooks/useAuth";  

type BackendQuestionTemplate = {
  Id?: string;
  id?: string;

  Question?: string;
  question?: string;

  Type?: number | string;
  type?: number | string;

  AnswerOptions?: string[];
  answerOptions?: string[];

  Category?: string;
  category?: string;

  Description?: string;
  description?: string;
};

type PreviewResponse = {
  title?: string;
  Title?: string;

  questionTemplates?: BackendQuestionTemplate[];
  QuestionTemplates?: BackendQuestionTemplate[];

  selfEnrollmentAllowed?: boolean;
  SelfEnrollmentAllowed?: boolean;
};

function mapType(type: number | string | undefined): QuestionType {
  if (type == null) return "OpenEnded";

  if (typeof type === "number") {
    switch (type) {
      case 1: return "LikertScaleOneToFive";
      case 2: return "MultinomialSingleChoice";
      case 3: return "MultinomialSingleChoiceOther";
      case 4: return "MultipleChoice";
      case 5: return "OpenEnded";
      default: return "OpenEnded";
    }
  }

  const t = String(type);
  if (
    t === "LikertScaleOneToFive" ||
    t === "MultinomialSingleChoice" ||
    t === "MultinomialSingleChoiceOther" ||
    t === "MultipleChoice" ||
    t === "OpenEnded"
  ) {
    return t as QuestionType;
  }

  const n = Number(t);
  return Number.isNaN(n) ? "OpenEnded" : mapType(n);
}

function ReadonlyBlock({ children }: { children: React.ReactNode }) {
  return (
    <div
      aria-disabled
      style={{ pointerEvents: "none", userSelect: "none" }}
    >
      {children}
    </div>
  );
}

export default function QuestionnaireTemplatePreview() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [data, setData] = useState<PreviewResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isEnrolling, setIsEnrolling] = useState(false);

  const user = useAuthStore((s) => s.user);
  const hasAttemptedAutoEnroll = useRef(false);

   // automatic self opt-in ussefect
  useEffect(() => {

    if (user && data && id && (data.selfEnrollmentAllowed || data.SelfEnrollmentAllowed) && !hasAttemptedAutoEnroll.current) {
      const urlParams = new URLSearchParams(window.location.search);
      const optInToken = urlParams.get('optin');
      
      if (optInToken) {
        hasAttemptedAutoEnroll.current = true;  // mark trying to subscribe
        
        // call self opt in link
        (async () => {
          setIsEnrolling(true);
          try {
            await SelfOptIn(id, optInToken);
            toast.success("Sikeresen feliratkoztál a kérdőívre!");
            
            // waiting
            setTimeout(() => {
              navigate("/dashboard/student");
            }, 1500);
            
          } catch (error: any) {
            console.error("Auto self opt-in error:", error);
            
            if (error.response?.status === 401) {
              toast.error("Hitelesítési hiba történt.");
            } else if (error.response?.status === 403) {
              toast.error("A feliratkozás már nem engedélyezett.");
            } else if (error.response?.status === 200) {
              toast.info("Már feliratkoztál erre a kérdőívre!");
              setTimeout(() => {
                navigate("/dashboard/student");
              }, 1500);
            } else {
              toast.error("Feliratkozás sikertelen: " + (error.response?.data || error.message));
            }
          } finally {
            setIsEnrolling(false);
          }
        })();
      }
    }
  }, [user, data, id, navigate]);

  useEffect(() => {
    if (!id) return;

    (async () => {
      try {
        setLoading(true);
        const res = await GetQuestionnaireTemplatePreview(id);
        setData(res);
      } catch (e: any) {
        setError(e?.message ?? "Hiba történt a betöltéskor.");
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  const handleSelfOptIn = async () => {
    if (!id) return;
    
    // Check the user is logged in
    if (!user) {
      toast.error("Be kell jelentkezned a feliratkozáshoz!");
      navigate("/?returnTo=" + encodeURIComponent(window.location.pathname + window.location.search));
      return;
    }
    
    setIsEnrolling(true);
    
    try {
      // get token
      const urlParams = new URLSearchParams(window.location.search);
      const optInToken = urlParams.get('optin');
      
      if (!optInToken) {
        toast.error("Hiányzó opt-in token!");
        return;
      }
      
      // call api endpoint
      await SelfOptIn(id, optInToken);
      toast.success("Sikeresen feliratkoztál a kérdőívre!");
      
      // navigate to the dashboard
      navigate("/dashboard/student");
      
    } catch (error: any) {
      console.error("Self opt-in error:", error);
      
      if (error.response?.status === 401) {
        toast.error("Be kell jelentkezned a feliratkozáshoz!");
        navigate("/?returnTo=" + encodeURIComponent(window.location.pathname + window.location.search));
      } else if (error.response?.status === 403) {
        toast.error("A feliratkozás már nem engedélyezett.");
      } else {
        toast.error("Feliratkozás sikertelen: " + (error.response?.data || error.message));
      }
    } finally {
      setIsEnrolling(false);
    }
  };


  const title = data?.title ?? data?.Title ?? "Kérdőív sablon – előnézet";
  const raw = data?.questionTemplates ?? data?.QuestionTemplates ?? [];
  const selfEnrollmentAllowed =
    data?.selfEnrollmentAllowed ?? data?.SelfEnrollmentAllowed ?? false;

  const questions: Question[] = useMemo(
    () =>
      raw.map((q, i) => ({
        id: (q.Id ?? q.id ?? `q${i}`) as QuestionID,
        text: q.Question ?? q.question ?? "",
        type: mapType(q.Type ?? q.type),
        options: q.AnswerOptions ?? q.answerOptions ?? [],
        category: q.Category ?? q.category ?? "",
        description: (q.Description ?? q.description)?.trim() || undefined,
      })),
    [raw]
  );

  if (loading) return <div className="p-6">Betöltés…</div>;
  if (error) return <div className="p-6 text-red-600">{error}</div>;

  /* ide irányítunk, ha nincs self enrollment */
  const REDIRECT_PATH = "/no-access";

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <Card>
        <CardHeader>
          <CardTitle>{title}</CardTitle>
        </CardHeader>

        <CardContent className="space-y-6 opacity-40">
          {questions.map((q, idx) => {
            const showCategory = isNewCategory(questions, idx);

            return (
              <div key={q.id} className="space-y-2">
                {showCategory && (
                  <h3 className="text-xl font-semibold">{q.category}</h3>
                )}

                <ReadonlyBlock>
                  <DynamicQuestion
                    q={q}
                    index={idx + 1}
                    value={isMulti(q) ? [] : ""}
                    onChange={() => {}}
                    description={q.description}
                  />
                </ReadonlyBlock>
              </div>
            );
          })}
        </CardContent>
      </Card>

    <div className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-[0.25px] bg-black/30">
        <div className="bg-background rounded-xl border shadow-xl p-6 w-full max-w-md text-center space-y-4">
          <h2 className="text-lg font-semibold">Ez csak előnézet</h2>

          <p className="text-sm text-muted-foreground">
            {selfEnrollmentAllowed
              ? "A sablon elérhető, fel tudsz iratkozni."
              : "Ehhez a sablonhoz nem engedélyezett az önfeliratkozás."}
          </p>

          {selfEnrollmentAllowed ? (
            <Button
              className="
                w-full h-12 text-base font-semibold
                bg-gradient-to-r from-emerald-500 to-green-600
                hover:from-emerald-600 hover:to-green-700
                text-white
                shadow-lg shadow-emerald-500/30
              "
              onClick={handleSelfOptIn}
              disabled={isEnrolling}
            >
              {isEnrolling ? "Feliratkozás..." : "Feliratkozok"}
            </Button>
          ) : (
            <Button
              className="w-full h-11"
              onClick={() => navigate(REDIRECT_PATH)}
            >
              Tovább az információkhoz →
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
