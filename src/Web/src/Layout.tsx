import { useEffect, useState } from "react";
import { Link, NavLink, Outlet, useLocation } from "react-router-dom";
import { useBanking } from "./banking";

const selectedAccountKey = "bth-selected-account";

export function Layout() {
  const { accounts, error, notice, setError, setNotice, accountOf } = useBanking();
  const { pathname, search } = useLocation();
  const accountIdFromRoute = pathname.match(/^\/contas\/(?!nova$)([^/]+)$/)?.[1] ?? "";
  const onAccountDetail = accountIdFromRoute.length > 0;
  const origem = new URLSearchParams(search).get("origem") ?? "";
  const [selectedId, setSelectedId] = useState(() => sessionStorage.getItem(selectedAccountKey) ?? "");
  const adminActive = pathname === "/contas/nova" || pathname.startsWith("/transferencias") || pathname.startsWith("/administrador");

  useEffect(() => {
    const match = accountOf(accountIdFromRoute) ?? accountOf(origem);
    if (!match) return;
    setSelectedId(match.id);
    sessionStorage.setItem(selectedAccountKey, match.id);
  }, [accountIdFromRoute, origem, accountOf]);

  const currentAccount = accounts.find((account) => account.id === selectedId);

  return (
    <div className="app">
      <header className="nav">
        <div className="nav-inner">
          <Link to="/contas" className="brand">
            <span>Brasil Terrenos</span>
            <strong>Transferências</strong>
          </Link>
          <nav>
            <NavLink to="/contas" end className={onAccountDetail ? "active" : undefined}>
              <AccountsIcon />
              Contas
            </NavLink>
            <div className="menu">
              <span className={adminActive ? "menu-label active" : "menu-label"}>
                <AdminIcon />
                Administrador
              </span>
              <div className="menu-list" role="menu">
                <div className="menu-panel">
                  <NavLink to="/contas/nova" role="menuitem">Nova conta</NavLink>
                  <NavLink to="/administrador/cheque-especial" role="menuitem">Cheque especial</NavLink>
                  <NavLink to="/transferencias" end role="menuitem">Movimentações</NavLink>
                  <NavLink to="/transferencias/nova" role="menuitem">Transferir</NavLink>
                  <NavLink to="/transferencias/agendar" role="menuitem">Agendar</NavLink>
                </div>
              </div>
            </div>
          </nav>
          <p className="current-account">
            <span>Conta atual</span>
            <strong>{currentAccount ? currentAccount.holderName : "Nenhuma selecionada"}</strong>
          </p>
        </div>
      </header>
      <main>
        {error && (
          <p className="banner error" role="alert">
            <span>{error}</span>
            <button type="button" className="close" onClick={() => setError("")} aria-label="Fechar aviso">×</button>
          </p>
        )}
        {notice && !error && (
          <p className="banner ok" role="status">
            <span>{notice}</span>
            <button type="button" className="close" onClick={() => setNotice("")} aria-label="Fechar aviso">×</button>
          </p>
        )}
        <Outlet />
      </main>
    </div>
  );
}

function AccountsIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="3" y="6" width="18" height="13" rx="2.5" />
      <path d="M3 10.5h18" />
      <path d="M7 15h5" />
    </svg>
  );
}

function AdminIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="12" cy="8" r="3.2" />
      <path d="M5.2 19.2c1.3-3.1 3.7-4.6 6.8-4.6s5.5 1.5 6.8 4.6" />
    </svg>
  );
}
