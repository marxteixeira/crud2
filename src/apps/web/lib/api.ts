import axios from "axios";

export interface HealthResponse {
  status: string;
  timestamp: string;
}

const apiUrl: string = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5013";

const http = axios.create({ baseURL: apiUrl, timeout: 5000 });

export async function getHealth(): Promise<HealthResponse> {
  const response = await http.get<HealthResponse>("/health");
  return response.data;
}
