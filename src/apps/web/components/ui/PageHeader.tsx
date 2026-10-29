import type { ReactNode } from "react";

export default function PageHeader({ title, description, action }: {
  title: string;
  description?: string;
  action?: ReactNode;
}): JSX.Element {
  return (
    <div className="mb-space-xl flex flex-col justify-between gap-space-md lg:flex-row lg:items-center">
      <div>
        <h1 className="font-headline text-headline-lg-mobile text-on-surface md:text-headline-lg">{title}</h1>
        {description && <p className="mt-1 text-body-md text-on-surface-variant">{description}</p>}
      </div>
      {action && <div className="flex items-center gap-space-sm self-start lg:self-auto">{action}</div>}
    </div>
  );
}
