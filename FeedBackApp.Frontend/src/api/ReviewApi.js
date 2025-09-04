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
    const { data } = await apiClient.get(`/surveys/${id}`);
    return data;
};

export const PerformGetSurveys = async () => {
    const { data } = await apiClient.get(`/surveys`);
    return data;
};
export const CreateQuestionnaires = async (payload) => {
    const { data } = await apiClient.post(`/surveys`, payload);
    return data;
};

export const GetQuestionnaireSummary = async (questionnaireId) => {
    const { data } = await apiClient.get(`/summaries/${questionnaireId}`);
    return data;
}


export const GetEvaluation = async (evaluationId) => {
    const { data } = await apiClient.get(`/evaluations/${evaluationId}`);
    return data
};

export const DeleteQuestionnaire = async (questionnaireId) => {
    const { data } = await apiClient.delete(`/surveys/${questionnaireId}`);
    return data;
};

export const PerformQuestionnaireUpdate = async (id, payload) => {
    const { data } = await apiClient.patch(`/questionnaire/${id}`, payload);
    return data;
};

export const PerformQuestionnaireSubmit = async (id, payload) => {
    const { data } = await apiClient.post(`/questionnaire/${id}`, payload);
    return data;
}

export const GetSurveysAdmin = async () => {
    const { data } = await apiClient.get(`/surveys/admin`);
    return data;
};