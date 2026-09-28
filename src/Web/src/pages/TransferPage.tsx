import { Link, useParams } from "react-router-dom";
import { cancelTransfer } from "../api";
import { useBanking } from "../banking";
import { label, money, when } from "../format";

export function TransferPage() {
  const { id = "" } = useParams();
  const { transfers, nameOf, pathOf, refresh, setError, setNotice } = useBanking();
  const transfer = transfers.find((item) => item.id === id);

  async function cancel() {
    setError("");
    setNotice("");
    try {
      await cancelTransfer(id);
      await refresh();
      setNotice("Agendamento cancelado.");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao cancelar.");
    }
  }

  if (!transfer) return <p className="panel sheet">Transferência não encontrada.</p>;

  return (
    <div className="sheet">
      <header className="page-head">
        <div>
          <p className="eyebrow">Transferência</p>
          <div className="title-row">
            <h1>{money.format(transfer.amount)}</h1>
            <em className={transfer.status}>{label(transfer.status)}</em>
          </div>
        </div>
      </header>
      <section className="panel">
        <dl>
          <div><dt>Origem</dt><dd><Link to={pathOf(transfer.sourceAccountId)}>{nameOf(transfer.sourceAccountId)}</Link></dd></div>
          <div><dt>Destino</dt><dd><Link to={pathOf(transfer.destinationAccountId)}>{nameOf(transfer.destinationAccountId)}</Link></dd></div>
          <div><dt>Criada em</dt><dd>{when(transfer.createdAtUtc)}</dd></div>
          <div><dt>Agendada para</dt><dd>{when(transfer.scheduledAtUtc)}</dd></div>
          <div><dt>Processada em</dt><dd>{when(transfer.processedAtUtc)}</dd></div>
          <div><dt>Motivo</dt><dd>{transfer.failureReason ?? "—"}</dd></div>
        </dl>
        {transfer.status === "Scheduled" && <button className="danger" type="button" onClick={cancel}>Cancelar agendamento</button>}
      </section>
    </div>
  );
}
