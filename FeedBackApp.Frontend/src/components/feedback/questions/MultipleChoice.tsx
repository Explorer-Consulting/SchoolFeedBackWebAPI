import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";
import { Checkbox } from "@/components/ui/checkbox";

export default function MultipleChoice({ q, index, value, onChange, isInvalid, description }: BaseQuestionProps) {
    const selected = Array.isArray(value) ? value : [];
    const maxSelections = q.maxSelections;
    const limitReached =
        maxSelections != null && selected.length >= maxSelections;

    const toggle = (id: string) => {
        if (selected.includes(id)) {
            onChange(selected.filter((selectedId) => selectedId !== id));
            return;
        }

        if (!limitReached) {
            onChange([...selected, id]);
        }
    };

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
                {maxSelections != null && (
                    <p className="text-sm text-muted-foreground" aria-live="polite">
                        Legfeljebb {maxSelections} válasz jelölhető.
                        Kiválasztva: {selected.length}.
                    </p>
                )}
            </div>
            <div className="space-y-2">
                {(q.options ?? []).map((opt, idx) => {
                    const id = String(idx + 1);
                    const checked = selected.includes(id);

                    return (
                        <div key={id} className="flex items-center gap-2">
                            <Checkbox
                                id={`${q.id}-${id}`}
                                checked={checked}
                                disabled={limitReached && !checked}
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