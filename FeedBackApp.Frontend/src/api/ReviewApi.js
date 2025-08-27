import axios from "axios"

const API_URL = import.meta.env.VITE_API_BASE_URL

const apiClient = axios.create({
    baseURL: API_URL,
    withCredentials: true,
    headers: {
        "Content-Type": "application/json",
    },
});

export const LoginWithGoogle = async (idToken) => {
    const { data } = await apiClient.post('/auth/google', { IdToken: idToken });
    return data;
};

export const GetQuestionnaires = async (id) => {
    const { data } = await apiClient.get(`/questionnaires/${id}`);
    return data;
};

export const PerformGetSurveys = async () => {
    const { data } = await apiClient.get(`/surveys`);
    return data;
};
export const CreateQuestionnaires = async (payload) => {
    const response = await apiClient.post(`/questionnaires`,payload); 
    return response.data;
};

export const GetQuestionnaireSummary = async (questionnaireId) => {
    const response = await apiClient.get(`/summaries/${questionnaireId}`);
    return response.data;
}


export const GetEvaluation = async (evaluationId) => {
    const response = await apiClient.get(`/evaluations/${evaluationId}`);
    return response.data;
};

export const DeleteQuestionnaire = async (questionnaireId) => {
    const response = await apiClient.delete(`/questionnaires/${questionnaireId}`);
    return response.data;
};

export const PerformQuestionnaireUpdate = async (id, payload) => {
    const { data } = await apiClient.patch(`/questionnaire/${id}`, payload);
    return data;
};

export const GetSurveysAdmin = async () => {
  const response = await apiClient.get(`/surveys/admin`);
  return response.data;
};