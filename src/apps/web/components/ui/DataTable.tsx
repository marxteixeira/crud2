import type { ReactNode } from "react";

export interface DataTableProps {
  caption: string;
  headers: string[];
  children: ReactNode;
  pagination?: {
    page: number;
    hasPrevious: boolean;
    hasNext: boolean;
    onPrevious: () => void;
    onNext: () => void;
  };
}

export function DataTable({ caption, headers, children, pagination }: DataTableProps): JSX.Element {
  return (
    <div className="overflow-hidden rounded-xl bg-surface-container-lowest shadow-sm ring-1 ring-outline-variant/50">
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <caption className="sr-only">{caption}</caption>
          <thead className="bg-surface-container-low">
            <tr>
              {headers.map((header) => (
                <th key={header} scope="col" className="px-space-lg py-space-md text-label-sm uppercase tracking-wider text-on-surface-variant">
                  {header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/40 text-body-md">{children}</tbody>
        </table>
      </div>
      {pagination && (
        <nav aria-label="Paginação" className="flex items-center justify-between border-t border-outline-variant/40 px-space-lg py-space-md">
          <span className="text-body-sm text-on-surface-variant">Página {pagination.page}</span>
          <div className="flex gap-space-sm">
            <button type="button" disabled={!pagination.hasPrevious} onClick={pagination.onPrevious} className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">
              Anterior
            </button>
            <button type="button" disabled={!pagination.hasNext} onClick={pagination.onNext} className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">
              Próxima
            </button>
          </div>
        </nav>
      )}
    </div>
  );
}

export function DataRow({ children }: { children: ReactNode }): JSX.Element {
  return <tr className="hover:bg-surface-container-low/60">{children}</tr>;
}

export function DataCell({ children, className = "" }: { children: ReactNode; className?: string }): JSX.Element {
  return <td className={`px-space-lg py-space-md ${className}`}>{children}</td>;
}

export default DataTable;
