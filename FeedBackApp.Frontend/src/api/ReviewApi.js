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
    const { data } = await apiClient.post('/auth/google', { IdToken: idToken });
    return data;
};
//LoginWithFacebook
export const LoginWithFacebook = async (accessToken) => {
    const { data } = await apiClient.post('/auth/facebook', { AccessToken: accessToken });
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