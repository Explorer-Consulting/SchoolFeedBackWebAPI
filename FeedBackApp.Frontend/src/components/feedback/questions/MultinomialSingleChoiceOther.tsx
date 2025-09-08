import type { BaseQuestionProps } from "@/components/feedback/questions/types";
import { RadioGroupItem, RadioGroup } from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label";
import QuestionWrapper from "@/components/feedback/questions/QuestionWrapper";
import { Input } from "@/components/ui/input";

export default function MultinomialSingleChoiceOther({ q, index, value, onChange, isInvalid }: BaseQuestionProps) {
    const v = String(value ?? "");
            const options = q.options ?? [];
            const predefinedIds = options.map((_, i) => String(i + 1));
            const isPredef = predefinedIds.includes(v);
            const isOther = v !== "" && !isPredef;

            return (
                <QuestionWrapper isInvalid={isInvalid}>
                    <Label>{index}. {q.text}</Label>
                    <RadioGroup
                        value={isPredef ? v : ""}
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
                    <Input
                        placeholder="Egyéb, éspedig:"
                        value={isOther ? v : ""}
                        onChange={(e) => onChange(e.target.value)}
                    />
                </QuestionWrapper>
            );
}