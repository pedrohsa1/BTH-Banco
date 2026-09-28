export const money = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export const statusLabel: Record<string, string> = {
  Scheduled: "Agendada",
  Processing: "Processando",
  Completed: "Concluída",
  Failed: "Falhou",
  Cancelled: "Cancelada",
  Active: "Ativa",
  Inactive: "Inativa"
};

export function label(status: string) {
  return statusLabel[status] ?? status;
}

export function when(value: string | null) {
  if (!value) return "—";
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" }).format(new Date(value));
}
