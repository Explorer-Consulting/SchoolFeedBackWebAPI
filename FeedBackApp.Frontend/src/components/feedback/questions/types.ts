import { Question } from "@/models/StudentContext";

export type BaseQuestionProps = {
  q: Question;
  index: number;
  value: string | string[];
  onChange: (val: string | string[]) => void;
  isInvalid?: boolean;
  description?: string;

  readOnly?: boolean; // preview miatt
};