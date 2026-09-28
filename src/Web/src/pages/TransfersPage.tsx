import { Link } from "react-router-dom";
import { useBanking } from "../banking";
import { label, money, when } from "../format";

export function TransfersPage() {
  const { transfers, nameOf, pathOf } = useBanking();

  return (
    <>
      <header className="page-head">
        <div>
          <p className="eyebrow">Movimentações</p>
          <h1>Transferências</h1>
        </div>
        <div className="actions">
          <Link className="button" to="/transferencias/nova">Transferir agora</Link>
          <Link className="button ghost" to="/transferencias/agendar">Agendar</Link>
        </div>
      </header>
      <section className="panel">
        {transfers.length === 0 && <p className="hint">Nenhuma transferência registrada.</p>}
        {transfers.length > 0 && (
          <table>
            <thead>
              <tr><th>Quando</th><th>Origem</th><th>Destino</th><th>Valor</th><th>Status</th></tr>
            </thead>
            <tbody>
              {transfers.map((transfer) => (
                <tr key={transfer.id}>
                  <td>{when(transfer.createdAtUtc)}</td>
                  <td><Link to={pathOf(transfer.sourceAccountId)}>{nameOf(transfer.sourceAccountId)}</Link></td>
                  <td><Link to={pathOf(transfer.destinationAccountId)}>{nameOf(transfer.destinationAccountId)}</Link></td>
                  <td>{money.format(transfer.amount)}</td>
                  <td>
                    <Link to={`/transferencias/${transfer.id}`}><em className={transfer.status}>{label(transfer.status)}</em></Link>
                    {transfer.failureReason && <small>{transfer.failureReason}</small>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </>
  );
}
