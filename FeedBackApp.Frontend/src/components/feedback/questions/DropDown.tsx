import { useId } from "react";
import type { BaseQuestionProps } from "./types";
import QuestionWrapper from "./QuestionWrapper";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

export default function DropDownQuestion({
  q,
  index,
  value,
  onChange,
  isInvalid,
  description,
  readOnly,
}: BaseQuestionProps) {
  const inputId = useId();
  const descriptionId = `${inputId}-description`;

  const items = q.dropDownItems ?? [];
  const selectedId = typeof value === "string" ? value : "";

  return (
    <QuestionWrapper isInvalid={isInvalid}>
      <div className="flex flex-col gap-1">
        <Label htmlFor={inputId} className="font-medium">
          {index}. {q.text}
        </Label>

        {description && (
          <span
            id={descriptionId}
            className="text-sm text-muted-foreground"
          >
            {description}
          </span>
        )}
      </div>

      <Select
        value={selectedId}
        onValueChange={(itemId) => onChange(itemId)}
        disabled={readOnly || items.length === 0}
      >
        <SelectTrigger
          id={inputId}
          className="w-full max-w-md"
          aria-invalid={isInvalid}
          aria-describedby={description ? descriptionId : undefined}
        >
          <SelectValue
            placeholder={
              items.length === 0
                ? "Nincsenek választható lehetőségek"
                : "Válassz egy lehetőséget..."
            }
          />
        </SelectTrigger>

        <SelectContent>
          {items.map((item) => (
            <SelectItem key={item.id} value={item.id}>
              {item.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </QuestionWrapper>
  );
}