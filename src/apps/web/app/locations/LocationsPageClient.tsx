"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import Dialog from "@/components/ui/Dialog";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { describeApiError } from "@/lib/api";
import {
  createRack,
  createShelf,
  listRacks,
  setRackStatus,
  setShelfStatus,
  updateRack,
  updateShelf,
  type LocationStatus,
  type Rack,
} from "@/lib/locations";

export default function LocationsPageClient(): JSX.Element {
  const queryClient = useQueryClient();
  const [selectedRackId, setSelectedRackId] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [rackDialogOpen, setRackDialogOpen] = useState(false);
  const [rackCreateError, setRackCreateError] = useState<string | null>(null);
  const [shelfCreateError, setShelfCreateError] = useState<string | null>(null);
  const racksQuery = useQuery({
    queryKey: ["locations", "racks"],
    queryFn: () => listRacks(getSupabaseBrowserClient()),
  });

  useEffect(() => {
    const racks = racksQuery.data;
    if (racks?.length && !racks.some((rack) => rack.id === selectedRackId)) {
      setSelectedRackId(racks[0].id);
    }
  }, [racksQuery.data, selectedRackId]);

  async function refresh(): Promise<void> {
    setErrorMessage(null);
    await queryClient.invalidateQueries({ queryKey: ["locations"] });
  }

  const createRackMutation = useMutation({
    mutationFn: (input: { code: string; description: string | null }) =>
      createRack(getSupabaseBrowserClient(), input),
    onSuccess: async (rack) => {
      queryClient.setQueryData<Rack[]>(["locations", "racks"], (current) =>
        current?.some((item) => item.id === rack.id) ? current : [...(current ?? []), rack],
      );
      setSelectedRackId(rack.id);
      await refresh();
    },
    onError: (error) => setRackCreateError(describeApiError(error, "Não foi possível cadastrar a estante. Verifique o código e tente novamente.")),
  });
  const updateRackMutation = useMutation({
    mutationFn: ({ rackId, code, description }: { rackId: string; code: string; description: string | null }) =>
      updateRack(getSupabaseBrowserClient(), rackId, { code, description }),
    onSuccess: refresh,
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível atualizar a estante.")),
  });
  const rackStatusMutation = useMutation({
    mutationFn: ({ rackId, status }: { rackId: string; status: LocationStatus }) =>
      setRackStatus(getSupabaseBrowserClient(), rackId, status),
    onSuccess: refresh,
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível alterar a situação da estante.")),
  });
  const createShelfMutation = useMutation({
    mutationFn: ({ rackId, code }: { rackId: string; code: string }) =>
      createShelf(getSupabaseBrowserClient(), rackId, { code }),
    onSuccess: (shelf) => {
      queryClient.setQueryData<Rack[]>(["locations", "racks"], (current) =>
        current?.map((rack) => rack.id === shelf.rackId
          ? {
              ...rack,
              shelves: rack.shelves.some((item) => item.id === shelf.id)
                ? rack.shelves
                : [...rack.shelves, shelf],
            }
          : rack),
      );
      void queryClient.invalidateQueries({ queryKey: ["locations"] });
    },
    onError: (error) => setShelfCreateError(
      describeApiError(error, "Não foi possível cadastrar a prateleira. O código pode já existir nesta estante; confira e tente novamente."),
    ),
  });
  const updateShelfMutation = useMutation({
    mutationFn: ({ rackId, shelfId, code }: { rackId: string; shelfId: string; code: string }) =>
      updateShelf(getSupabaseBrowserClient(), rackId, shelfId, { code }),
    onSuccess: refresh,
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível atualizar a prateleira.")),
  });
  const shelfStatusMutation = useMutation({
    mutationFn: ({ rackId, shelfId, status }: { rackId: string; shelfId: string; status: LocationStatus }) =>
      setShelfStatus(getSupabaseBrowserClient(), rackId, shelfId, status),
    onSuccess: refresh,
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível alterar a situação da prateleira.")),
  });

  async function submitRack(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const formElement = event.currentTarget;
    const form = new FormData(formElement);
    try {
      await createRackMutation.mutateAsync({
        code: String(form.get("code") ?? "").trim(),
        description: String(form.get("description") ?? "").trim() || null,
      });
      formElement.reset();
      setRackCreateError(null);
      setRackDialogOpen(false);
    } catch {
      // O erro da mutação é exibido dentro do modal.
    }
  }

  async function submitShelf(event: FormEvent<HTMLFormElement>, rackId: string): Promise<void> {
    event.preventDefault();
    const formElement = event.currentTarget;
    const form = new FormData(formElement);
    const code = String(form.get("shelf-code") ?? "").trim();
    if (!code) {
      setShelfCreateError("Informe o código da prateleira.");
      return;
    }

    setShelfCreateError(null);
    try {
      await createShelfMutation.mutateAsync({ rackId, code });
      formElement.reset();
    } catch {
      // O erro da mutação é exibido junto ao formulário.
    }
  }

  const selectedRack = racksQuery.data?.find((rack) => rack.id === selectedRackId);

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-7xl space-y-space-xl p-space-md text-on-surface md:p-space-xl">
        <header className="flex flex-wrap items-center justify-between gap-space-md">
          <div><p className="mb-space-xs text-label-sm uppercase tracking-wider text-on-surface-variant">Localizações de estoque</p><h1 className="font-headline text-headline-lg-mobile md:text-headline-lg">Estantes e prateleiras</h1>
          <p className="mt-1 text-body-md text-on-surface-variant">Organize as localizações usadas no cadastro de produtos.</p>
          </div>
          <button
            type="button"
            onClick={() => {
              setRackCreateError(null);
              setRackDialogOpen(true);
            }}
            className="rounded-lg bg-primary-container px-space-md py-2.5 text-label-md text-on-primary hover:bg-primary"
          >
            + Cadastrar Nova Estante
          </button>
        </header>

        {errorMessage && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
        {racksQuery.isLoading && <p role="status" className="rounded-xl bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">Carregando estantes...</p>}
        {racksQuery.isError && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">Não foi possível carregar as localizações.</p>}
        {racksQuery.data && racksQuery.data.length === 0 && <p className="rounded-xl bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">Nenhuma estante cadastrada.</p>}

        {racksQuery.data && racksQuery.data.length > 0 && (
          <>
            <div className="rounded-xl bg-surface-container-lowest p-space-md shadow-sm ring-1 ring-outline-variant/50">
              <label htmlFor="rack-selector" className="mb-space-xs block text-label-sm text-on-surface-variant">Estante selecionada</label>
              <select
                id="rack-selector"
                value={selectedRackId}
                onChange={(event) => {
                  setShelfCreateError(null);
                  setSelectedRackId(event.target.value);
                }}
                className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md sm:max-w-sm"
              >
                {racksQuery.data.map((rack) => (
                  <option key={rack.id} value={rack.id} disabled={rack.status !== "Active"}>
                    {rack.code}{rack.status === "Inactive" ? " (inativa)" : ""}
                  </option>
                ))}
              </select>
            </div>

            {selectedRack && (
              <section aria-label={`Prateleiras da estante ${selectedRack.code}`} className="space-y-space-md rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/50">
                <header className="flex flex-wrap items-center justify-between gap-space-sm"><div><h2 className="font-headline text-headline-sm">Prateleiras da Estante {selectedRack.code}</h2><p className="text-body-sm text-on-surface-variant">Níveis de armazenamento vinculados a esta estante.</p></div><span className="rounded-full bg-secondary-container px-space-sm py-1 text-label-sm">{selectedRack.shelves.length} cadastrada(s)</span></header>
                <form
                  key={selectedRack.id}
                  aria-label={`Editar estante ${selectedRack.code}`}
                  className="grid gap-space-md rounded-xl bg-surface-container-low p-space-md sm:grid-cols-3"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    const form = new FormData(event.currentTarget);
                    await updateRackMutation.mutateAsync({
                      rackId: selectedRack.id,
                      code: String(form.get("rack-code") ?? "").trim(),
                      description: String(form.get("rack-description") ?? "").trim() || null,
                    });
                  }}
                >
                  <div>
                    <label htmlFor="edit-rack-code" className="mb-space-xs block text-label-sm text-on-surface-variant">Código</label>
                    <input id="edit-rack-code" name="rack-code" required defaultValue={selectedRack.code} className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
                  </div>
                  <div>
                    <label htmlFor="edit-rack-description" className="mb-space-xs block text-label-sm text-on-surface-variant">Descrição</label>
                    <input id="edit-rack-description" name="rack-description" defaultValue={selectedRack.description ?? ""} className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
                  </div>
                  <div className="flex items-end gap-2">
                    <button type="submit" className="rounded-lg bg-secondary-container px-space-md py-2 text-label-md text-on-secondary-fixed">Salvar estante</button>
                    <button
                      type="button"
                      onClick={() => rackStatusMutation.mutate({
                        rackId: selectedRack.id,
                        status: selectedRack.status === "Active" ? "Inactive" : "Active",
                      })}
                      className="rounded-lg bg-secondary-container px-space-md py-2 text-label-md text-on-secondary-fixed"
                    >
                      {selectedRack.status === "Active" ? "Inativar" : "Reativar"} estante
                    </button>
                  </div>
                </form>

                {selectedRack.shelves.length === 0 && <p>Esta estante ainda não tem prateleiras.</p>}
                {selectedRack.status === "Active" && (
                  <form
                    key={`create-shelf-${selectedRack.id}`}
                    aria-label="Cadastrar prateleira"
                    className="flex flex-wrap items-end gap-space-sm rounded-xl bg-surface-container-low p-space-md"
                    onSubmit={(event) => submitShelf(event, selectedRack.id)}
                  >
                    <div>
                      <label htmlFor="create-shelf-code" className="mb-space-xs block text-label-sm text-on-surface-variant">Código da prateleira</label>
                      <input
                        id="create-shelf-code"
                        name="shelf-code"
                        required
                        disabled={createShelfMutation.isPending}
                        onChange={() => setShelfCreateError(null)}
                        className="rounded-lg border border-outline-variant bg-surface-container-lowest px-space-md py-2.5 text-body-md"
                      />
                    </div>
                    <button
                      type="submit"
                      disabled={createShelfMutation.isPending}
                      className="rounded-lg bg-primary-container px-space-md py-2.5 text-label-md text-on-primary hover:bg-primary disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      {createShelfMutation.isPending ? "Cadastrando..." : "Cadastrar prateleira"}
                    </button>
                    {shelfCreateError && <p role="alert" className="w-full rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{shelfCreateError}</p>}
                  </form>
                )}
                <ul className="space-y-3">
                  {selectedRack.shelves.map((shelf) => (
                    <li key={shelf.id} className="flex flex-wrap items-end gap-space-md rounded-xl bg-surface-container-low p-space-md">
                      <form
                        aria-label={`Editar prateleira ${shelf.code}`}
                        className="flex flex-wrap items-end gap-space-sm"
                        onSubmit={async (event) => {
                          event.preventDefault();
                          const form = new FormData(event.currentTarget);
                          await updateShelfMutation.mutateAsync({
                            rackId: selectedRack.id,
                            shelfId: shelf.id,
                            code: String(form.get("shelf-code") ?? "").trim(),
                          });
                        }}
                      >
                        <div>
                          <label htmlFor={`edit-shelf-${shelf.id}`} className="mb-space-xs block text-label-sm text-on-surface-variant">Prateleira</label>
                          <input id={`edit-shelf-${shelf.id}`} name="shelf-code" required defaultValue={shelf.code} className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
                        </div>
                        <button type="submit" className="rounded-lg bg-secondary-container px-space-md py-2 text-label-md text-on-secondary-fixed">Salvar</button>
                      </form>
                      <span className="text-sm">{shelf.status === "Active" ? "Ativa" : "Inativa"}</span>
                      <Link
                        href={`/locations/${selectedRack.id}/shelves/${shelf.id}`}
                        className="rounded-lg bg-secondary-container px-space-md py-2 text-label-md text-on-secondary-fixed underline"
                      >
                        Ver produtos
                      </Link>
                      <button
                        type="button"
                        onClick={() => shelfStatusMutation.mutate({
                          rackId: selectedRack.id,
                          shelfId: shelf.id,
                          status: shelf.status === "Active" ? "Inactive" : "Active",
                        })}
                        className="rounded-lg bg-secondary-container px-space-md py-2 text-label-md text-on-secondary-fixed"
                      >
                        {shelf.status === "Active" ? "Inativar" : "Reativar"}
                      </button>
                    </li>
                  ))}
                </ul>
              </section>
            )}
          </>
        )}
      </main>
      <Dialog
        open={rackDialogOpen}
        title="Cadastrar Nova Estante"
        onClose={() => {
          setRackDialogOpen(false);
          setRackCreateError(null);
        }}
      >
        <p className="mb-space-lg text-body-sm text-on-surface-variant">Crie uma nova localização para organizar o estoque.</p>
        {rackCreateError && <p role="alert" className="mb-space-md rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{rackCreateError}</p>}
        <form onSubmit={submitRack} className="grid gap-space-md sm:grid-cols-2" aria-label="Cadastrar estante">
          <div>
            <label htmlFor="rack-code" className="mb-space-xs block text-label-sm text-on-surface-variant">Código da estante</label>
            <input id="rack-code" name="code" required className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
          </div>
          <div>
            <label htmlFor="rack-description" className="mb-space-xs block text-label-sm text-on-surface-variant">Descrição</label>
            <input id="rack-description" name="description" className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
          </div>
          <div className="flex justify-end gap-space-sm sm:col-span-2">
            <button type="button" onClick={() => {
              setRackDialogOpen(false);
              setRackCreateError(null);
            }} className="rounded-lg bg-secondary-container px-space-md py-2.5 text-label-md text-on-secondary-fixed">
              Cancelar
            </button>
            <button type="submit" disabled={createRackMutation.isPending} className="rounded-lg bg-primary-container px-space-md py-2.5 text-label-md text-on-primary disabled:opacity-50">
              Cadastrar estante
            </button>
          </div>
        </form>
      </Dialog>
    </>
  );
}
