import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getUnansweredCount(context): number {
  if (!context) return 0;
  const total = context.subjects.reduce((sum, subject) => {
    const teachers = context.teachersBySubject[subject] ?? [];
    return sum + teachers.length;
  }, 0);
  return total ;
}