import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Wallet, Ban, ArrowLeft } from "lucide-react";
import PageHeader from "../../components/PageHeader";
import StatusBadge from "../../components/StatusBadge";
import { LoadingState, ErrorState } from "../../components/States";
import { useFetch } from "../../hooks/useFetch";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useTranslation } from "../../i18n/LanguageContext";
import { useAuth } from "../../context/AuthContext";
import type { InvoiceDto, PaymentMethod, RecordPaymentResultDto, BankAccountDto } from "../../api/types";

export default function InvoiceDetailPage() {
  const { t } = useTranslation();
  const { role } = useAuth();
  const { invoiceId } = useParams<{ invoiceId: string }>();
  const { data: invoice, loading, error, reload } = useFetch<InvoiceDto>(invoiceId ? `/invoices/${invoiceId}` : null, [invoiceId]);
  const [showPayment, setShowPayment] = useState(false);
  const [showVerify, setShowVerify] = useState(false);
  const [lastReceipt, setLastReceipt] = useState<RecordPaymentResultDto | null>(null);
  const [cancelling, setCancelling] = useState(false);
  const [payingOnline, setPayingOnline] = useState<string | null>(null);
  const [payOnlineError, setPayOnlineError] = useState<string | null>(null);

  async function payOnline(providerKey: string) {
    if (!invoiceId) return;
    setPayingOnline(providerKey);
    setPayOnlineError(null);
    try {
      const { data } = await apiClient.post<{ checkoutUrl: string }>(`/invoices/${invoiceId}/pay/${providerKey}`);
      window.location.href = data.checkoutUrl;
    } catch (err) {
      setPayOnlineError(extractErrorMessage(err));
      setPayingOnline(null);
    }
  }

  async function cancelInvoice() {
    if (!invoiceId) return;
    setCancelling(true);
    try {
      await apiClient.post(`/invoices/${invoiceId}/cancel`);
      reload();
    } finally {
      setCancelling(false);
    }
  }

  if (loading) return <LoadingState label={t("common.loading")} />;
  if (error) return <ErrorState message={error} />;
  if (!invoice) return null;

  const canCancel = role === "SuperAdmin" && invoice.status !== "Cancelled" && invoice.status !== "Paid";

  return (
    <div>
      <Link to="/invoices" className="mb-3 inline-flex items-center gap-1 text-sm text-evergreen-700 hover:underline">
        <ArrowLeft className="h-4 w-4" /> {t("common.back")}
      </Link>

      <PageHeader
        eyebrow={invoice.invoiceNumber}
        title={`${invoice.grandTotal.toFixed(2)} ${t("common.currency")}`}
        description={new Date(invoice.invoiceDate).toLocaleString()}
        action={
          <div className="flex gap-2">
            {invoice.balanceDue > 0 && invoice.status !== "Cancelled" && (
              <>
                <button className="btn-secondary" onClick={() => payOnline("chapa")} disabled={payingOnline !== null}>
                  <Wallet className="h-4 w-4" /> {payingOnline === "chapa" ? t("common.loading") : `${t("invoices.payOnline")} (Chapa)`}
                </button>
                <button className="btn-secondary" onClick={() => payOnline("telebirr")} disabled={payingOnline !== null}>
                  <Wallet className="h-4 w-4" /> {payingOnline === "telebirr" ? t("common.loading") : `${t("invoices.payOnline")} (Telebirr)`}
                </button>
              </>
            )}
            {invoice.balanceDue > 0 && invoice.status !== "Cancelled" && (
              <button className="btn-secondary" onClick={() => setShowVerify((s) => !s)}>
                <Wallet className="h-4 w-4" /> {t("invoices.alreadyPaid")}
              </button>
            )}
            {invoice.balanceDue > 0 && invoice.status !== "Cancelled" && (
              <button className="btn-primary" onClick={() => setShowPayment((s) => !s)}>
                <Wallet className="h-4 w-4" /> {t("invoices.recordPayment")}
              </button>
            )}
            {canCancel && (
              <button className="btn-secondary text-clay-600" onClick={cancelInvoice} disabled={cancelling}>
                <Ban className="h-4 w-4" /> {t("invoices.cancelInvoice")}
              </button>
            )}
          </div>
        }
      />

      <div className="mb-4 flex items-center gap-2">
        <StatusBadge status={invoice.status} />
        {!invoice.taxWasEnabled && <span className="badge bg-ink-900/5 text-ink-600">{t("invoices.taxDisabledBadge")}</span>}
      </div>

      {lastReceipt && (
        <div className="mb-4 rounded-md border border-evergreen-200 bg-evergreen-50 px-4 py-3 text-sm text-evergreen-700">
          {t("invoices.receiptIssued")} <span className="font-mono">{lastReceipt.receiptNumber}</span>
        </div>
      )}

      {payOnlineError && <div className="mb-4"><ErrorState message={payOnlineError} /></div>}

      {showVerify && (
        <div className="mb-6">
          <AlreadyPaidForm
            invoiceId={invoice.id}
            balanceDue={invoice.balanceDue}
            onRecorded={(result) => {
              setShowVerify(false);
              setLastReceipt(result);
              reload();
            }}
          />
        </div>
      )}

      {showPayment && (
        <div className="mb-6">
          <RecordPaymentForm
            invoiceId={invoice.id}
            balanceDue={invoice.balanceDue}
            onRecorded={(result) => {
              setShowPayment(false);
              setLastReceipt(result);
              reload();
            }}
          />
        </div>
      )}

      <div className="card mb-6 overflow-x-auto">
        <table className="table-shell">
          <thead>
            <tr>
              <th>{t("orders.product")}</th>
              <th>{t("common.quantity")}</th>
              <th>{t("products.unitPrice")}</th>
              <th>{t("orders.subtotal")}</th>
              <th>{t("products.taxProfile")}</th>
              <th>{t("invoices.rateApplied")}</th>
              <th>{t("invoices.tax")}</th>
              <th>{t("orders.lineTotal")}</th>
            </tr>
          </thead>
          <tbody>
            {invoice.items.map((item) => (
              <tr key={item.productId}>
                <td>{item.productName}</td>
                <td>{item.quantity}</td>
                <td>{item.unitPrice.toFixed(2)}</td>
                <td>{item.lineSubtotal.toFixed(2)}</td>
                <td>{item.taxProfileApplied}</td>
                <td className="font-mono text-xs">{item.taxRateApplied}%</td>
                <td>{item.taxAmount.toFixed(2)}</td>
                <td className="font-medium">{item.lineTotal.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="card max-w-sm p-5">
        <SummaryRow label={t("orders.subtotal")} value={invoice.subtotal} />
        <SummaryRow label={t("invoices.taxableAmount")} value={invoice.taxableAmount} />
        <SummaryRow label={t("invoices.tax")} value={invoice.taxAmount} />
        <SummaryRow
          label={`${t("invoices.platformCommission")} (${invoice.platformCommissionRateApplied}%)`}
          value={invoice.platformCommissionAmount}
        />
        <SummaryRow label={t("invoices.hotelAgentBonus")} value={invoice.hotelAgentBonusAmount} />
        <SummaryRow label={t("invoices.discount")} value={-invoice.discount} />
        <div className="my-2 border-t border-evergreen-100" />
        <SummaryRow label={t("invoices.grandTotal")} value={invoice.grandTotal} bold />
        <SummaryRow label={t("invoices.paid")} value={invoice.amountPaid} />
        <SummaryRow label={t("invoices.balanceDue")} value={invoice.balanceDue} bold={invoice.balanceDue > 0} />
      </div>
    </div>
  );
}

function SummaryRow({ label, value, bold }: { label: string; value: number; bold?: boolean }) {
  const { t } = useTranslation();
  return (
    <div className={`flex justify-between py-1 text-sm ${bold ? "font-semibold text-ink-900" : "text-ink-600"}`}>
      <span>{label}</span>
      <span>{value.toFixed(2)} {t("common.currency")}</span>
    </div>
  );
}

function RecordPaymentForm({
  invoiceId, balanceDue, onRecorded
}: { invoiceId: string; balanceDue: number; onRecorded: (result: RecordPaymentResultDto) => void }) {
  const { t } = useTranslation();
  const [amount, setAmount] = useState(balanceDue);
  const [method, setMethod] = useState<PaymentMethod>("Cash");
  const [reference, setReference] = useState("");
  const [bankAccountId, setBankAccountId] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { data: bankAccounts } = useFetch<BankAccountDto[]>(method === "Bank" ? "/bankaccounts" : null, [method]);

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      const { data } = await apiClient.post<RecordPaymentResultDto>("/payments", {
        invoiceId, amount, method, reference: reference || null,
        bankAccountId: method === "Bank" && bankAccountId ? bankAccountId : null
      });
      onRecorded(data);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-4 text-base font-semibold text-evergreen-900">{t("invoices.recordPayment")}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label className="label">{t("common.amount")} ({t("common.currency")})</label>
          <input type="number" className="input" value={amount} onChange={(e) => setAmount(Number(e.target.value))} />
        </div>
        <div>
          <label className="label">{t("invoices.method")}</label>
          <select className="input" value={method} onChange={(e) => setMethod(e.target.value as PaymentMethod)}>
            <option value="Cash">{t("invoices.methodCash")}</option>
            <option value="Bank">{t("invoices.methodBank")}</option>
            <option value="Telebirr">Telebirr</option>
            <option value="Chapa">Chapa</option>
            <option value="CbeBirr">CBE Birr</option>
            <option value="Credit">{t("invoices.methodCredit")}</option>
          </select>
        </div>
        <div>
          <label className="label">{t("invoices.reference")}</label>
          <input className="input" value={reference} onChange={(e) => setReference(e.target.value)} />
        </div>
        {method === "Bank" && bankAccounts && bankAccounts.length > 0 && (
          <div className="sm:col-span-3">
            <label className="label">{t("bank.accounts")}</label>
            <select className="input" value={bankAccountId} onChange={(e) => setBankAccountId(e.target.value)}>
              <option value="">-</option>
              {bankAccounts.map((a) => (
                <option key={a.id} value={a.id}>{a.bankName} - {a.accountName} ({a.accountNumber})</option>
              ))}
            </select>
          </div>
        )}
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || amount <= 0}>
        {saving ? t("common.saving") : t("invoices.recordPayment")}
      </button>
    </div>
  );
}

/**
 * The self-service "I already transferred the money" panel. Two paths:
 *  - A provider with automatic verification (currently just CBE Birr - see
 *    GET /payments/verifiers): enter the reference (+ phone/suffix if
 *    needed), and the invoice is confirmed paid immediately if it checks
 *    out against the provider's own public receipt lookup.
 *  - Any other bank (CBE regular transfer, Bank of Abyssinia, Awash,
 *    Dashen, or any bank Admin has added under Settings -> Bank Accounts):
 *    shown as "transfer to this account, then tell us the reference" -
 *    recorded the same way staff would, for an admin to reconcile via the
 *    bank reconciliation screen if anything looks off.
 */
function AlreadyPaidForm({ invoiceId, balanceDue, onRecorded }: { invoiceId: string; balanceDue: number; onRecorded: (result: RecordPaymentResultDto) => void }) {
  const { t } = useTranslation();
  const { data: verifierKeys } = useFetch<string[]>("/payments/verifiers");
  const { data: bankAccounts } = useFetch<BankAccountDto[]>("/bankaccounts");
  const activeBankAccounts = (bankAccounts ?? []).filter((a) => a.isActive);

  const [selection, setSelection] = useState<string>("");
  const [reference, setReference] = useState("");
  const [secondaryIdentifier, setSecondaryIdentifier] = useState("");
  const [bankAccountId, setBankAccountId] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isAutoVerified = selection !== "" && selection !== "otherbank" && (verifierKeys ?? []).includes(selection);
  const selectedBank = activeBankAccounts.find((a) => a.id === bankAccountId);

  async function submit() {
    if (!reference) return;
    setSaving(true);
    setError(null);
    try {
      if (isAutoVerified) {
        const { data } = await apiClient.post<RecordPaymentResultDto>(`/invoices/${invoiceId}/verify-payment`, {
          providerKey: selection, reference, secondaryIdentifier: secondaryIdentifier || null, bankAccountId: null
        });
        onRecorded(data);
      } else {
        // No automatic verifier for this bank yet - record it the same way
        // staff would, for an admin to reconcile.
        const { data } = await apiClient.post<RecordPaymentResultDto>("/payments", {
          invoiceId, amount: balanceDue, method: "Bank", reference, bankAccountId: bankAccountId || null
        });
        onRecorded(data);
      }
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card p-6">
      <h3 className="mb-1 text-base font-semibold text-evergreen-900">{t("invoices.alreadyPaid")}</h3>
      <p className="mb-4 text-xs text-ink-600">{t("invoices.alreadyPaidHint")}</p>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="label">{t("invoices.paidVia")}</label>
          <select className="input" value={selection} onChange={(e) => { setSelection(e.target.value); setBankAccountId(""); }}>
            <option value="">-</option>
            {(verifierKeys ?? []).includes("cbebirr") && <option value="cbebirr">CBE Birr</option>}
            <option value="otherbank">{t("invoices.anyBankInEthiopia")}</option>
          </select>
        </div>

        {selection === "cbebirr" && (
          <div>
            <label className="label">{t("invoices.payerPhone")}</label>
            <input className="input" value={secondaryIdentifier} onChange={(e) => setSecondaryIdentifier(e.target.value)} placeholder="2519XXXXXXXX" />
          </div>
        )}

        {selection === "otherbank" && (
          <div>
            <label className="label">{t("bank.accounts")}</label>
            <select className="input" value={bankAccountId} onChange={(e) => setBankAccountId(e.target.value)}>
              <option value="">-</option>
              {activeBankAccounts.map((a) => (
                <option key={a.id} value={a.id}>{a.bankName} - {a.accountName} ({a.accountNumber})</option>
              ))}
            </select>
          </div>
        )}

        {selection && (
          <div className="sm:col-span-2">
            <label className="label">{t("invoices.reference")}</label>
            <input className="input" value={reference} onChange={(e) => setReference(e.target.value)} />
          </div>
        )}
      </div>

      {selection === "otherbank" && selectedBank && (
        <p className="mt-3 rounded-md bg-evergreen-50 px-3 py-2 text-xs text-evergreen-700">
          {t("invoices.transferInstructions")}: <strong>{selectedBank.bankName}</strong> - {selectedBank.accountName} ({selectedBank.accountNumber})
        </p>
      )}
      {selection === "otherbank" && (
        <p className="mt-2 text-[11px] text-ink-300">{t("invoices.manualReconciliationNote")}</p>
      )}

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !selection || !reference}>
        {saving ? t("common.saving") : t("invoices.confirmPayment")}
      </button>
    </div>
  );
}
