import { SelfSignInLink } from '@/models/SelfSignInLink';

const STORAGE_KEY = 'self_sign_in_links';

export const selfSignInLinkStorage = {
  getAll: (): SelfSignInLink[] => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (!stored) return [];
    
    try {
      return JSON.parse(stored);
    } catch {
      return [];
    }
  },

  save: (link: SelfSignInLink): void => {
    const existing = selfSignInLinkStorage.getAll();
    const updated = [...existing, link];
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
  },

  delete: (id: string): void => {
    const existing = selfSignInLinkStorage.getAll();
    const filtered = existing.filter(link => link.id !== id);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
  },

  clearAll: (): void => {
    localStorage.removeItem(STORAGE_KEY);
  }
};