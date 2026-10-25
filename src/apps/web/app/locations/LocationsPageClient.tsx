"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
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
} from "@/lib/locations";

export default function LocationsPageClient(): JSX.Element {
  const queryClient = useQueryClient();
  const [selectedRackId, setSelectedRackId] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
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
      setSelectedRackId(rack.id);
      await refresh();
    },
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível cadastrar a estante. Verifique o código e tente novamente.")),
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
    onSuccess: refresh,
    onError: (error) => setErrorMessage(describeApiError(error, "Não foi possível cadastrar a prateleira.")),
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
    } catch {
      // O erro já é exibido pelo onError da mutação.
    }
  }

  async function submitShelf(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    const formElement = event.currentTarget;
    const form = new FormData(formElement);
    const code = String(form.get("shelf-code") ?? "").trim();
    if (selectedRackId && code) {
      try {
        await createShelfMutation.mutateAsync({ rackId: selectedRackId, code });
        formElement.reset();
      } catch {
        // O erro já é exibido pelo onError da mutação.
      }
    }
  }

  const selectedRack = racksQuery.data?.find((rack) => rack.id === selectedRackId);

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-5xl space-y-6 p-6">
        <header>
          <h1 className="text-2xl font-bold">Estantes e prateleiras</h1>
          <p className="text-sm text-slate-600">Organize as localizações usadas no cadastro de produtos.</p>
        </header>

        <form onSubmit={submitRack} className="grid gap-3 rounded border p-4 sm:grid-cols-3" aria-label="Cadastrar estante">
          <div>
            <label htmlFor="rack-code" className="mb-1 block text-sm font-medium">Código da estante</label>
            <input id="rack-code" name="code" required className="w-full rounded border p-2" />
          </div>
          <div>
            <label htmlFor="rack-description" className="mb-1 block text-sm font-medium">Descrição</label>
            <input id="rack-description" name="description" className="w-full rounded border p-2" />
          </div>
          <button type="submit" disabled={createRackMutation.isPending} className="self-end rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50">
            Cadastrar estante
          </button>
        </form>

        {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
        {racksQuery.isLoading && <p role="status">Carregando estantes...</p>}
        {racksQuery.isError && <p role="alert" className="text-sm text-red-700">Não foi possível carregar as localizações.</p>}
        {racksQuery.data && racksQuery.data.length === 0 && <p>Nenhuma estante cadastrada.</p>}

        {racksQuery.data && racksQuery.data.length > 0 && (
          <>
            <div>
              <label htmlFor="rack-selector" className="mb-1 block text-sm font-medium">Estante selecionada</label>
              <select
                id="rack-selector"
                value={selectedRackId}
                onChange={(event) => setSelectedRackId(event.target.value)}
                className="w-full rounded border p-2 sm:max-w-sm"
              >
                {racksQuery.data.map((rack) => (
                  <option key={rack.id} value={rack.id} disabled={rack.status !== "Active"}>
                    {rack.code}{rack.status === "Inactive" ? " (inativa)" : ""}
                  </option>
                ))}
              </select>
            </div>

            {selectedRack && (
              <section aria-label={`Prateleiras da estante ${selectedRack.code}`} className="space-y-4 rounded border p-4">
                <form
                  aria-label={`Editar estante ${selectedRack.code}`}
                  className="grid gap-3 sm:grid-cols-3"
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
                    <label htmlFor="edit-rack-code" className="mb-1 block text-sm font-medium">Código</label>
                    <input id="edit-rack-code" name="rack-code" required defaultValue={selectedRack.code} className="w-full rounded border p-2" />
                  </div>
                  <div>
                    <label htmlFor="edit-rack-description" className="mb-1 block text-sm font-medium">Descrição</label>
                    <input id="edit-rack-description" name="rack-description" defaultValue={selectedRack.description ?? ""} className="w-full rounded border p-2" />
                  </div>
                  <div className="flex items-end gap-2">
                    <button type="submit" className="rounded border px-3 py-2">Salvar estante</button>
                    <button
                      type="button"
                      onClick={() => rackStatusMutation.mutate({
                        rackId: selectedRack.id,
                        status: selectedRack.status === "Active" ? "Inactive" : "Active",
                      })}
                      className="rounded border px-3 py-2"
                    >
                      {selectedRack.status === "Active" ? "Inativar" : "Reativar"} estante
                    </button>
                  </div>
                </form>

                <form onSubmit={submitShelf} className="flex flex-wrap items-end gap-3" aria-label="Cadastrar prateleira">
                  <div>
                    <label htmlFor="shelf-code" className="mb-1 block text-sm font-medium">Código da prateleira</label>
                    <input id="shelf-code" name="shelf-code" required className="rounded border p-2" />
                  </div>
                  <button type="submit" disabled={selectedRack.status !== "Active"} className="rounded border px-4 py-2 disabled:opacity-50">
                    Cadastrar prateleira
                  </button>
                </form>

                {selectedRack.shelves.length === 0 && <p>Esta estante ainda não tem prateleiras.</p>}
                <ul className="space-y-3">
                  {selectedRack.shelves.map((shelf) => (
                    <li key={shelf.id} className="flex flex-wrap items-end gap-2 rounded border p-3">
                      <form
                        aria-label={`Editar prateleira ${shelf.code}`}
                        className="flex flex-wrap items-end gap-2"
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
                          <label htmlFor={`edit-shelf-${shelf.id}`} className="mb-1 block text-sm font-medium">Prateleira</label>
                          <input id={`edit-shelf-${shelf.id}`} name="shelf-code" required defaultValue={shelf.code} className="rounded border p-2" />
                        </div>
                        <button type="submit" className="rounded border px-3 py-2">Salvar</button>
                      </form>
                      <span className="text-sm">{shelf.status === "Active" ? "Ativa" : "Inativa"}</span>
                      <Link
                        href={`/locations/${selectedRack.id}/shelves/${shelf.id}`}
                        className="rounded border px-3 py-2 underline"
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
                        className="rounded border px-3 py-2"
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
    </>
  );
}
