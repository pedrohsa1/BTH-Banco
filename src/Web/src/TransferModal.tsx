import { FormEvent, useEffect, useState } from "react";
import { createTransfer } from "./api";
import { useBanking } from "./banking";
import { money } from "./format";

type TransferModalProps = {
  open: boolean;
  scheduled: boolean;
  sourceAccountId: string;
  onClose: () => void;
};

export function TransferModal({ open, scheduled, sourceAccountId, onClose }: TransferModalProps) {
  const { accounts, refresh, setNotice } = useBanking();
  const [destinationAccountId, setDestinationAccountId] = useState("");
  const [amount, setAmount] = useState("100");
  const [scheduledAt, setScheduledAt] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (!open) return;
    const other = accounts.find((account) => account.id !== sourceAccountId);
    setDestinationAccountId(other?.id ?? "");
    setAmount("100");
    setScheduledAt("");
    setError("");
  }, [open, sourceAccountId]);

  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open) return null;

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError("");
    try {
      const result = await createTransfer({
        sourceAccountId,
        destinationAccountId,
        amount: Number(amount),
        scheduledAtUtc: scheduled ? new Date(scheduledAt).toISOString() : undefined
      });
      await refresh();
      if (!result.ok) {
        setError(result.transfer.failureReason ?? "Transferência rejeitada.");
        return;
      }
      setNotice(scheduled ? "Transferência agendada." : "Transferência concluída.");
      onClose();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao transferir.");
    }
  }

  const source = accounts.find((account) => account.id === sourceAccountId);

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <form className="modal" onClick={(event) => event.stopPropagation()} onSubmit={submit}>
        <header>
          <h2>{scheduled ? "Agendar" : "Transferir"}</h2>
          {source && <p className="lede">De {source.holderName}</p>}
        </header>
        {error && <p className="banner error">{error}</p>}
        <label>
          Para
          <select value={destinationAccountId} onChange={(event) => setDestinationAccountId(event.target.value)} required>
            {accounts.filter((account) => account.id !== sourceAccountId).map((account) => (
              <option key={account.id} value={account.id}>{account.holderName}</option>
            ))}
          </select>
        </label>
        <label>
          Valor (R$)
          <input value={amount} onChange={(event) => setAmount(event.target.value)} type="number" min="0.01" step="0.01" required />
        </label>
        {scheduled && (
          <label>
            Quando
            <input value={scheduledAt} onChange={(event) => setScheduledAt(event.target.value)} type="datetime-local" required />
          </label>
        )}
        {source && (
          <ul className="limit-note">
            <li><span>Dia</span><strong>até {money.format(source.dayMaxAmountPerHour)} e {source.dayMaxAttemptsPerHour} tentativas</strong></li>
            <li><span>Noite</span><strong>até {money.format(source.nightMaxAmountPerHour)} e {source.nightMaxAttemptsPerHour} tentativas</strong></li>
          </ul>
        )}
        <div className="modal-actions">
          <button type="button" className="ghost" onClick={onClose}>Cancelar</button>
          <button type="submit">{scheduled ? "Agendar" : "Transferir"}</button>
        </div>
      </form>
    </div>
  );
}
