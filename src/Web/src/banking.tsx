import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { Account, Transfer, getAccounts, getTransfers } from "./api";

type BankingState = {
  accounts: Account[];
  transfers: Transfer[];
  error: string;
  notice: string;
  setError: (value: string) => void;
  setNotice: (value: string) => void;
  refresh: () => Promise<void>;
  nameOf: (id: string) => string;
  accountOf: (key: string) => Account | undefined;
  pathOf: (id: string) => string;
};

const BankingContext = createContext<BankingState | null>(null);

export function BankingProvider({ children }: { children: React.ReactNode }) {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [transfers, setTransfers] = useState<Transfer[]>([]);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");

  const refresh = useCallback(async () => {
    const [nextAccounts, nextTransfers] = await Promise.all([getAccounts(), getTransfers()]);
    setAccounts(nextAccounts);
    setTransfers(nextTransfers);
  }, []);

  useEffect(() => {
    refresh().catch((reason: Error) => setError(reason.message));

    const connection = new HubConnectionBuilder()
      .withUrl("/hubs/banking")
      .withAutomaticReconnect()
      .build();

    const reload = () => {
      refresh().catch(() => undefined);
    };

    connection.on("transferUpdated", reload);
    connection.on("accountUpdated", reload);
    connection.start().catch(() => undefined);

    return () => {
      connection.stop().catch(() => undefined);
    };
  }, [refresh]);

  const accountOf = useCallback(
    (key: string) => {
      let decoded = key;
      try {
        decoded = decodeURIComponent(key);
      } catch {
        decoded = key;
      }
      return accounts.find((account) => account.id === key || account.document === decoded);
    },
    [accounts]
  );

  const nameOf = useCallback(
    (id: string) => accountOf(id)?.holderName ?? "Conta",
    [accountOf]
  );

  const pathOf = useCallback(
    (id: string) => `/contas/${encodeURIComponent(accountOf(id)?.document ?? id)}`,
    [accountOf]
  );

  return (
    <BankingContext.Provider value={{ accounts, transfers, error, notice, setError, setNotice, refresh, nameOf, accountOf, pathOf }}>
      {children}
    </BankingContext.Provider>
  );
}

export function useBanking() {
  const value = useContext(BankingContext);
  if (!value) throw new Error("useBanking precisa ficar dentro do BankingProvider");
  return value;
}
