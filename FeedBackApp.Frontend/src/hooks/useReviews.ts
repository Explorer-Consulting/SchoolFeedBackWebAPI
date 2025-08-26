import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query"
import { CreateQuestionnaires, GetQuestionnaireSummary, GetEvaluation, UpdateEvaluation, DeleteQuestionnaire, LoginWithGoogle , GetFormByEmail} from "@/api/ReviewApi"
import { useParams } from "react-router-dom";
import {StudentContext} from "@/models/StudentContext"

export const useReviews = (email?) => {
    const client = useQueryClient();
    const { questionnaireId, evaluationId } = useParams();

    const { mutate: createQuestionnaires, isPending: isCreatingQuestionnaire } = useMutation({
        mutationFn:(payload: { startDate: string; endDate: string }) => CreateQuestionnaires(payload),
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
        queryFn:() => GetQuestionnaireSummary(questionnaireId),
    })

    const {
        data: evaluation,
        isLoading: isLoadingEvaluation,
        isError: isErrorEvaluation,
        error: errorEvaluation
    } = useQuery({
        queryKey: [`evaluation`, evaluationId],
        queryFn:() => GetEvaluation(evaluationId),
        enabled: !!evaluationId
    })

    const{
        data: form,
        isLoading: isLoadingForm,
        isError: isErrorForm,
        error: errorForm
    } =useQuery<StudentContext>({
        queryKey: ['form',email],
        queryFn:() => GetFormByEmail(email!),
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
        mutationFn: DeleteQuestionnaire,
        onSuccess: (questionnaireId) => {
            client.invalidateQueries({
                queryKey: ['deletedQuestionnaire', questionnaireId],
            });
        }
    })

    const { mutate: loginWithGoogle, isPending: isLoggingIn} = useMutation({
    mutationFn: (idToken:string) => LoginWithGoogle(idToken)
    });

    return {
        createQuestionnaires,isCreatingQuestionnaire,
        questionnairesSummary,isLoadingQuestionnairesSummary,isErrorQuestionnairesSummary,errorQuestionnairesSummary,
        evaluation,isLoadingEvaluation,isErrorEvaluation,errorEvaluation,
        updateEvaluation,isUpdatingEvaluation,deleteQuestionnaire,isDeletingQuestionnaire,
        loginWithGoogle,isLoggingIn,
        form,isLoadingForm,isErrorForm,errorForm
    }
}