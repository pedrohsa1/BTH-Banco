import { FormEvent, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { changeAccountStatus } from "../api";
import { useBanking } from "../banking";
import { label, money, when } from "../format";
import { LimitsModal } from "../LimitsModal";
import { TransferModal } from "../TransferModal";

export function AccountPage() {
  const { id = "" } = useParams();
  const { transfers, refresh, setError, setNotice, accountOf } = useBanking();
  const account = accountOf(id);
  const movements = transfers.filter(
    (transfer) => account !== undefined && (transfer.sourceAccountId === account.id || transfer.destinationAccountId === account.id)
  );
  const [status, setStatus] = useState(account?.status ?? "Active");
  const [modal, setModal] = useState<"now" | "later" | null>(null);
  const [limitsOpen, setLimitsOpen] = useState(false);

  useEffect(() => {
    if (account) setStatus(account.status);
  }, [account]);

  async function saveStatus(event: FormEvent) {
    event.preventDefault();
    setError("");
    setNotice("");
    if (!account) return;
    try {
      await changeAccountStatus(account.id, status);
      await refresh();
      setNotice("Status atualizado.");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao alterar o status.");
    }
  }

  if (!account) {
    return <p className="panel sheet">Conta não encontrada.</p>;
  }

  return (
    <div className="account-page">
      <section className="cover">
        <div className="cover-top">
          <div className="cover-identity">
            <div className="title-row">
              <h1>{account.holderName}</h1>
              <em className={account.status}>{label(account.status)}</em>
            </div>
            <p className="account-line">
              <span>Conta</span>
              <strong>{account.document}</strong>
            </p>
          </div>
          <div className="actions">
            <button type="button" className="button" onClick={() => setModal("now")}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12h14" /><path d="M13 6l6 6-6 6" /></svg>
              Transferir
            </button>
            <button type="button" className="button ghost" onClick={() => setModal("later")}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="4" y="5" width="16" height="15" rx="2" /><path d="M8 3.5v3M16 3.5v3M4 10h16" /></svg>
              Agendar
            </button>
          </div>
        </div>
        <div className="cover-stats">
          <article>
            <span>Saldo</span>
            <strong className={account.balance < 0 ? "debt" : "credit"}>{money.format(account.balance)}</strong>
          </article>
          <article>
            <span>Cheque especial</span>
            <strong>{money.format(account.overdraftLimit)}</strong>
          </article>
          <article>
            <span>Disponível</span>
            <strong>{money.format(account.availableFunds)}</strong>
          </article>
        </div>
      </section>
      <TransferModal
        open={modal !== null}
        scheduled={modal === "later"}
        sourceAccountId={account.id}
        onClose={() => setModal(null)}
      />

      <LimitsModal open={limitsOpen} account={account} onClose={() => setLimitsOpen(false)} />

      <section className="grid">
        <div className="panel">
          <div className="panel-head">
            <h2>Limites por hora</h2>
            <button type="button" className="icon" aria-label="Configurar limites" onClick={() => setLimitsOpen(true)}>
              <GearIcon />
            </button>
          </div>
          <dl>
            <div><dt>Dia, valor</dt><dd>{money.format(account.dayMaxAmountPerHour)}</dd></div>
            <div><dt>Dia, tentativas</dt><dd>{account.dayMaxAttemptsPerHour}</dd></div>
            <div><dt>Noite, valor</dt><dd>{money.format(account.nightMaxAmountPerHour)}</dd></div>
            <div><dt>Noite, tentativas</dt><dd>{account.nightMaxAttemptsPerHour}</dd></div>
          </dl>
          <p className="hint">Noite (20h–6h), horário de Brasília</p>
        </div>
        <form className="panel stack" onSubmit={saveStatus}>
          <h2>Situação da conta</h2>
          <label>
            Status
            <select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="Active">Ativa</option>
              <option value="Inactive">Inativa</option>
            </select>
          </label>
          <p className="hint">Conta inativa não envia nem recebe.</p>
          <button type="submit">Salvar status</button>
        </form>
      </section>

      <section className="panel">
        <h2>Movimentações desta conta</h2>
        <TransferTable rows={movements} />
      </section>
    </div>
  );
}

function GearIcon() {
  return (
    <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
      <path
        fill="currentColor"
        d="M19.4 13.5a7.7 7.7 0 0 0 .1-1.5 7.7 7.7 0 0 0-.1-1.5l2-1.6-2-3.4-2.4 1a7.4 7.4 0 0 0-2.6-1.5L14 2h-4l-.4 2.5a7.4 7.4 0 0 0-2.6 1.5l-2.4-1-2 3.4 2 1.6a7.7 7.7 0 0 0-.1 1.5 7.7 7.7 0 0 0 .1 1.5l-2 1.6 2 3.4 2.4-1a7.4 7.4 0 0 0 2.6 1.5L10 22h4l.4-2.5a7.4 7.4 0 0 0 2.6-1.5l2.4 1 2-3.4-2-1.6zM12 15.5A3.5 3.5 0 1 1 15.5 12 3.5 3.5 0 0 1 12 15.5z"
      />
    </svg>
  );
}

function TransferTable({ rows }: { rows: { id: string; sourceAccountId: string; destinationAccountId: string; amount: number; status: string; createdAtUtc: string; failureReason: string | null }[] }) {
  const { nameOf } = useBanking();
  if (rows.length === 0) return <p className="hint">Nenhuma movimentação ainda.</p>;

  return (
    <table>
      <thead>
        <tr><th>Quando</th><th>Origem</th><th>Destino</th><th>Valor</th><th>Status</th></tr>
      </thead>
      <tbody>
        {rows.map((transfer) => (
          <tr key={transfer.id}>
            <td>{when(transfer.createdAtUtc)}</td>
            <td>{nameOf(transfer.sourceAccountId)}</td>
            <td>{nameOf(transfer.destinationAccountId)}</td>
            <td>{money.format(transfer.amount)}</td>
            <td>
              <Link to={`/transferencias/${transfer.id}`}><em className={transfer.status}>{label(transfer.status)}</em></Link>
              {transfer.failureReason && <small>{transfer.failureReason}</small>}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
