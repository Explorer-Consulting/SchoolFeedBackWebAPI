import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Question } from "@/models/StudentContext";

type Props = {
    q: Question;
    index: number;
    value: string| string[];
    onChange: (val: string | string[]) => void;
    isInvalid?: boolean;
};

export default function DynamicQuestion({ q,index, value, onChange,isInvalid }: Props) {

    const wrapper = "space-y-2 p-3 rounded-md border " + (isInvalid ? "border-red-500" : "border-transparent");
    
    switch (q.type) {
        case "LikertScaleOneToFive": {
            const v = String(value ?? "");
            const opts = ["1", "2", "3", "4", "5"];
            return (
                <div className={wrapper}>
                    <Label>{index}.{q.text}</Label>
                    <RadioGroup
                        value={v}
                        onValueChange={(val) => onChange(val)}
                        className="grid grid-cols-5 gap-4"
                    >
                        {opts.map((opt) => (
                            <div key={opt} className="flex items-center justify-center gap-2">
                                <RadioGroupItem id={`${q.id}-${opt}`} value={opt} />
                                <Label htmlFor={`${q.id}-${opt}`}>{opt}</Label>
                            </div>
                        ))}
                    </RadioGroup>
                </div>
            );
        }

        case "MultinomialSingleChoice": {
            const v = String(value ?? "");
            const options = q.options ?? [];
            return (
                <div className={wrapper}>
                    <Label>{index}.{q.text}</Label>
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
                </div>
            );
        }

        case "MultinomialSingleChoiceOther": {
            const v = String(value ?? "");
            const options = q.options ?? [];
            const predefinedIds = options.map((_, i) => String(i + 1));
            const isPredef = predefinedIds.includes(v);
            const isOther = v !== "" && !isPredef;

            return (
                <div className={wrapper}>
                    <Label>{index}.{q.text}</Label>
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
                </div>
            );
        }
        case "MultipleChoice": {
            const arr = Array.isArray(value) ? (value as string[]) : [];
            const toggle = (id: string) =>
                onChange(arr.includes(id) ? arr.filter((x) => x !== id) : [...arr, id]);

            return (
                <div className={wrapper}>
                    <Label>{index}.{q.text}</Label>
                    <div>
                        {(q.options ?? []).map((opt, idx) => {
                            const id = String(idx + 1); 
                            return (
                                <div key={id} className="flex items-center gap-3 space-x-2">
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
                </div>
            );
        }

        case "OpenEnded": {
            const v = String(value ?? "");
            const tooShort = v.trim().length < 20;
            return (
                <div className={wrapper}>
                    <Label>{index}.{q.text}</Label>
                    <Textarea
                        value={v}
                        onChange={(e) => onChange(e.target.value)}
                        placeholder="Rövid válasz..."
                        maxLength={300}
                    />
                    <p className="text-xs text-muted-foreground">{v.trim().length}/300 (min. 20)</p>
                </div>
            );
        }
        default:
            return null;
    }
}