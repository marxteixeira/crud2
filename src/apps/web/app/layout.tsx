import type { Metadata } from "next";
import { Manrope, Plus_Jakarta_Sans } from "next/font/google";
import type { ReactNode } from "react";
import Providers from "./providers";
import "./globals.css";

const jakarta = Plus_Jakarta_Sans({ subsets: ["latin"], display: "swap", variable: "--font-jakarta" });
const manrope = Manrope({ subsets: ["latin"], display: "swap", variable: "--font-manrope" });

export const metadata: Metadata = {
  title: "Sistema de Estoque",
  description: "Controle de Produtos e Fornecedores",
};

export default function RootLayout({ children }: { children: ReactNode }): JSX.Element {
  return (
    <html lang="pt-BR" className={`${jakarta.variable} ${manrope.variable}`}>
      <body><Providers>{children}</Providers></body>
    </html>
  );
}
