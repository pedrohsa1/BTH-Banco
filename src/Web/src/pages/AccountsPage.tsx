import { Link } from "react-router-dom";
import { useBanking } from "../banking";
import { label, money } from "../format";

export function AccountsPage() {
  const { accounts } = useBanking();

  return (
    <>
      <header className="page-head">
        <div>
          <p className="eyebrow">Contas</p>
          <h1>Pessoas e saldos</h1>
        </div>
        <Link className="button" to="/contas/nova">Abrir conta</Link>
      </header>
      <section className="cards">
        {accounts.map((account) => (
          <Link className="card" key={account.id} to={`/contas/${account.document}`}>
            <div className="card-top">
              <span className="card-person">
                <span className="card-mark" aria-hidden="true">
                  <svg viewBox="0 0 24 24">
                    <rect x="3" y="6" width="18" height="13" rx="2.5" />
                    <path d="M3 10.5h18" />
                    <path d="M7 15h5" />
                  </svg>
                </span>
                <strong>{account.holderName}</strong>
              </span>
              <em className={account.status}>{label(account.status)}</em>
            </div>
            <span className="account-number">{account.document}</span>
            <b>{money.format(account.balance)}</b>
            <p className="meta">
              <span>Disponível {money.format(account.availableFunds)}</span>
              <span>Cheque especial {money.format(account.overdraftLimit)}</span>
            </p>
          </Link>
        ))}
      </section>
    </>
  );
}
