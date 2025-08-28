import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import { StudentContext,Survey } from '@/models/StudentContext'

type StudentContextStore = {
    context: StudentContext | null
    setContext: (c: StudentContext | null) => void
    clearContext: () => void

    surveys: Survey[] | null;
    setSurveys: (s: Survey[]) => void;

    selectedSurveyId: string | null
    setSelectedSurveyId: (id: string | null) => void
}

export const useStudentContextStore = create<StudentContextStore>()(
    persist(
        (set) => ({
            context: null,
            setContext: (c) => set({ context: c }),
            clearContext: () =>
                set({
                    context: null,
                    surveys: null,
                    selectedSurveyId: null,
                }),
            surveys: null,
            setSurveys: (s) => set({ surveys: s }),

            selectedSurveyId: null,
            setSelectedSurveyId: (id) => set({ selectedSurveyId: id }),

        }),
        {
            name: 'student_context',
            storage: createJSONStorage(() => sessionStorage)
        }
    )
)
