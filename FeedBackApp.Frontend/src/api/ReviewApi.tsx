import axios from "axios"

const API_URL = import.meta.env.VITE_API_BASE_URL

const apiClient = axios.create({
  baseURL: API_URL, 
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

//LoginWithGoogle
export const LoginWithGoogle = async (idToken) => {
  const { data } = await apiClient.post("/auth/google", { IdToken: idToken });
  return data;
};
//LoginWithMicrosoft
export const LoginWithMicrosoft = async (idToken) => {
    const { data } = await apiClient.post('/auth/microsoft', { IdToken: idToken });
    return data;
};
//LoginWithLinkedIn
export const LoginWithLinkedIn = async (accessToken) => {
    const { data } = await apiClient.post('/auth/linkedin', { AccessToken: accessToken });
    return data;
};

//PerformGetSurveyData
export const GetQuestionnaires = async (id) => {
    const { data } = await apiClient.get(`/surveys/${id}`);
    return data;
};
//PerformGetSurveys -> for students
export const PerformGetSurveys = async () => {
    const { data } = await apiClient.get(`/surveys`);
    return data;
};
//PerformQuestionnaireCompilation
export const CreateQuestionnaires = async (payload) => {
    const { data } = await apiClient.post(`/surveys`, payload);
    return data;
};
//PerformGenerateReports
export const PerformGenerateReports = async (questionnaireTemplateId) => {
    console.log(questionnaireTemplateId);
    const { data } = await apiClient.post(`/reports/${questionnaireTemplateId}`);
    return data;
}
//PerformSendReports
export const PerformSendReports = async (questionnaireTemplateId) => {
    console.log(questionnaireTemplateId);
    const { data } = await apiClient.post(`/reports/send/${questionnaireTemplateId}`);
    return data
};
//PerformQuestionnaireDeletion
export const DeleteQuestionnaire = async (questionnaireId) => {
    const { data } = await apiClient.delete(`/surveys/${questionnaireId}`);
    return data;
};
//PerformQuestionnaireUpdate
export const PerformQuestionnaireUpdate = async (id, payload) => {
    const { data } = await apiClient.patch(`/questionnaire/${id}`, payload);
    return data;
};
//PerformQuestionnaireSubmit
export const PerformQuestionnaireSubmit = async (id, payload) => {
    const { data } = await apiClient.post(`/questionnaire/${id}`, payload);
    return data;
}
//PerformGetSurveysAdmin
export const GetSurveysAdmin = async () => {
    const { data } = await apiClient.get(`/management/surveys`);
    return data;
};

// SendOTP - Sends OTP code to user's email
export const SendOTP = async (email: string): Promise<unknown> => {
    const { data } = await apiClient.post('/auth/otp/send', { email });
    return data;
};

// VerifyOTP - Verifies OTP code and logs in the user
export const VerifyOTP = async (email: string, code: string): Promise<unknown> => {
    const { data } = await apiClient.post('/auth/otp/verify', { email, code });
    return data;
};

export const GetQuestionnaireTemplatePreview = async (templateId) => {
    const { data } = await apiClient.get(`/questionnairetemplate/${templateId}/preview`); 
    return data;
};
// Enable self opt-in for a template (debug endpoint)
export const EnableSelfOptIn = async (templateId: string): Promise<unknown> => {
    const { data } = await apiClient.post(`/debug/templates/${templateId}/enable-optin`);
    return data;
};

// Generate share link for QR code
export const GenerateShareLink = async (
    templateId: string, 
    minutes: number = 525600 * 50 // one year * 50 = 50 year
): Promise<{ url: string; expiresAt: string }> => {
    const { data } = await apiClient.get(`/optin/share-link/${templateId}`, {
        params: { minutes }
    });
    return data;
};

// Self opt-in - creates a questionnaire instance for the authenticated user
export const SelfOptIn = async (templateId: string, optInToken: string): Promise<unknown> => {
    const { data } = await apiClient.post(`/templates/${templateId}/self-opt-in`, {
        optInToken: optInToken
    });
    return data;
};

// Generate Validation token for QR scan
export const GenerateValidationToken = async(surveyId: string, studentEmail: string) => {
    const { data } = await apiClient.post(`/surveys/${surveyId}/validation-token`, {studentEmail})
    return data;
}