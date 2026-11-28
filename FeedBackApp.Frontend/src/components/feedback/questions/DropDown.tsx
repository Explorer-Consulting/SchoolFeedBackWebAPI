import { useEffect, useId, useState } from "react";
import type { BaseQuestionProps } from "./types";
import QuestionWrapper from "./QuestionWrapper";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const OTHER_VALUE = "__other__";

export default function DropDownQuestion({
  q,
  index,
  value,
  onChange,
  isInvalid,
  description,
  readOnly,
  emptyPlaceholder = "Nincsenek választható lehetőségek"
}: BaseQuestionProps & { emptyPlaceholder?: string }) {
  const inputId = useId();
  const otherInputId = `${inputId}-other`;
  const descriptionId = `${inputId}-description`;

  const items = q.dropDownItems ?? [];
  const allowOther = Boolean(q.allowOther);
  const currentValue = typeof value === "string" ? value : "";
  const matchesKnownItem = items.some((item) => item.id === currentValue);

  const [isOtherSelected, setIsOtherSelected] = useState<boolean>(
    allowOther && currentValue !== "" && !matchesKnownItem
  );

  useEffect(() => {
    if (!allowOther) {
      setIsOtherSelected(false);
      return;
    }
    if (currentValue !== "" && !matchesKnownItem) {
      setIsOtherSelected(true);
    } else if (matchesKnownItem) {
      setIsOtherSelected(false);
    }
  }, [allowOther, currentValue, matchesKnownItem]);

  const selectValue = isOtherSelected ? OTHER_VALUE : (matchesKnownItem ? currentValue : "");
  const hasOptions = items.length > 0 || allowOther;

  const handleSelectChange = (selected: string) => {
    if (selected === OTHER_VALUE) {
      setIsOtherSelected(true);
      onChange("");
      return;
    }

    setIsOtherSelected(false);
    onChange(selected);
  };

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
        value={selectValue}
        onValueChange={handleSelectChange}
        disabled={readOnly || !hasOptions}
      >
        <SelectTrigger
          id={inputId}
          className="w-full max-w-md"
          aria-invalid={isInvalid}
          aria-describedby={description ? descriptionId : undefined}
        >
          <SelectValue
            placeholder={
              !hasOptions
                ? emptyPlaceholder
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
          {allowOther && (
            <SelectItem value={OTHER_VALUE}>
              Egyéb...
            </SelectItem>
          )}
        </SelectContent>
      </Select>

      {allowOther && isOtherSelected && (
        <div className="flex flex-col gap-1 max-w-md">
          <Label htmlFor={otherInputId} className="text-sm">
            Egyéb válasz megadása
          </Label>
          <Input
            id={otherInputId}
            type="text"
            value={matchesKnownItem ? "" : currentValue}
            disabled={readOnly}
            placeholder="Írd be a választ..."
            onChange={(e) => onChange(e.target.value)}
          />
        </div>
      )}
    </QuestionWrapper>
  );
}