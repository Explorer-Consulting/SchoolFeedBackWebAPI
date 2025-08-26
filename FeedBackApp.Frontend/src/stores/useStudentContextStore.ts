import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import { StudentContext } from '@/models/StudentContext'

type StudentContextStore = {
    context: StudentContext | null
    setContext: (c: StudentContext | null) => void
    clearContext: () => void
}

export const useStudentContextStore = create<StudentContextStore>()(
    persist(
        (set) => ({
            context: null,
            setContext: (c) => set({ context: c }),
            clearContext: () => set({ context: null }),
        }),
        {
            name: 'student_context',
            storage: createJSONStorage(() => localStorage)
        }
    )
)
