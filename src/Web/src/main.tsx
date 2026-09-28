import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./App";
import { BankingProvider } from "./banking";
import "./styles.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter>
      <BankingProvider>
        <App />
      </BankingProvider>
    </BrowserRouter>
  </StrictMode>
);
