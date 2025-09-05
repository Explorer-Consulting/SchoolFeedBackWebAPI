import { PropsWithChildren } from "react";

type Props = PropsWithChildren<{ isInvalid?: boolean }>;

export default function QuestionWrapper({ isInvalid, children }: Props) {
  const cls =
    "space-y-2 p-3 rounded-md border " +
    (isInvalid ? "border-red-500" : "border-transparent");

  return <div className={cls}>{children}</div>;
}
