import { useId, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes } from "react";

const controlClass =
  "w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-space-md py-2 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container disabled:opacity-60";

interface FieldShellProps {
  label: string;
  error?: string;
  hint?: string;
  children: (ids: { id: string; describedBy: string | undefined; invalid: boolean }) => ReactNode;
}

function FieldShell({ label, error, hint, children }: FieldShellProps): JSX.Element {
  const id = useId();
  const messageId = `${id}-message`;
  const message = error ?? hint;
  return (
    <div className="flex flex-col gap-space-xs">
      <label htmlFor={id} className="text-label-md text-on-surface">{label}</label>
      {children({ id, describedBy: message ? messageId : undefined, invalid: Boolean(error) })}
      {message && (
        <p id={messageId} role={error ? "alert" : undefined} className={`text-body-sm ${error ? "text-error" : "text-on-surface-variant"}`}>
          {message}
        </p>
      )}
    </div>
  );
}

interface InputFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  hint?: string;
}

export function InputField({ label, error, hint, className = "", ...props }: InputFieldProps): JSX.Element {
  return (
    <FieldShell label={label} error={error} hint={hint}>
      {({ id, describedBy, invalid }) => (
        <input id={id} aria-describedby={describedBy} aria-invalid={invalid || undefined} className={`${controlClass} ${className}`} {...props} />
      )}
    </FieldShell>
  );
}

interface SelectFieldProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  error?: string;
  hint?: string;
}

export function SelectField({ label, error, hint, className = "", children, ...props }: SelectFieldProps): JSX.Element {
  return (
    <FieldShell label={label} error={error} hint={hint}>
      {({ id, describedBy, invalid }) => (
        <select id={id} aria-describedby={describedBy} aria-invalid={invalid || undefined} className={`${controlClass} ${className}`} {...props}>
          {children}
        </select>
      )}
    </FieldShell>
  );
}
