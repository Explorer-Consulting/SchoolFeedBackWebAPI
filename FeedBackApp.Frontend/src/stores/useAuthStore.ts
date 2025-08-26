import {create} from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import {User} from "@/models/User"

type AuthStore ={
    user:User | null
    setUser: (u:User | null) => void
}

export const useAuthStore = create<AuthStore>()(
     persist(
    (set) => ({
      user: null,
      setUser: (u) => set({ user: u }),
    }),
    {
      name: 'auth',
      storage: createJSONStorage(() => sessionStorage),
    }
  )
)