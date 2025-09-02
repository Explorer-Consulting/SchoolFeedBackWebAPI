import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import { StudentContext, Survey } from '@/models/StudentContext'

type StudentContextStore = {
    context: StudentContext | null
    setContext: (c: StudentContext | null) => void

    selectedSurveyId: string | null
    setSelectedSurveyId: (id: string | null) => void

    selectedTeacher: string | null
    setSelectedTeacher: (id: string | null) => void

    selectedSubject: string | null
    setSelectedSubject: (id: string | null) => void
}

export const useStudentContextStore = create<StudentContextStore>()(
    persist(
        (set) => ({
            context: null,
            setContext: (c) => set({ context: c }),

            selectedSurveyId: null,
            setSelectedSurveyId: (id) => set({ selectedSurveyId: id }),

            selectedTeacher: null,
            setSelectedTeacher: (teacher) => set({ selectedTeacher: teacher}),

            selectedSubject: null,
            setSelectedSubject: (subject) => set({ selectedSubject : subject}),

        }),
        {
            name: 'student_context',
            storage: createJSONStorage(() => sessionStorage)
        }
    )
)
