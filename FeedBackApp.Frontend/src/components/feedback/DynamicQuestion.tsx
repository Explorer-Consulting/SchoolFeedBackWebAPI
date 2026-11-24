import { Question } from "@/models/StudentContext";
import LikertScale from "@/components/feedback/questions/LikertScale"
import MultinomialSingleChoice from "@/components/feedback/questions/MultinomialSingleChoice"
import MultinomialSingleChoiceOther from "@/components/feedback/questions/MultinomialSingleChoiceOther"
import MultipleChoice from "./questions/MultipleChoice";
import OpenEnded from "./questions/OpenEnded";
import NumberQuestion from "./questions/Number";
import DateQuestion from "./questions/Date";
import DropDownQuestion from "./questions/DropDown";

type Props = {
    q: Question;
    index: number;
    value: string | string[];
    onChange: (val: string | string[]) => void;
    isInvalid?: boolean;
    description?: string;
    readOnly?: boolean;
    parentValue?: string;
};

export default function DynamicQuestion({ q, index, value, onChange, isInvalid, description, readOnly, parentValue }: Props) {

   switch (q.type) {
        case "LikertScaleOneToFive": {
            return (
                <LikertScale
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description} />
            );
        }

        case "MultinomialSingleChoice": {
            return (
                <MultinomialSingleChoice
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description} />
            );
        }

        case "MultinomialSingleChoiceOther": {
            return (
                <MultinomialSingleChoiceOther
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description} />
            );
        }
        case "MultipleChoice": {
            return (
                <MultipleChoice
                    q={q}
                    index={index}
                    value={Array.isArray(value) ? value : []}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description} />
            );
        }

        case "OpenEnded": {
            return (
                <OpenEnded
                    q={q}
                    index={index}
                    value={String(value ?? "")}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description} />
            );
        }
        case "Number":
            return <NumberQuestion q={q} index={index} value={String(value ?? "")}
                                onChange={(val) => onChange(val)} isInvalid={isInvalid} description={description} />;

        case "Date":
            return <DateQuestion q={q} index={index} value={String(value ?? "")}
                                onChange={(val) => onChange(val)} isInvalid={isInvalid} description={description} />;

        case "DropDown":
            return (
                <DropDownQuestion
                q={q}
                index={index}
                value={typeof value === "string" ? value : ""}
                onChange={(val) => onChange(val)}
                isInvalid={isInvalid}
                description={description}
                readOnly={readOnly}
                />
            );  
                
        case "FilteredDropDown": {
            const visibleItems = parentValue
                ? (q.dropDownItems ?? []).filter((item) => item.parentId === parentValue)
                : [];

            return (
                <DropDownQuestion
                    q={{ ...q, dropDownItems: visibleItems }}
                    index={index}
                    value={typeof value === "string" ? value : ""}
                    onChange={(val) => onChange(val)}
                    isInvalid={isInvalid}
                    description={description}
                    readOnly={readOnly}
                    emptyPlaceholder={parentValue ? "Nincs választható lehetőség" : "Előbb válaszolj az előző kérdésre"}
                />
            );
        }
        default:
            return null;
    }
}