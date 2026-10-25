import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import ProductForm from "@/components/products/ProductForm";

const racks = [{
  id: "rack-1",
  code: "A",
  description: null,
  status: "Active" as const,
  shelves: [{ id: "shelf-1", rackId: "rack-1", code: "1", status: "Active" as const }],
}];

describe("ProductForm", () => {
  it("omits price for an operator and submits a product without price", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<ProductForm racks={racks} isAdministrator={false} submitting={false} onSubmit={onSubmit} />);

    expect(screen.queryByLabelText("Preço (R$)")).not.toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Nome"), { target: { value: "Café" } });
    fireEvent.change(screen.getByLabelText("Quantidade"), { target: { value: "4" } });
    fireEvent.change(screen.getByLabelText("Estante"), { target: { value: "rack-1" } });
    fireEvent.change(screen.getByLabelText("Prateleira"), { target: { value: "shelf-1" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar produto" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledWith({
      name: "Café", quantity: 4, shelfId: "shelf-1",
    }));
  });

  it("shows a price field only to administrators and validates its precision", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<ProductForm racks={racks} isAdministrator submitting={false} onSubmit={onSubmit} />);
    fireEvent.change(screen.getByLabelText("Nome"), { target: { value: "Café" } });
    fireEvent.change(screen.getByLabelText("Quantidade"), { target: { value: "4" } });
    fireEvent.change(screen.getByLabelText("Preço (R$)"), { target: { value: "10.123" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar produto" }));

    expect(await screen.findByText("Informe um preço não negativo com até duas casas decimais."))
      .toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });
});
