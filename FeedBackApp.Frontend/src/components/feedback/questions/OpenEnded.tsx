import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";
import { Textarea } from "@/components/ui/textarea";

export default function OpenEnded({ q, index, value, onChange, isInvalid }: BaseQuestionProps) {
     const v = String(value ?? "");
            return (
                <QuestionWrapper isInvalid={isInvalid}>
                    <Label>{index}. {q.text}</Label>
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