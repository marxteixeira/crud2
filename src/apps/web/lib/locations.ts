import type { SupabaseClient } from "@supabase/supabase-js";
import { requestWithSession } from "@/lib/api";

export type LocationStatus = "Active" | "Inactive";

export interface Shelf {
  id: string;
  rackId: string;
  code: string;
  status: LocationStatus;
}

export interface Rack {
  id: string;
  code: string;
  description: string | null;
  status: LocationStatus;
  shelves: Shelf[];
}

export interface RackInput {
  code: string;
  description?: string | null;
}

export interface ShelfInput {
  code: string;
}

export async function listRacks(
  client: SupabaseClient,
  status?: LocationStatus,
): Promise<Rack[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : "";
  return requestWithSession<Rack[]>(client, "GET", `/api/locations/racks${query}`);
}

export function getRack(client: SupabaseClient, rackId: string): Promise<Rack> {
  return requestWithSession<Rack>(client, "GET", `/api/locations/racks/${rackId}`);
}

export function createRack(client: SupabaseClient, input: RackInput): Promise<Rack> {
  return requestWithSession<Rack>(client, "POST", "/api/locations/racks", input);
}

export function updateRack(
  client: SupabaseClient,
  rackId: string,
  input: RackInput,
): Promise<Rack> {
  return requestWithSession<Rack>(client, "PATCH", `/api/locations/racks/${rackId}`, input);
}

export function setRackStatus(
  client: SupabaseClient,
  rackId: string,
  status: LocationStatus,
): Promise<void> {
  return requestWithSession<void>(client, "PUT", `/api/locations/racks/${rackId}/status`, { status });
}

export function listShelves(
  client: SupabaseClient,
  rackId: string,
  status?: LocationStatus,
): Promise<Shelf[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : "";
  return requestWithSession<Shelf[]>(client, "GET", `/api/locations/racks/${rackId}/shelves${query}`);
}

export function createShelf(
  client: SupabaseClient,
  rackId: string,
  input: ShelfInput,
): Promise<Shelf> {
  return requestWithSession<Shelf>(client, "POST", `/api/locations/racks/${rackId}/shelves`, input);
}

export function updateShelf(
  client: SupabaseClient,
  rackId: string,
  shelfId: string,
  input: ShelfInput,
): Promise<Shelf> {
  return requestWithSession<Shelf>(
    client,
    "PATCH",
    `/api/locations/racks/${rackId}/shelves/${shelfId}`,
    input,
  );
}

export function setShelfStatus(
  client: SupabaseClient,
  rackId: string,
  shelfId: string,
  status: LocationStatus,
): Promise<void> {
  return requestWithSession<void>(
    client,
    "PUT",
    `/api/locations/racks/${rackId}/shelves/${shelfId}/status`,
    { status },
  );
}
