import type { ReactNode } from "react";

export default function AuthLayout({ children }: { children: ReactNode }): JSX.Element {
  return (
    <main className="min-h-screen bg-surface px-space-md py-space-md text-on-surface sm:px-space-lg sm:py-space-lg">
      <header className="mx-auto flex w-full max-w-[1440px] items-center gap-space-sm px-space-sm py-space-sm">
        <span aria-hidden="true" className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-container text-label-md text-on-primary">S</span>
        <span className="flex flex-col">
          <span className="font-title text-title-md text-primary">StockSystem</span>
          <span className="text-caption text-on-surface-variant">Sistema de Estoque</span>
        </span>
      </header>
      <div className="mx-auto mt-8 grid w-full max-w-[1440px] overflow-hidden rounded-xl bg-surface-container-lowest shadow-xl ring-1 ring-outline-variant/40 lg:mt-12 lg:grid-cols-[1.15fr_0.85fr]">
        <aside className="flex flex-col justify-between gap-space-xl bg-gradient-to-br from-[#00563a] to-[#003d4a] p-space-lg text-white sm:p-space-xl lg:p-12">
          <div>
            <span className="inline-flex rounded-full bg-black/20 px-space-md py-2 text-label-sm text-[#9bf2ca]">GESTÃO DE ESTOQUE</span>
            <p className="mt-8 text-label-sm uppercase tracking-[0.16em] text-[#8ce0bb]">Logística &amp; operação</p>
            <h2 className="mt-3 max-w-2xl font-headline text-headline-lg-mobile leading-tight text-white sm:text-4xl">
              A inteligência e o controle que seu estoque precisa.
            </h2>
            <p className="mt-4 max-w-xl text-body-lg text-white/80">
              Acompanhe produtos, localizações e fornecedores em um só lugar, com informações organizadas para o dia a dia da operação.
            </p>
          </div>
          <div className="grid gap-space-md sm:grid-cols-2">
            <div className="rounded-xl bg-black/20 p-space-lg">
              <span aria-hidden="true" className="flex h-10 w-10 items-center justify-center rounded-lg bg-white/10 text-xl">01</span>
              <h3 className="mt-4 font-title text-title-md text-white">Visão centralizada</h3>
              <p className="mt-1 text-body-sm text-white/75">Produtos e locais organizados para facilitar a consulta.</p>
            </div>
            <div className="rounded-xl bg-black/20 p-space-lg">
              <span aria-hidden="true" className="flex h-10 w-10 items-center justify-center rounded-lg bg-white/10 text-xl">02</span>
              <h3 className="mt-4 font-title text-title-md text-white">Gestão da equipe</h3>
              <p className="mt-1 text-body-sm text-white/75">Acesso de cada pessoa conforme seu perfil.</p>
            </div>
          </div>
          <p className="border-t border-white/15 pt-space-md text-body-sm text-white/70">StockSystem | Gestão de estoque e operações</p>
        </aside>
        <section className="flex items-center px-space-lg py-space-xl sm:px-12 lg:px-10 lg:py-12">
          <div className="mx-auto w-full max-w-lg">{children}</div>
        </section>
      </div>
    </main>
  );
}
