import { FormEvent, useEffect, useState } from "react";
import { changeOverdraft } from "../api";
import { useBanking } from "../banking";
import { money } from "../format";

export function OverdraftPage() {
  const { accounts, refresh, setNotice } = useBanking();
  const [values, setValues] = useState<Record<string, string>>({});
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    setValues((current) => {
      const next = { ...current };
      for (const account of accounts) {
        if (next[account.id] === undefined) next[account.id] = String(account.overdraftLimit);
      }
      return next;
    });
  }, [accounts]);

  async function save(event: FormEvent, id: string) {
    event.preventDefault();
    setErrors((current) => ({ ...current, [id]: "" }));
    try {
      await changeOverdraft(id, Number(values[id]));
      await refresh();
      setNotice("Cheque especial atualizado.");
    } catch (reason) {
      setErrors((current) => ({
        ...current,
        [id]: reason instanceof Error ? reason.message : "Falha ao salvar o cheque especial."
      }));
    }
  }

  return (
    <div className="sheet wide">
      <header className="page-head">
        <div>
          <p className="eyebrow">Administrador</p>
          <h1>Cheque especial</h1>
        </div>
      </header>
      <section className="panel overdraft-list">
        {accounts.length === 0 && <p className="hint">Nenhuma conta para configurar.</p>}
        {accounts.map((account) => (
          <form key={account.id} onSubmit={(event) => save(event, account.id)}>
            <div>
              <strong>{account.holderName}</strong>
              <span>Conta {account.document}</span>
              <small>Atual {money.format(account.overdraftLimit)}</small>
            </div>
            <label>
              Limite (R$)
              <input
                value={values[account.id] ?? ""}
                onChange={(event) => setValues((current) => ({ ...current, [account.id]: event.target.value }))}
                type="number"
                min="0"
                step="0.01"
                required
              />
            </label>
            <button type="submit">Salvar</button>
            {errors[account.id] && <p className="banner error">{errors[account.id]}</p>}
          </form>
        ))}
      </section>
    </div>
  );
}
