import axios, { AxiosInstance } from "axios";
import type { BackendPayload } from "@/utils/toBackendPayload";
import type { Survey } from "@/models/StudentContext";

const API_URL: string = import.meta.env.VITE_API_BASE_URL;

const apiClient: AxiosInstance = axios.create({
    baseURL: API_URL,
    withCredentials: true,
    headers: {
        "Content-Type": "application/json",
    },
});

// LoginWithGoogle
export const LoginWithGoogle = async (idToken: string): Promise<unknown> => {
    const { data } = await apiClient.post('/auth/google', { IdToken: idToken });
    return data;
};

// PerformGetSurveyData
export const GetQuestionnaires = async (id: string): Promise<unknown> => {
    const { data } = await apiClient.get(`/surveys/${id}`);
    return data;
};

// PerformGetSurveys -> for students
export const PerformGetSurveys = async (): Promise<Survey[]> => {
    const { data } = await apiClient.get(`/surveys`);
    return data;
};

// PerformQuestionnaireCompilation
export const CreateQuestionnaires = async (payload: unknown): Promise<unknown> => {
    const { data } = await apiClient.post(`/surveys`, payload);
    return data;
};

// PerformGenerateReports
export const PerformGenerateReports = async (questionnaireTemplateId: string): Promise<unknown> => {
    console.log(questionnaireTemplateId);
    const { data } = await apiClient.post(`/reports/${questionnaireTemplateId}`);
    return data;
};

// PerformSendReports
export const PerformSendReports = async (questionnaireTemplateId: string): Promise<unknown> => {
    console.log(questionnaireTemplateId);
    const { data } = await apiClient.post(`/reports/send/${questionnaireTemplateId}`);
    return data;
};

// PerformQuestionnaireDeletion
export const DeleteQuestionnaire = async (questionnaireId: string): Promise<unknown> => {
    const { data } = await apiClient.delete(`/surveys/${questionnaireId}`);
    return data;
};

// PerformQuestionnaireUpdate
export const PerformQuestionnaireUpdate = async (id: string, payload: BackendPayload): Promise<unknown> => {
    const { data } = await apiClient.patch(`/questionnaire/${id}`, payload);
    return data;
};

// PerformQuestionnaireSubmit
export const PerformQuestionnaireSubmit = async (id: string, payload: BackendPayload): Promise<unknown> => {
    const { data } = await apiClient.post(`/questionnaire/${id}`, payload);
    return data;
};

// PerformGetSurveysAdmin
export const GetSurveysAdmin = async (): Promise<unknown> => {
    const { data } = await apiClient.get(`/management/surveys`);
    return data;
};

// SendOTP - Sends OTP code to user's email
export const SendOTP = async (email: string): Promise<unknown> => {
    const { data } = await apiClient.post('/auth/otp/send', { email });
    return data;
};


