import { FormEvent, useEffect, useState } from "react";
import { Account, changeAccountLimits } from "./api";
import { useBanking } from "./banking";

type LimitsModalProps = {
  open: boolean;
  account: Account;
  onClose: () => void;
};

export function LimitsModal({ open, account, onClose }: LimitsModalProps) {
  const { refresh, setNotice } = useBanking();
  const [dayAmount, setDayAmount] = useState("");
  const [dayAttempts, setDayAttempts] = useState("");
  const [nightAmount, setNightAmount] = useState("");
  const [nightAttempts, setNightAttempts] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (!open) return;
    setDayAmount(String(account.dayMaxAmountPerHour));
    setDayAttempts(String(account.dayMaxAttemptsPerHour));
    setNightAmount(String(account.nightMaxAmountPerHour));
    setNightAttempts(String(account.nightMaxAttemptsPerHour));
    setError("");
  }, [open, account.id]);

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
      await changeAccountLimits(account.id, {
        dayMaxAmountPerHour: Number(dayAmount),
        dayMaxAttemptsPerHour: Number(dayAttempts),
        nightMaxAmountPerHour: Number(nightAmount),
        nightMaxAttemptsPerHour: Number(nightAttempts)
      });
      await refresh();
      setNotice("Limites atualizados.");
      onClose();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao salvar os limites.");
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <form className="modal" onClick={(event) => event.stopPropagation()} onSubmit={submit}>
        <header>
          <h2>Limites por hora</h2>
          <p className="lede">{account.holderName}</p>
        </header>
        {error && <p className="banner error">{error}</p>}
        <div className="limit-fields">
          <label>
            Dia, valor (R$)
            <input value={dayAmount} onChange={(event) => setDayAmount(event.target.value)} type="number" min="0.01" step="0.01" required />
          </label>
          <label>
            Dia, tentativas
            <input value={dayAttempts} onChange={(event) => setDayAttempts(event.target.value)} type="number" min="1" step="1" required />
          </label>
          <label>
            Noite, valor (R$)
            <input value={nightAmount} onChange={(event) => setNightAmount(event.target.value)} type="number" min="0.01" step="0.01" required />
          </label>
          <label>
            Noite, tentativas
            <input value={nightAttempts} onChange={(event) => setNightAttempts(event.target.value)} type="number" min="1" step="1" required />
          </label>
        </div>
        <p className="hint">Noite (20h–6h), horário de Brasília. O próximo envio desta conta usa estes valores.</p>
        <div className="modal-actions">
          <button type="button" className="ghost" onClick={onClose}>Cancelar</button>
          <button type="submit">Salvar limites</button>
        </div>
      </form>
    </div>
  );
}
