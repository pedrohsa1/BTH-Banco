export type Account = {
  id: string;
  holderName: string;
  document: string;
  balance: number;
  overdraftLimit: number;
  availableFunds: number;
  status: string;
  dayMaxAmountPerHour: number;
  dayMaxAttemptsPerHour: number;
  nightMaxAmountPerHour: number;
  nightMaxAttemptsPerHour: number;
};

export type Transfer = {
  id: string;
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  status: string;
  scheduledAtUtc: string | null;
  createdAtUtc: string;
  processedAtUtc: string | null;
  failureReason: string | null;
  replayed: boolean;
};

async function readError(response: Response): Promise<string> {
  const body = await response.json().catch(() => null);
  if (body?.failureReason) return body.failureReason as string;
  if (body?.detail) return body.detail as string;
  if (body?.title) return body.title as string;
  return "Não foi possível concluir a operação.";
}

export async function getAccounts(): Promise<Account[]> {
  const response = await fetch("/api/accounts");
  if (!response.ok) throw new Error(await readError(response));
  return response.json();
}

export async function getAccount(id: string): Promise<Account> {
  const response = await fetch(`/api/accounts/${id}`);
  if (!response.ok) throw new Error(await readError(response));
  return response.json();
}

export async function getTransfers(accountId?: string): Promise<Transfer[]> {
  const query = accountId ? `?accountId=${accountId}` : "";
  const response = await fetch(`/api/transfers${query}`);
  if (!response.ok) throw new Error(await readError(response));
  return response.json();
}

export async function getTransfer(id: string): Promise<Transfer> {
  const response = await fetch(`/api/transfers/${id}`);
  if (!response.ok) throw new Error(await readError(response));
  return response.json();
}

export async function createTransfer(input: {
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  scheduledAtUtc?: string;
}): Promise<{ transfer: Transfer; ok: boolean; message: string }> {
  const path = input.scheduledAtUtc ? "/api/transfers/scheduled" : "/api/transfers";
  const response = await fetch(path, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Idempotency-Key": crypto.randomUUID()
    },
    body: JSON.stringify(input)
  });
  const transfer = (await response.json()) as Transfer;
  return {
    transfer,
    ok: response.ok && transfer.status !== "Failed",
    message: transfer.failureReason ?? (response.ok ? "Transferência registrada." : await readError(new Response(JSON.stringify(transfer))))
  };
}

export async function cancelTransfer(id: string): Promise<void> {
  const response = await fetch(`/api/transfers/${id}/cancel`, { method: "POST" });
  if (!response.ok) throw new Error(await readError(response));
}

export async function openAccount(input: {
  holderName: string;
  document: string;
  initialBalance: number;
  overdraftLimit: number;
}): Promise<string> {
  const response = await fetch("/api/accounts", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input)
  });
  if (!response.ok) throw new Error(await readError(response));
  const body = (await response.json()) as { id: string };
  return body.id;
}

export async function changeAccountLimits(
  id: string,
  limits: {
    dayMaxAmountPerHour: number;
    dayMaxAttemptsPerHour: number;
    nightMaxAmountPerHour: number;
    nightMaxAttemptsPerHour: number;
  }
): Promise<void> {
  const response = await fetch(`/api/accounts/${id}/limits`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(limits)
  });
  if (!response.ok) throw new Error(await readError(response));
}

export async function changeOverdraft(id: string, overdraftLimit: number): Promise<void> {
  const response = await fetch(`/api/accounts/${id}/overdraft`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ overdraftLimit })
  });
  if (!response.ok) throw new Error(await readError(response));
}

export async function changeAccountStatus(id: string, status: string): Promise<void> {
  const response = await fetch(`/api/accounts/${id}/status`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ status })
  });
  if (!response.ok) throw new Error(await readError(response));
}
