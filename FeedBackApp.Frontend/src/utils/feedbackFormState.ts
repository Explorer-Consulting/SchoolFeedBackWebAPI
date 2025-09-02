export interface FeedbackFormState {
    [key: `q${number}`]: string | string[];
    q0: string;
    q1: string;
    q2: string;
    q3: string;
    q4: string;
    q5: string;
    q6: string;
    q7: string;
    q8: string;
    q9: string;
    q10: string;
    q11: string;
    q12: string;
    q13: string;
    q14: string;
    q15: string;
    q16: string;
    q17: string;
    q18: string;
    q19: string[];
    q20: string[];
    q21: string;
    q22: string;
    q23: string;
    q24: string;
    q25: string;
}

export const initialFeedbackForm: FeedbackFormState = {
    q0: "",
    q1: "",
    q2: "",
    q3: "",
    q4: "",
    q5: "",
    q6: "",
    q7: "",
    q8: "",
    q9: "",
    q10: "",
    q11: "",
    q12: "",
    q13: "",
    q14: "",
    q15: "",
    q16: "",
    q17: "",
    q18: "",
    q19: [],
    q20: [],
    q21: "",
    q22: "",
    q23: "",
    q24: "",
    q25: ""
};
