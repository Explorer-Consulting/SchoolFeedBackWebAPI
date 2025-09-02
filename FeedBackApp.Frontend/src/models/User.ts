export enum Role {
    Admin = 'Admin',
    Student = 'Student',
    Teacher = 'Teacher',
}

export type User = {
    firstName: string;
    lastName: string;
    email: string;
    role: Role;
}