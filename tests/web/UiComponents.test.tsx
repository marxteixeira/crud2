import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import Dialog from "@/components/ui/Dialog";
import { DataRow, DataCell, DataTable } from "@/components/ui/DataTable";
import { InputField } from "@/components/ui/Field";

describe("componentes de UI compartilhados", () => {
  it("fecha o diálogo com Esc", () => {
    const onClose = vi.fn();
    render(<Dialog open title="Convidar" onClose={onClose}><p>Conteúdo</p></Dialog>);
    fireEvent.keyDown(screen.getByRole("dialog", { hidden: true }), { key: "Escape" });
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("não renderiza o diálogo fechado", () => {
    render(<Dialog open={false} title="Convidar" onClose={() => undefined}><p>Conteúdo</p></Dialog>);
    expect(screen.queryByText("Conteúdo")).not.toBeInTheDocument();
  });

  it("associa a mensagem de erro ao campo", () => {
    render(<InputField label="E-mail" error="Informe um e-mail válido." />);
    const input = screen.getByLabelText("E-mail");
    const message = screen.getByRole("alert");
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input.getAttribute("aria-describedby")).toBe(message.id);
  });

  it("aciona os callbacks de paginação", () => {
    const onPrevious = vi.fn();
    const onNext = vi.fn();
    render(
      <DataTable caption="Lista" headers={["Nome"]} pagination={{ page: 2, hasPrevious: true, hasNext: true, onPrevious, onNext }}>
        <DataRow><DataCell>Item</DataCell></DataRow>
      </DataTable>,
    );
    fireEvent.click(screen.getByRole("button", { name: "Anterior" }));
    fireEvent.click(screen.getByRole("button", { name: "Próxima" }));
    expect(onPrevious).toHaveBeenCalledTimes(1);
    expect(onNext).toHaveBeenCalledTimes(1);
  });
});
