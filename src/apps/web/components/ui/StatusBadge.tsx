export default function StatusBadge({ active, activeLabel = "Ativo", inactiveLabel = "Inativo" }: {
  active: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
}): JSX.Element {
  const tone = active ? "bg-tertiary-fixed text-on-tertiary-fixed-variant" : "bg-surface-container-high text-on-surface-variant";
  return (
    <span className={`inline-flex items-center rounded-full px-space-sm py-0.5 text-label-sm ${tone}`}>
      {active ? activeLabel : inactiveLabel}
    </span>
  );
}
