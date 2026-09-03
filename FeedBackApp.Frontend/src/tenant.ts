export type TenantDescription = {
  institutionName: string;
  studentIntro: {
    paragraphs: string[];
    legalNotice?: string;
  };
};

export const tenant: TenantDescription = JSON.parse(import.meta.env.VITE_TENANT_DESCRIPTION);
