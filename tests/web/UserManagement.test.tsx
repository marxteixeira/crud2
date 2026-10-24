import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import UserForm from "@/components/users/UserForm";
import UserTable from "@/components/users/UserTable";
import type { UserAccount } from "@/lib/users";

const account: UserAccount = {
  id: "account-1",
  email: "operator@example.com",
  displayName: "Operador",
  pendingEmail: null,
  role: "Operador",
  status: "Ativa",
};

describe("UserManagement", () => {
  it("valida dados do convite e envia o perfil selecionado", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<UserForm submitting={false} onSubmit={onSubmit} />);
    fireEvent.change(screen.getByLabelText("E-mail"), { target: { value: "invalid" } });
    fireEvent.change(screen.getByLabelText("Nome"), { target: { value: "Nova pessoa" } });
    fireEvent.click(screen.getByRole("button", { name: "Enviar convite" }));

    expect(await screen.findByText("Informe um e-mail válido.")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();

    fireEvent.change(screen.getByLabelText("E-mail"), { target: { value: "new@example.com" } });
    fireEvent.change(screen.getByLabelText("Perfil"), { target: { value: "Administrador" } });
    fireEvent.click(screen.getByRole("button", { name: "Enviar convite" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledWith({
      email: "new@example.com",
      displayName: "Nova pessoa",
      role: "Administrador",
    }));
  });

  it("mostra situação e fornece uma ação acessível para inativar", () => {
    const onStatusChange = vi.fn();
    render(<UserTable accounts={[account]} onStatusChange={onStatusChange} />);

    expect(screen.getByText("Ativa")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Inativar Operador" }));

    expect(onStatusChange).toHaveBeenCalledWith(account, "Inativa");
  });

  it("exibe o novo e-mail até a confirmação do titular", () => {
    render(
      <UserForm
        account={{ ...account, pendingEmail: "next@example.com" }}
        submitting={false}
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.getByText("Novo e-mail aguardando confirmação: next@example.com")).toBeInTheDocument();
    expect(screen.getByLabelText("Perfil")).toHaveValue("Operador");
  });
});
