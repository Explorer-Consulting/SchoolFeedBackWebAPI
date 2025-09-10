import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { RadioGroupItem, RadioGroup } from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";

export default function MultinomialSingleChoice({ q, index, value, onChange, isInvalid,description }: BaseQuestionProps) {
    const v = String(value ?? "");
    const options = q.options ?? [];
    return (
        <QuestionWrapper isInvalid={isInvalid}>
             <div className="flex flex-col gap-1">
                <Label className="font-medium">
                    {index}. {q.text}
                </Label>
                {description && (
                    <span className="text-sm text-muted-foreground">
                        {description}
                    </span>
                )}
            </div>
            <RadioGroup
                value={v}
                onValueChange={(val) => onChange(val)}
            >
                {options.map((opt, idx) => {
                    const id = String(idx + 1);
                    return (
                        <div key={id} className="flex items-center space-x-2">
                            <RadioGroupItem id={`${q.id}-${id}`} value={id} />
                            <Label htmlFor={`${q.id}-${id}`}>{opt}</Label>
                        </div>
                    );
                })}
            </RadioGroup>
        </QuestionWrapper>
    );
}