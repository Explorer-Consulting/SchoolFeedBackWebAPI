import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";
import { Textarea } from "@/components/ui/textarea";

export default function OpenEnded({ q, index, value, onChange, isInvalid, description }: BaseQuestionProps) {
    const v = String(value ?? "");
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
            <Textarea
                value={v}
                onChange={(e) => onChange(e.target.value)}
                placeholder="Rövid válasz..."
                maxLength={300}
            />
            <p className="text-xs text-muted-foreground">{v.trim().length}/300 (min. 20)</p>
        </QuestionWrapper>
    );
}