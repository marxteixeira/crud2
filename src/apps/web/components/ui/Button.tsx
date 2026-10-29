import type { ButtonHTMLAttributes } from "react";

export type ButtonVariant = "primary" | "secondary" | "danger";

const variants: Record<ButtonVariant, string> = {
  primary: "bg-primary-container text-on-primary hover:bg-primary",
  secondary: "bg-surface-container-lowest text-on-surface ring-1 ring-outline-variant hover:bg-surface-container-low",
  danger: "bg-error text-on-error hover:opacity-90",
};

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
}

export default function Button({ variant = "primary", className = "", type = "button", ...props }: ButtonProps): JSX.Element {
  return (
    <button
      type={type}
      className={`inline-flex items-center justify-center gap-space-xs rounded-lg px-space-md py-2 text-label-md transition-colors disabled:cursor-not-allowed disabled:opacity-60 ${variants[variant]} ${className}`}
      {...props}
    />
  );
}
