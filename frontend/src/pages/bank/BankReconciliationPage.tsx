import { useState } from "react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState, EmptyState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { BankAccountDto, BankTransactionDto, BankTransactionType } from "../../api/types";

export default function BankReconciliationPage() {
  const { t } = useTranslation();
  const [filterAccountId, setFilterAccountId] = useState("");
  const [showNewAccount, setShowNewAccount] = useState(false);
  const [showNewTransaction, setShowNewTransaction] = useState(false);

  const { data: accounts, loading: loadingAccounts, error: accountsError, reload: reloadAccounts } =
    useFetch<BankAccountDto[]>("/bankaccounts");

  const url = filterAccountId
    ? `/bank-reconciliation/transactions/unreconciled?bankAccountId=${filterAccountId}`
    : "/bank-reconciliation/transactions/unreconciled";
  const { data: transactions, loading, error, reload } = useFetch<BankTransactionDto[]>(url, [filterAccountId]);

  return (
    <div>
      <PageHeader
        title={t("bank.title")}
        description={t("bank.description")}
        action={
          <div className="flex gap-2">
            <button className="btn-secondary" onClick={() => setShowNewAccount((s) => !s)}>{t("bank.newAccount")}</button>
            <button className="btn-primary" onClick={() => setShowNewTransaction((s) => !s)}>{t("bank.recordTransaction")}</button>
          </div>
        }
      />

      <h3 className="mb-3 text-base font-semibold text-evergreen-900">{t("bank.accounts")}</h3>
      {loadingAccounts && <LoadingState label={t("common.loading")} />}
      {accountsError && <ErrorState message={accountsError} />}
      {accounts && accounts.length === 0 && <EmptyState title={t("bank.accounts")} />}
      {accounts && accounts.length > 0 && (
        <div className="card mb-6 overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("bank.bankName")}</th>
                <th>{t("bank.accountName")}</th>
                <th>{t("bank.accountNumber")}</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((a) => (
                <tr key={a.id} className={filterAccountId === a.id ? "bg-evergreen-50" : ""}>
                  <td className="font-medium">{a.bankName}</td>
                  <td>{a.accountName}</td>
                  <td className="font-mono text-xs">{a.accountNumber}</td>
                  <td><StatusBadge status={a.isActive ? "Active" : "Inactive"} /></td>
                  <td>
                    <div className="flex gap-1">
                      <button
                        className="btn-secondary px-2 py-1 text-xs"
                        onClick={() => setFilterAccountId(filterAccountId === a.id ? "" : a.id)}
                      >
                        {filterAccountId === a.id ? t("common.view") : t("bank.filterByAccount")}
                      </button>
                      <button
                        className="btn-secondary px-2 py-1 text-xs"
                        onClick={async () => {
                          if (a.isActive && !confirm(`${t("common.delete")} "${a.bankName} - ${a.accountName}"?`)) return;
                          try {
                            if (a.isActive) {
                              await apiClient.delete(`/bankaccounts/${a.id}`);
                            } else {
                              await apiClient.put(`/bankaccounts/${a.id}`, {
                                bankName: a.bankName, accountName: a.accountName, accountNumber: a.accountNumber, isActive: true
                              });
                            }
                            reloadAccounts();
                          } catch (err) {
                            alert(extractErrorMessage(err));
                          }
                        }}
                      >
                        {a.isActive ? t("common.delete") : t("common.restore")}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showNewAccount && (
        <div className="mb-6">
          <NewAccountForm onCreated={() => { setShowNewAccount(false); reloadAccounts(); }} />
        </div>
      )}

      {showNewTransaction && (
        <div className="mb-6">
          <NewTransactionForm onRecorded={() => { setShowNewTransaction(false); reload(); }} />
        </div>
      )}

      <div className="card mb-6 max-w-lg p-6">
        <label className="label">{t("bank.filterByAccount")}</label>
        <div className="flex gap-2">
          <input className="input" value={filterAccountId} onChange={(e) => setFilterAccountId(e.target.value)} />
          <button className="btn-secondary" onClick={reload}>{t("bank.refresh")}</button>
        </div>
      </div>

      <h3 className="mb-3 text-base font-semibold text-evergreen-900">{t("bank.unreconciledTitle")}</h3>

      {loading && <LoadingState label={t("common.loading")} />}
      {error && <ErrorState message={error} />}
      {transactions && transactions.length === 0 && (
        <EmptyState title={t("bank.emptyTitle")} description={t("bank.emptyDescription")} />
      )}

      {transactions && transactions.length > 0 && (
        <div className="card overflow-x-auto">
          <table className="table-shell">
            <thead>
              <tr>
                <th>{t("common.date")}</th>
                <th>{t("bank.transactionType")}</th>
                <th>{t("common.amount")}</th>
                <th>{t("bank.transactionDescription")}</th>
                <th>{t("common.status")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((tx) => (
                <TransactionRow key={tx.id} tx={tx} onChanged={reload} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function TransactionRow({ tx, onChanged }: { tx: BankTransactionDto; onChanged: () => void }) {
  const { t } = useTranslation();
  const [paymentId, setPaymentId] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function match() {
    if (!paymentId) return;
    setBusy(true);
    setError(null);
    try {
      await apiClient.post(`/bank-reconciliation/transactions/${tx.id}/reconcile`, { paymentId });
      onChanged();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function dispute() {
    setBusy(true);
    setError(null);
    try {
      await apiClient.post(`/bank-reconciliation/transactions/${tx.id}/dispute`);
      onChanged();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <tr>
      <td>{new Date(tx.transactionDate).toLocaleDateString()}</td>
      <td>{tx.type}</td>
      <td>{tx.amount.toFixed(2)} {t("common.currency")}</td>
      <td>{tx.description ?? "-"}</td>
      <td><StatusBadge status={tx.reconciliationStatus} /></td>
      <td>
        <div className="flex items-center gap-1.5">
          <input
            className="input w-32 py-1 text-xs"
            placeholder={t("bank.paymentId")}
            value={paymentId}
            onChange={(e) => setPaymentId(e.target.value)}
          />
          <button className="btn-secondary px-2 py-1 text-xs" onClick={match} disabled={busy || !paymentId}>
            {t("bank.matchToPayment")}
          </button>
          <button className="btn-secondary px-2 py-1 text-xs text-clay-600" onClick={dispute} disabled={busy}>
            {t("bank.dispute")}
          </button>
        </div>
        {error && <p className="mt-1 text-[11px] text-clay-600">{error}</p>}
      </td>
    </tr>
  );
}

function NewAccountForm({ onCreated }: { onCreated: () => void }) {
  const { t } = useTranslation();
  const [bankName, setBankName] = useState("");
  const [accountName, setAccountName] = useState("");
  const [accountNumber, setAccountNumber] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/bankaccounts", { bankName, accountName, accountNumber });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("bank.newAccount")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label className="label">{t("bank.bankName")}</label>
          <input className="input" value={bankName} onChange={(e) => setBankName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("bank.accountName")}</label>
          <input className="input" value={accountName} onChange={(e) => setAccountName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("bank.accountNumber")}</label>
          <input className="input" value={accountNumber} onChange={(e) => setAccountNumber(e.target.value)} />
        </div>
      </div>
      {error && <div className="mt-4"><ErrorState message={error} /></div>}
      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !bankName}>
        {saving ? t("common.creating") : t("bank.createAccount")}
      </button>
    </div>
  );
}

function NewTransactionForm({ onRecorded }: { onRecorded: () => void }) {
  const { t } = useTranslation();
  const [bankAccountId, setBankAccountId] = useState("");
  const [type, setType] = useState<BankTransactionType>("Deposit");
  const [amount, setAmount] = useState(0);
  const [transactionDate, setTransactionDate] = useState(new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      await apiClient.post("/bank-reconciliation/transactions", {
        bankAccountId, type, amount, transactionDate: new Date(transactionDate).toISOString(), description: description || null
      });
      onRecorded();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("bank.recordTransaction")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("bank.bankAccountId")}</label>
          <input className="input" value={bankAccountId} onChange={(e) => setBankAccountId(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("bank.transactionType")}</label>
          <select className="input" value={type} onChange={(e) => setType(e.target.value as BankTransactionType)}>
            <option value="Deposit">{t("bank.deposit")}</option>
            <option value="Withdrawal">{t("bank.withdrawal")}</option>
          </select>
        </div>
        <div>
          <label className="label">{t("common.amount")}</label>
          <input type="number" className="input" value={amount} onChange={(e) => setAmount(Number(e.target.value))} />
        </div>
        <div>
          <label className="label">{t("common.date")}</label>
          <input type="date" className="input" value={transactionDate} onChange={(e) => setTransactionDate(e.target.value)} />
        </div>
        <div className="sm:col-span-2">
          <label className="label">{t("bank.transactionDescription")}</label>
          <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
        </div>
      </div>
      {error && <div className="mt-4"><ErrorState message={error} /></div>}
      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !bankAccountId || amount <= 0}>
        {saving ? t("common.saving") : t("bank.recordTransaction")}
      </button>
    </div>
  );
}
