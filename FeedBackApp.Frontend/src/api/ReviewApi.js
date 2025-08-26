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

export const GetFormByEmail = async (email) => {
    const {data} =await apiClient.get('/students/context' ,{params: {email}});
    return data;
}

export const CreateQuestionnaires = async (payload) => {
    const response = await apiClient.post(`/questionnaires`,payload); 
    return response.data;
}

export const GetQuestionnaireSummary = async (questionnaireId) => {
    const response = await apiClient.get(`/questionnaires/${questionnaireId}`) //globalisan az osszes statisztika admin keri le
    return response.data;
}

export const GetEvaluation = async (evaluationId) => {
    const response = await apiClient.get(`/evaluations/${evaluationId}`)  //minden tanarnak a sajat tantargya ertekelese
    return response.data;
}

export const UpdateEvaluation = async (evaluationId) => {
    const response = await apiClient.patch(`/evaluations/${evaluationId}`) //minden questionnal
    return response.data;
}

export const DeleteQuestionnaire = async (questionnaireId) => {
    const response = await apiClient.delete(`/questionnaires/${questionnaireId}`) //admin deletel mindent a db bol
    return response.data;
}
