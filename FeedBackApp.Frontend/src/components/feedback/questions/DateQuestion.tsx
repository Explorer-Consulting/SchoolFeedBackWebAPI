import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";

export default function NumberQuestion({ q, index, value, onChange, isInvalid, description }: BaseQuestionProps) {
    return (
        <QuestionWrapper isInvalid={isInvalid}>
            <div className="flex flex-col gap-1">
                <Label className="font-medium">{index}. {q.text}</Label>
                {description && <span className="text-sm text-muted-foreground">{description}</span>}
            </div>
            <Input
                type="date"
                value={String(value ?? "")}
                onChange={(e) => onChange(e.target.value)}   // "yyyy-MM-dd"
                className="max-w-xs"
            />
        </QuestionWrapper>
    );
}