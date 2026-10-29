"use client";

import { useEffect, useRef, type ReactNode } from "react";

export interface DialogProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
}

export default function Dialog({ open, title, onClose, children }: DialogProps): JSX.Element | null {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog || dialog.open) return;
    if (typeof dialog.showModal === "function") dialog.showModal();
    else dialog.setAttribute("open", "");
  }, [open]);

  if (!open) return null;

  return (
    <dialog
      ref={ref}
      aria-labelledby="dialog-title"
      onCancel={(event) => {
        event.preventDefault();
        onClose();
      }}
      onKeyDown={(event) => {
        if (event.key === "Escape") onClose();
      }}
      className="w-full max-w-lg rounded-xl bg-surface-container-lowest p-space-xl text-on-surface shadow-xl backdrop:bg-inverse-surface/50"
    >
      <h2 id="dialog-title" className="mb-space-md font-headline text-headline-sm">{title}</h2>
      {children}
    </dialog>
  );
}
