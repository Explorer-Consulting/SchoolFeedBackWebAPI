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

/*export const PerformGetSurveys =async () => {
    const {data} =await apiClient.get('/surveys');
    return data;
}

export const GetQuestionnaires = async (id) => {
    const {data} =await apiClient.get(`/questionnaires/${id}`);
    return data;
}*/

export const CreateQuestionnaires = async (payload) => {
    const response = await apiClient.post(`/questionnaires`,payload); 
    return response.data;
}

export const GetQuestionnaireSummary = async (questionnaireId) => {
    const response = await apiClient.get(`/questionnaires/${questionnaireId}`) //globalisan az osszes statisztika admin keri le
    return response.data;
}

export const ExportQuestionnaire = async (questionnaireId) => {
  const { data } = await apiClient.get(`/questionnaires/${questionnaireId}/export`);
  return data;
};

export const GetEvaluation = async (evaluationId) => {
    const response = await apiClient.get(`/evaluations/${evaluationId}`)  //minden tanarnak a sajat tantargya ertekelese
    return response.data;
}

export const PerformQuestionnaireUpdate = async (id,payload) => {
    const {data} = await apiClient.patch(`/evaluations/${id}`,payload) //minden questionnal
    return data;
}

export const DeleteQuestionnaire = async (questionnaireId) => {
    const response = await apiClient.delete(`/questionnaires/${questionnaireId}`) //admin deletel mindent a db bol
    return response.data;
}
