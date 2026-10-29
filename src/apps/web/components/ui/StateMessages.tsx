export function LoadingState({ message = "Carregando..." }: { message?: string }): JSX.Element {
  return <p role="status" className="rounded-xl bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">{message}</p>;
}

export function EmptyState({ message }: { message: string }): JSX.Element {
  return <p className="rounded-xl bg-surface-container-low p-space-lg text-center text-body-md text-on-surface-variant">{message}</p>;
}

export function ErrorState({ message }: { message: string }): JSX.Element {
  return <p role="alert" className="rounded-xl bg-error-container p-space-md text-body-md text-on-error-container">{message}</p>;
}

export function SuccessState({ message }: { message: string }): JSX.Element {
  return <p role="status" className="rounded-xl bg-tertiary-fixed p-space-md text-body-md text-on-tertiary-fixed-variant">{message}</p>;
}
