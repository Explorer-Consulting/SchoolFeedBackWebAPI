import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query"
import { CreateQuestionnaires, GetQuestionnaireSummary, GetEvaluation, DeleteQuestionnaire, LoginWithGoogle,GetSurveysAdmin,PerformGetSurveys,GetQuestionnaires,PerformQuestionnaireUpdate, PerformQuestionnaireSubmit} from "@/api/ReviewApi"
import { useParams } from "react-router-dom";
import { BackendPayload, toBackendPayload } from "@/utils/toBackendPayload";
import {Survey} from "@/models/Survey"
import { useAuthStore } from "@/stores/useAuthStore";
import { Variable } from "lucide-react";

export const useReviews = (selectedSurveyId?: string) => {
    const client = useQueryClient();
    const { questionnaireId, evaluationId } = useParams();
    const user=useAuthStore((s)=>s.user);

    const { mutate: createQuestionnaires, isPending: isCreatingQuestionnaire } = useMutation<any, any, any>({
        mutationFn: (payload) => CreateQuestionnaires(payload),
        onSuccess: () => {
            client.invalidateQueries({
                queryKey: ['questionnaires']
            });
        }
    })

    const {
        data: surveys,
        isLoading: isLoadingSurveys,
        isError: isErrorSurveys,
        error: errorSurveys,
        refetch: refetchSurveys 
    } = useQuery<Survey[]>({
        queryKey: [`surveys`],
        queryFn: PerformGetSurveys,
        enabled: false
    })

   const{
        data: questionnaires,
        isLoading: isLoadingQuestionnaire,
        isError: isErrorQuestionnaire,
        error: errorQuestionnaire,
        refetch: refetchQuestionnaires
    } = useQuery ({
        queryKey: ['questionnaires',selectedSurveyId],
        queryFn: () => GetQuestionnaires(selectedSurveyId!),
        enabled: !!selectedSurveyId
    }) 

    const {
        data: questionnairesSummary,
        isLoading: isLoadingQuestionnairesSummary,
        isError: isErrorQuestionnairesSummary,
        error: errorQuestionnairesSummary
    } = useQuery({
        queryKey: [`questionnairesSummary`, questionnaireId],
        queryFn: () => GetQuestionnaireSummary(questionnaireId),
        enabled: !!questionnaireId,
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

    const { mutate: performQuestionnaireUpdate, isPending: isPerformQuestionnaireUpdating } = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: BackendPayload }) =>
            PerformQuestionnaireUpdate(id, payload),
        onSuccess: (_data, variables) => {
            client.invalidateQueries({
                queryKey: ['questionnaireUpdate', variables.id]
            });
        }
    })

    const { mutate: performQuestionnaireSubmit, isPending :  isPerformQuestionnaireSubmit } = useMutation({
        mutationFn: ({id,payload}: {id:string; payload: BackendPayload }) =>
             PerformQuestionnaireSubmit(id,payload),
        onSuccess: (_data, variables) => {
            client.invalidateQueries({
                queryKey: ['questionnaireSubmit',variables.id]
            });
        }
    })

    const { mutate: deleteQuestionnaire, isPending: isDeletingQuestionnaire } = useMutation({
        mutationFn: (questionnaireId: string) => DeleteQuestionnaire(questionnaireId),
        onSuccess: (questionnaireId) => {
            client.invalidateQueries({
                queryKey: ['deletedQuestionnaire', questionnaireId],
            });
        }
    })

    const { mutate: exportTeacherEvaluations, isPending: isExportingTeacher } = useMutation({
        mutationFn: (evaluationId: string) => GetEvaluation(evaluationId)
    });

    const { mutate: exportGlobalSummary, isPending: isExportingSummary } = useMutation({
        mutationFn: (questionnaireId: string) => GetQuestionnaireSummary(questionnaireId)
    });


    const { mutate: loginWithGoogle, isPending: isLoggingIn } = useMutation({
        mutationFn: (idToken: string) => LoginWithGoogle(idToken)
    });

    const {
        data: adminSurveys,
        isLoading: isLoadingAdminSurveys,
        isError: isErrorAdminSurveys,
        error: errorAdminSurveys,
        refetch: refetchAdminSurveys
    } = useQuery({
        queryKey: ['adminSurveys'],
        queryFn: () => GetSurveysAdmin(),
        enabled: false
    });

    return {
        createQuestionnaires, isCreatingQuestionnaire,
        surveys,isLoadingSurveys,isErrorSurveys,errorSurveys,refetchSurveys,
        questionnaires,isLoadingQuestionnaire,isErrorQuestionnaire,errorQuestionnaire,refetchQuestionnaires,
        questionnairesSummary, isLoadingQuestionnairesSummary, isErrorQuestionnairesSummary, errorQuestionnairesSummary,
        evaluation, isLoadingEvaluation, isErrorEvaluation, errorEvaluation,
        performQuestionnaireUpdate, isPerformQuestionnaireUpdating,
        performQuestionnaireSubmit,isPerformQuestionnaireSubmit,
        deleteQuestionnaire, isDeletingQuestionnaire,
        exportTeacherEvaluations, isExportingTeacher,
        exportGlobalSummary, isExportingSummary,
        loginWithGoogle, isLoggingIn,
        adminSurveys,isLoadingAdminSurveys,isErrorAdminSurveys,errorAdminSurveys,refetchAdminSurveys,
    }
}