import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";
import { Checkbox } from "@/components/ui/checkbox";

export default function MultipleChoice({ q, index, value, onChange, isInvalid, description }: BaseQuestionProps) {
    const arr = Array.isArray(value) ? (value as string[]) : [];
    const toggle = (id: string) =>
        onChange(arr.includes(id) ? arr.filter((x) => x !== id) : [...arr, id]);

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
            <div className="space-y-2">
                {(q.options ?? []).map((opt, idx) => {
                    const id = String(idx + 1);
                    return (
                        <div key={id} className="flex items-center gap-2">
                            <Checkbox
                                id={`${q.id}-${id}`}
                                checked={arr.includes(id)}
                                onCheckedChange={() => toggle(id)}
                            />
                            <Label htmlFor={`${q.id}-${id}`}>{opt}</Label>
                        </div>
                    );
                })}
            </div>
        </QuestionWrapper>
    );
}