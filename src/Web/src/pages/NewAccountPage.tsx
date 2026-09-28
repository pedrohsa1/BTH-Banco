import { FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import { openAccount } from "../api";
import { useBanking } from "../banking";

export function NewAccountPage() {
  const navigate = useNavigate();
  const { refresh, setError, setNotice } = useBanking();
  const [holderName, setHolderName] = useState("");
  const [documentNumber, setDocumentNumber] = useState("");
  const [initialBalance, setInitialBalance] = useState("1000");
  const [overdraftLimit, setOverdraftLimit] = useState("500");

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError("");
    setNotice("");
    try {
      await openAccount({
        holderName,
        document: documentNumber,
        initialBalance: Number(initialBalance),
        overdraftLimit: Number(overdraftLimit)
      });
      await refresh();
      setNotice("Conta aberta.");
      navigate(`/contas/${encodeURIComponent(documentNumber.trim())}`);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao abrir conta.");
    }
  }

  return (
    <div className="sheet">
      <header className="page-head">
        <div>
          <p className="eyebrow">Contas</p>
          <h1>Abrir conta</h1>
        </div>
      </header>
      <form className="panel stack narrow" onSubmit={submit}>
        <label>
          Nome
          <input value={holderName} onChange={(event) => setHolderName(event.target.value)} required />
        </label>
        <label>
          Documento
          <input value={documentNumber} onChange={(event) => setDocumentNumber(event.target.value)} required />
        </label>
        <label>
          Saldo inicial
          <input value={initialBalance} onChange={(event) => setInitialBalance(event.target.value)} type="number" min="0" step="0.01" />
        </label>
        <label>
          Cheque especial
          <input value={overdraftLimit} onChange={(event) => setOverdraftLimit(event.target.value)} type="number" min="0" step="0.01" />
        </label>
        <p className="hint">Os limites padrão são R$ 5.000 e 5 tentativas de dia, R$ 1.000 e 3 tentativas de noite.</p>
        <button type="submit">Abrir conta</button>
      </form>
    </div>
  );
}
