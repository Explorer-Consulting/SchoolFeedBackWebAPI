import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query"
import { CreateQuestionnaires, PerformSendReports, PerformGenerateReports, DeleteQuestionnaire, LoginWithGoogle, LoginWithMicrosoft, LoginWithLinkedIn, GetSurveysAdmin, PerformGetSurveys, GetQuestionnaires, PerformQuestionnaireUpdate, PerformQuestionnaireSubmit, SendOTP, VerifyOTP,  EnableSelfOptIn, 
    GenerateShareLink, GenerateValidationToken } from "@/api/ReviewApi"
import { BackendPayload } from "@/utils/toBackendPayload";
import { Survey } from "@/models/StudentContext"

export const useReviews = (selectedSurveyId?: string) => {
    const client = useQueryClient();

    const { mutate: loginWithGoogle, isPending: isLoggingIn } = useMutation({
        mutationFn: (idToken: string) => LoginWithGoogle(idToken) as Promise<any>
    });

    const { mutate: sendOTP, isPending: isSendingOTP } = useMutation({
        mutationFn: (email: string) => SendOTP(email) as Promise<any>
    });

    const { mutate: verifyOTP, isPending: isVerifyingOTP } = useMutation({
        mutationFn: ({ email, code }: { email: string; code: string }) => VerifyOTP(email, code) as Promise<any>
    });

    const { mutate: loginWithMicrosoft, isPending: isLoggingInMicrosoft } = useMutation({
        mutationFn: (idToken: string) => LoginWithMicrosoft(idToken)
    });

    const { mutate: loginWithLinkedIn, isPending: isLoggingInLinkedIn } = useMutation({
        mutationFn: (authCode: string) => LoginWithLinkedIn(authCode)
    });

    const { mutate: generateValidationToken, isPending: isGeneratingValidationToken } = useMutation({
    mutationFn: ({ surveyId, studentEmail }: { surveyId: string; studentEmail: string }) =>
        GenerateValidationToken(surveyId, studentEmail)
    });

    const {
        data: questionnaires,
        isLoading: isLoadingQuestionnaire,
        isError: isErrorQuestionnaire,
        error: errorQuestionnaire,
        refetch: refetchQuestionnaires
    } = useQuery({
        queryKey: ['questionnaires', selectedSurveyId],
        queryFn: () => GetQuestionnaires(selectedSurveyId!),
        enabled: !!selectedSurveyId
    });

    const {
        data: querySurveys,
        isLoading: isLoadingSurveys,
        isError: isErrorSurveys,
        error: errorSurveys,
        refetch: refetchSurveys
    } = useQuery<Survey[]>({
        queryKey: [`surveys`],
        queryFn: PerformGetSurveys,
        enabled: false
    });

    const { mutate: createQuestionnaires, isPending: isCreatingQuestionnaire } = useMutation<any, any, any>({
        mutationFn: (payload) => CreateQuestionnaires(payload),
        onSuccess: () => {
            client.invalidateQueries({
                queryKey: ['questionnaires']
            });
        }
    });

    const { mutate: performGenerateReports, isPending: isGeneratingReports } = useMutation({
        mutationFn: (questionnaireTemplateId: string) => PerformGenerateReports(questionnaireTemplateId)
    });

    const { mutate: performSendReports, isPending: isSendingReports } = useMutation({
        mutationFn: (questionnaireTemplateId: string) => PerformSendReports(questionnaireTemplateId)
    });

    const { mutate: deleteQuestionnaire, isPending: isDeletingQuestionnaire } = useMutation({
        mutationFn: (questionnaireId: string) => DeleteQuestionnaire(questionnaireId),
        onSuccess: (questionnaireId) => {
            client.invalidateQueries({
                queryKey: ['deletedQuestionnaire', questionnaireId],
            });
        }
    });

    const { mutate: performQuestionnaireUpdate, isPending: isPerformQuestionnaireUpdating } = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: BackendPayload }) =>
            PerformQuestionnaireUpdate(id, payload),
        onSuccess: (_data, variables) => {
            client.invalidateQueries({
                queryKey: ['questionnaireUpdate', variables.id]
            });
        }
    });

    const { mutate: performQuestionnaireSubmit, isPending: isPerformQuestionnaireSubmit } = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: BackendPayload }) =>
            PerformQuestionnaireSubmit(id, payload),
        onSuccess: (_data, variables) => {
            client.invalidateQueries({
                queryKey: ['questionnaireSubmit', variables.id]
            });
        }
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

    const { mutate: enableSelfOptIn, isPending: isEnablingSelfOptIn } = useMutation({
        mutationFn: (templateId: string) => EnableSelfOptIn(templateId)
    });

    const { mutate: generateShareLink, isPending: isGeneratingShareLink } = useMutation({
        mutationFn: ({ templateId, minutes }: { templateId: string; minutes?: number }) => 
            GenerateShareLink(templateId, minutes)
    });

    return {
        createQuestionnaires, isCreatingQuestionnaire,
        querySurveys, isLoadingSurveys, isErrorSurveys, errorSurveys, refetchSurveys,
        questionnaires, isLoadingQuestionnaire, isErrorQuestionnaire, errorQuestionnaire, refetchQuestionnaires,
        performQuestionnaireUpdate, isPerformQuestionnaireUpdating,
        performQuestionnaireSubmit, isPerformQuestionnaireSubmit,
        deleteQuestionnaire, isDeletingQuestionnaire,
        performGenerateReports, isGeneratingReports,
        performSendReports, isSendingReports,
        loginWithGoogle, isLoggingIn,
        loginWithMicrosoft, isLoggingInMicrosoft,
        sendOTP, isSendingOTP,
        verifyOTP, isVerifyingOTP,
        loginWithLinkedIn, isLoggingInLinkedIn,
        adminSurveys, isLoadingAdminSurveys, isErrorAdminSurveys, errorAdminSurveys, refetchAdminSurveys,
        enableSelfOptIn, isEnablingSelfOptIn,
        generateShareLink, isGeneratingShareLink,
        generateValidationToken, isGeneratingValidationToken,
    }
}