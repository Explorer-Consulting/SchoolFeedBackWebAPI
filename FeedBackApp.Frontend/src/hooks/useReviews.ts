import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query"
import { CreateQuestionnaires, GetQuestionnaireSummary, GetEvaluation, UpdateEvaluation, DeleteQuestionnaire, LoginWithGoogle, GetFormByEmail ,GetSurveysAdmin} from "@/api/ReviewApi"
import { useParams } from "react-router-dom";
import { StudentContext } from "@/models/StudentContext"

export const useReviews = (email?) => {
    const client = useQueryClient();
    const { questionnaireId, evaluationId } = useParams();

    const { mutate: createQuestionnaires, isPending: isCreatingQuestionnaire } = useMutation<any, any, any>({
        mutationFn: (payload) => CreateQuestionnaires(payload),
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

    const {
        data: form,
        isLoading: isLoadingForm,
        isError: isErrorForm,
        error: errorForm
    } = useQuery<StudentContext>({
        queryKey: ['form', email],
        queryFn: () => GetFormByEmail(email!),
        enabled: !!email,
    })


    const { mutate: updateEvaluation, isPending: isUpdatingEvaluation } = useMutation({
        mutationFn: UpdateEvaluation,
        onSuccess: (evaluationId) => {
            client.invalidateQueries({
                queryKey: ['updatedEvaluation', evaluationId]
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
    });

    return {
        // Create
        createQuestionnaires, isCreatingQuestionnaire,
        // Summary
        questionnairesSummary, isLoadingQuestionnairesSummary, isErrorQuestionnairesSummary, errorQuestionnairesSummary,
        // Evaluation
        evaluation, isLoadingEvaluation, isErrorEvaluation, errorEvaluation,
        updateEvaluation, isUpdatingEvaluation,
        // Questionnaire actions
        deleteQuestionnaire, isDeletingQuestionnaire,
        // Export
        exportTeacherEvaluations, isExportingTeacher,
        exportGlobalSummary, isExportingSummary,
        // Auth
        loginWithGoogle, isLoggingIn,
        // Forms
        form, isLoadingForm, isErrorForm, errorForm,
        //getsurveyadmin
        adminSurveys,isLoadingAdminSurveys,isErrorAdminSurveys,errorAdminSurveys,refetchAdminSurveys,
    }
}