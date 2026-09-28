import { FormEvent, useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { createTransfer } from "../api";
import { useBanking } from "../banking";
import { money } from "../format";

export function TransferFormPage({ scheduled }: { scheduled: boolean }) {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const { accounts, refresh, setError, setNotice } = useBanking();
  const [sourceAccountId, setSourceAccountId] = useState(params.get("origem") ?? accounts[0]?.id ?? "");
  const [destinationAccountId, setDestinationAccountId] = useState(accounts.find((account) => account.id !== (params.get("origem") ?? accounts[0]?.id))?.id ?? "");
  const [amount, setAmount] = useState("100");
  const [scheduledAt, setScheduledAt] = useState("");

  useEffect(() => {
    if (accounts.length === 0) return;
    setSourceAccountId((current) => current || params.get("origem") || accounts[0].id);
    setDestinationAccountId((current) => current || accounts.find((account) => account.id !== (params.get("origem") || accounts[0].id))?.id || accounts[0].id);
  }, [accounts, params]);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError("");
    setNotice("");
    try {
      const result = await createTransfer({
        sourceAccountId,
        destinationAccountId,
        amount: Number(amount),
        scheduledAtUtc: scheduled ? new Date(scheduledAt).toISOString() : undefined
      });
      await refresh();
      if (result.ok) {
        setNotice(scheduled ? "Transferência agendada." : "Transferência concluída.");
      } else {
        setError(result.transfer.failureReason ?? "Transferência rejeitada.");
      }
      if (result.transfer.id) navigate(`/transferencias/${result.transfer.id}`);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao transferir.");
    }
  }

  const source = accounts.find((account) => account.id === sourceAccountId);

  return (
    <div className="sheet">
      <header className="page-head">
        <div>
          <p className="eyebrow">{scheduled ? "Agendamento" : "Imediata"}</p>
          <h1>{scheduled ? "Agendar transferência" : "Transferir agora"}</h1>
        </div>
      </header>
      <form className="panel stack narrow" onSubmit={submit}>
        <label>
          Origem
          <select value={sourceAccountId} onChange={(event) => setSourceAccountId(event.target.value)}>
            {accounts.map((account) => <option key={account.id} value={account.id}>{account.holderName}</option>)}
          </select>
        </label>
        <label>
          Destino
          <select value={destinationAccountId} onChange={(event) => setDestinationAccountId(event.target.value)}>
            {accounts.map((account) => <option key={account.id} value={account.id}>{account.holderName}</option>)}
          </select>
        </label>
        <label>
          Valor (R$)
          <input value={amount} onChange={(event) => setAmount(event.target.value)} type="number" min="0.01" step="0.01" required />
        </label>
        {scheduled && (
          <label>
            Data e hora
            <input value={scheduledAt} onChange={(event) => setScheduledAt(event.target.value)} type="datetime-local" required />
          </label>
        )}
        {source && (
          <p className="hint">
            Dia: até {money.format(source.dayMaxAmountPerHour)} e {source.dayMaxAttemptsPerHour} tentativas por hora.
            Noite (20h–6h, horário de Brasília): até {money.format(source.nightMaxAmountPerHour)} e {source.nightMaxAttemptsPerHour} tentativas.
          </p>
        )}
        <button type="submit">{scheduled ? "Agendar" : "Transferir"}</button>
      </form>
    </div>
  );
}
