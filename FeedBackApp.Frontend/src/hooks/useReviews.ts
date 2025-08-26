import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query"
import { CreateQuestionnaires, GetQuestionnaireSummary,ExportQuestionnaire, GetEvaluation, PerformQuestionnaireUpdate, DeleteQuestionnaire, LoginWithGoogle,GetQuestionnaires,PerformGetSurveys } from "@/api/ReviewApi"
import { useParams } from "react-router-dom";
import { StudentContext } from "@/models/StudentContext"
import { BackendPayload } from "@/utils/toBackendPayload";
import {SurveySummary} from "@/models/Survey"
import { useAuthStore } from "@/stores/useAuthStore";

export const useReviews = () => {
    const client = useQueryClient();
    const { questionnaireId, evaluationId } = useParams();
    const user=useAuthStore((s)=>s.user);

    const { mutate: createQuestionnaires, isPending: isCreatingQuestionnaire } = useMutation({
        mutationFn: (payload: { startDate: string; endDate: string }) => CreateQuestionnaires(payload),
        onSuccess: () => {
            client.invalidateQueries({
                queryKey: ['questionnaires']
            });
        }
    })

    const {
        data: questionnairesSummary,
        isLoading: isLoadingQuestionnairesSummary,
        isError: isErrorQuestionnairesSummary,
        error: errorQuestionnairesSummary
    } = useQuery({
        queryKey: [`questionnairesSummary`, questionnaireId],
        queryFn: () => GetQuestionnaireSummary(questionnaireId),
    })

    const {
        data: evaluation,
        isLoading: isLoadingEvaluation,
        isError: isErrorEvaluation,
        error: errorEvaluation
    } = useQuery({
        queryKey: [`evaluation`, evaluationId],
        queryFn: () => GetEvaluation(evaluationId),
        enabled: !!evaluationId
    })

    /*const {
        data: surveys,
        isLoading: isLoadingSurveys,
        isError: isErrorSurveys,
        error: errorSurveys,
    }= useQuery<SurveySummary[]>({
        queryKey: ['surveys'],
        queryFn: PerformGetSurveys,
    });

    const {
        data: form,
        isLoading: isLoadingForm,
        isError: isErrorForm,
        error: errorForm
    } = useQuery<StudentContext>({
        queryKey: ['form', surveyId],
        queryFn: () => GetQuestionnaires(surveyId),
    })*/


    const { mutate: performQuestionnaireUpdate, isPending: isPerformQuestionnaireUpdating } = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: BackendPayload }) =>
            PerformQuestionnaireUpdate(id, payload),
        onSuccess: (_data, variables) => {
            client.invalidateQueries({
                queryKey: ['questionnaireUpdate', variables.id]
            });
        }
    })

    const { mutate: exportQuestionnaire, isPending: isExporting } = useMutation({
        mutationFn: (questionnaireId: string) => ExportQuestionnaire(questionnaireId),
    });

    const { mutate: deleteQuestionnaire, isPending: isDeletingQuestionnaire } = useMutation({
        mutationFn: (questionnaireId: string) => DeleteQuestionnaire(questionnaireId),
        onSuccess: (questionnaireId) => {
            client.invalidateQueries({
                queryKey: ['deletedQuestionnaire', questionnaireId],
            });
        }
    })

    const { mutate: loginWithGoogle, isPending: isLoggingIn } = useMutation({
        mutationFn: (idToken: string) => LoginWithGoogle(idToken)
    });

    return {
        createQuestionnaires, isCreatingQuestionnaire,
        questionnairesSummary, isLoadingQuestionnairesSummary, isErrorQuestionnairesSummary, errorQuestionnairesSummary,
        evaluation, isLoadingEvaluation, isErrorEvaluation, errorEvaluation,
        performQuestionnaireUpdate, isPerformQuestionnaireUpdating,
        deleteQuestionnaire, isDeletingQuestionnaire,
        loginWithGoogle, isLoggingIn,
        //form, isLoadingForm, isErrorForm, errorForm,
        //surveys,isLoadingSurveys,isErrorSurveys,errorSurveys,
        isExporting,exportQuestionnaire
    }
}