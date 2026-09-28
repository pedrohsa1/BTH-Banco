import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "./Layout";
import { AccountPage } from "./pages/AccountPage";
import { AccountsPage } from "./pages/AccountsPage";
import { NewAccountPage } from "./pages/NewAccountPage";
import { OverdraftPage } from "./pages/OverdraftPage";
import { TransferFormPage } from "./pages/TransferFormPage";
import { TransferPage } from "./pages/TransferPage";
import { TransfersPage } from "./pages/TransfersPage";

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to="/contas" replace />} />
        <Route path="contas" element={<AccountsPage />} />
        <Route path="contas/nova" element={<NewAccountPage />} />
        <Route path="administrador/cheque-especial" element={<OverdraftPage />} />
        <Route path="contas/:id" element={<AccountPage />} />
        <Route path="transferencias" element={<TransfersPage />} />
        <Route path="transferencias/nova" element={<TransferFormPage scheduled={false} />} />
        <Route path="transferencias/agendar" element={<TransferFormPage scheduled />} />
        <Route path="transferencias/:id" element={<TransferPage />} />
      </Route>
    </Routes>
  );
}
