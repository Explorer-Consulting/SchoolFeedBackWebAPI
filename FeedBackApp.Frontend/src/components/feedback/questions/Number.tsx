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
                type="number"
                inputMode="decimal"
                step="any"
                value={String(value ?? "")}
                onChange={(e) => onChange(e.target.value)}
                onWheel={(e) => e.currentTarget.blur()}  // görgetésre ne változzon az érték
                placeholder="Írj be egy számot..."
                className="max-w-xs"
            />
        </QuestionWrapper>
    );
}