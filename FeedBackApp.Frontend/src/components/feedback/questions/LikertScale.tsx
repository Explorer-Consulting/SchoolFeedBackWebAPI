import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import {RadioGroupItem,RadioGroup} from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";

export default function LikertScale({q,index,value,onChange,isInvalid}:BaseQuestionProps){
    const v = String(value ?? "");
    const opts = ["1", "2", "3", "4", "5"];
    return (
        <QuestionWrapper isInvalid={isInvalid}>
            <Label>{index}. {q.text}</Label>
            <RadioGroup
                value={v}
                onValueChange={(val) => onChange(val)}
                className="grid grid-cols-5 gap-4"
            >
                {opts.map((opt) => (
                    <div key={opt} className="flex items justify gap-2">
                        <RadioGroupItem id={`${q.id}-${opt}`} value={opt} />
                        <Label htmlFor={`${q.id}-${opt}`}>{opt}</Label>
                    </div>
                ))}
            </RadioGroup>
        </QuestionWrapper>
    );
}