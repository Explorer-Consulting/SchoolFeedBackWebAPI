import { Question } from "@/models/StudentContext";
import LikertScale from "@/components/feedback/questions/LikertScale"
import MultinomialSingleChoice from "@/components/feedback/questions/MultinomialSingleChoice"
import MultinomialSingleChoiceOther from "@/components/feedback/questions/MultinomialSingleChoiceOther"
import MultipleChoice from "./questions/MultipleChoice";
import OpenEnded from "./questions/OpenEnded";

type Props = {
    q: Question;
    index: number;
    value: string | string[];
    onChange: (val: string | string[]) => void;
    isInvalid?: boolean;
};

export default function DynamicQuestion({ q, index, value, onChange, isInvalid }: Props) {

   switch (q.type) {
        case "LikertScaleOneToFive": {
            return (
                <LikertScale
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid} />
            );
        }

        case "MultinomialSingleChoice": {
            return (
                <MultinomialSingleChoice
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid} />
            );
        }

        case "MultinomialSingleChoiceOther": {
            return (
                <MultinomialSingleChoiceOther
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid} />
            );
        }
        case "MultipleChoice": {
            return (
                <MultipleChoice
                    q={q}
                    index={index}
                    value={Array.isArray(value) ? value : []}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid} />
            );
        }

        case "OpenEnded": {
            return (
                <OpenEnded
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid} />
            );
        }

        default:
            return null;
    }
}