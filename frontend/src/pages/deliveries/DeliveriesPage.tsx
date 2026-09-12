import { useState, useRef, useEffect } from "react";
import { Link, useLocation } from "react-router-dom";
import PageHeader from "../../components/PageHeader";
import ExportButtons from "../../components/ExportButtons";
import StatusBadge from "../../components/StatusBadge";
import { ErrorState, LoadingState, EmptyState } from "../../components/States";
import { apiClient, extractErrorMessage } from "../../api/client";
import { useFetch } from "../../hooks/useFetch";
import { useTranslation } from "../../i18n/LanguageContext";
import type { DeliveryDto, FarmerDto, CustomerDto, OrderDto } from "../../api/types";

export default function DeliveriesPage() {
  const { t } = useTranslation();
  const location = useLocation();
  const { data: deliveries, loading, error, reload } = useFetch<DeliveryDto[]>("/deliveries");
  const { data: awaitingOrders, loading: loadingAwaiting } = useFetch<OrderDto[]>("/orders/awaiting-delivery");
  const [selected, setSelected] = useState<DeliveryDto | null>(null);
  const [orderIdForNew, setOrderIdForNew] = useState<string | null>(
    (location.state as { orderId?: string } | null)?.orderId ?? null
  );
  const orderForNew = awaitingOrders?.find((o) => o.id === orderIdForNew) ?? null;

  return (
    <div>
      <PageHeader
        title={t("deliveries.title")}
        description={t("deliveries.description")}
        action={<ExportButtons basePath="/deliveries" filenameBase="deliveries" />}
      />

      {!selected && !orderIdForNew && (
        <>
          <div className="card mb-6 max-w-lg p-4">
            <label className="label">{t("deliveries.createForOrder")}</label>
            {loadingAwaiting && <LoadingState label={t("common.loading")} />}
            {!loadingAwaiting && (!awaitingOrders || awaitingOrders.length === 0) && (
              <p className="text-sm text-ink-300">{t("deliveries.noOrdersAwaiting")}</p>
            )}
            {!loadingAwaiting && awaitingOrders && awaitingOrders.length > 0 && (
              <select
                className="input"
                value=""
                onChange={(e) => {
                  if (e.target.value) setOrderIdForNew(e.target.value);
                }}
              >
                <option value="">{t("deliveries.selectOrder")}</option>
                {awaitingOrders.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.orderNumber} - {o.customerName ?? t("customers.title")} ({o.subtotal.toFixed(2)} {t("common.currency")})
                  </option>
                ))}
              </select>
            )}
            <p className="mt-1 text-xs text-ink-300">{t("deliveries.createForOrderHint")}</p>
          </div>

          {loading && <LoadingState label={t("common.loading")} />}
          {error && <ErrorState message={error} />}
          {deliveries && deliveries.length === 0 && <EmptyState title={t("deliveries.emptyTitle")} />}

          {deliveries && deliveries.length > 0 && (
            <div className="card overflow-x-auto">
              <table className="table-shell">
                <thead>
                  <tr>
                    <th>{t("orders.title")} #</th>
                    <th>{t("deliveries.originLocation")}</th>
                    <th>{t("deliveries.destinationAddress")}</th>
                    <th>{t("deliveries.driverName")}</th>
                    <th>{t("common.status")}</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {deliveries.map((d) => (
                    <tr key={d.id}>
                      <td className="font-mono text-xs">
                        <Link to={`/orders/${d.orderId}`} className="text-evergreen-700 underline underline-offset-2">
                          {d.orderNumber ?? d.orderId}
                        </Link>
                      </td>
                      <td>{d.originLocation ?? "-"}</td>
                      <td>{d.destinationAddress}</td>
                      <td>{d.driverName ?? "-"}</td>
                      <td><StatusBadge status={d.status} /></td>
                      <td>
                        <button className="btn-secondary px-2 py-1 text-xs" onClick={() => setSelected(d)}>
                          {t("deliveries.manage")}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      {orderIdForNew && !selected && (
        <div>
          <button className="btn-secondary mb-4" onClick={() => setOrderIdForNew(null)}>&larr; {t("common.back")}</button>
          <CreateDeliveryForm orderId={orderIdForNew} order={orderForNew} onCreated={() => { setOrderIdForNew(null); reload(); }} />
        </div>
      )}

      {selected && (
        <div>
          <button className="btn-secondary mb-4" onClick={() => { setSelected(null); reload(); }}>&larr; {t("common.back")}</button>
          <DeliveryDetail delivery={selected} onChanged={(updated) => setSelected(updated)} />
        </div>
      )}
    </div>
  );
}

function DeliveryDetail({ delivery: initial, onChanged }: { delivery: DeliveryDto; onChanged: (d: DeliveryDto) => void }) {
  const { t } = useTranslation();
  const { data: delivery, reload } = useFetch<DeliveryDto | null>(`/deliveries/by-order/${initial.orderId}`);
  const [showPod, setShowPod] = useState(false);
  const current = delivery ?? initial;

  async function dispatch() {
    await apiClient.post(`/deliveries/${current.id}/dispatch`);
    reload();
  }

  async function markFailed() {
    const notes = window.prompt(t("deliveries.failureReasonPrompt")) ?? "";
    await apiClient.post(`/deliveries/${current.id}/failed`, { notes });
    reload();
  }

  return (
    <div className="card max-w-lg p-6">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-evergreen-900">{current.destinationAddress}</h3>
        <StatusBadge status={current.status} />
      </div>
      <p className="text-sm text-ink-600">{t("orders.title")}: {current.orderNumber ?? current.orderId}</p>
      <p className="text-sm text-ink-600">{t("deliveries.originLocation")}: {current.originLocation ?? "-"}</p>
      <p className="text-sm text-ink-600">{t("deliveries.driverName")}: {current.driverName ?? "-"}</p>
      <p className="text-sm text-ink-600">{t("deliveries.vehicleInfo")}: {current.vehicleInfo ?? "-"}</p>
      {current.tripPrice != null && <p className="text-sm text-ink-600">{t("commissions.baseAmount")}: {current.tripPrice.toFixed(2)} {t("common.currency")}</p>}

      {current.status === "Delivered" && (
        <div className="mt-3 rounded-md border border-evergreen-100 bg-evergreen-50/50 p-3 text-sm">
          <p className="font-medium text-evergreen-900">{t("deliveries.proofOfDelivery")}</p>
          <p className="text-ink-600">{t("deliveries.receivedBy")}: {current.receivedByName ?? "-"}</p>
          {current.hasSignature && <p className="text-ink-600">{t("deliveries.signatureCaptured")}</p>}
          {current.deliveryLatitude != null && current.deliveryLongitude != null && (
            <p className="text-ink-600">GPS: {current.deliveryLatitude.toFixed(5)}, {current.deliveryLongitude.toFixed(5)}</p>
          )}
        </div>
      )}

      <div className="mt-4 flex gap-2">
        {current.status === "Pending" && (
          <button className="btn-primary" onClick={dispatch}>{t("deliveries.dispatch")}</button>
        )}
        {current.status === "InTransit" && (
          <>
            <button className="btn-primary" onClick={() => setShowPod((s) => !s)}>{t("deliveries.markDelivered")}</button>
            <button className="btn-secondary text-clay-600" onClick={markFailed}>{t("deliveries.markFailed")}</button>
          </>
        )}
      </div>

      {showPod && (
        <div className="mt-4">
          <ProofOfDeliveryForm deliveryId={current.id} onDone={() => { setShowPod(false); reload(); }} />
        </div>
      )}
    </div>
  );
}

function CreateDeliveryForm({ orderId, order, onCreated }: { orderId: string; order: OrderDto | null; onCreated: () => void }) {
  const { t } = useTranslation();
  const { data: farmers } = useFetch<FarmerDto[]>("/farmers");
  const { data: customers } = useFetch<CustomerDto[]>("/customers");

  const [originFarmerId, setOriginFarmerId] = useState("");
  const [customFarmSearch, setCustomFarmSearch] = useState(false);
  const [originLocation, setOriginLocation] = useState("");
  const [destinationCustomerId, setDestinationCustomerId] = useState("");
  const [destinationAddress, setDestinationAddress] = useState("");
  const [tripPrice, setTripPrice] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function onSelectFarmer(farmerId: string) {
    setOriginFarmerId(farmerId);
    const farmer = farmers?.find((f) => f.id === farmerId);
    setOriginLocation(farmer ? `${farmer.name}${farmer.location ? ` (${farmer.location})` : ""}` : "");
  }

  function onSelectCustomer(customerId: string) {
    setDestinationCustomerId(customerId);
    const customer = customers?.find((c) => c.id === customerId);
    setDestinationAddress(customer?.address || customer?.name || "");
  }

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      // No driver field here on purpose: every trip posts to the open board
      // with full details (origin, destination, price) and any registered
      // driver can freely choose to accept it or not - admin doesn't assign
      // a specific driver.
      await apiClient.post("/deliveries", {
        orderId,
        destinationAddress,
        originLocation: originLocation || null,
        tripPrice: tripPrice ? Number(tripPrice) : null
      });
      onCreated();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="card max-w-lg p-6">
      <h3 className="mb-1 text-base font-semibold text-evergreen-900">{t("deliveries.noDeliveryTitle")}</h3>
      <p className="mb-4 text-xs text-ink-300">{t("deliveries.openBoardHint")}</p>

      {order && (
        <div className="mb-4 rounded-md border border-evergreen-100 bg-evergreen-50/50 p-3 text-sm">
          <p className="font-medium text-evergreen-900">{order.orderNumber} - {order.customerName}</p>
          <p className="text-ink-600">{t("orders.title")} {t("commissions.baseAmount")}: {order.subtotal.toFixed(2)} {t("common.currency")}</p>
          {order.agentUserId && (
            <p className="text-ink-600">
              {t("deliveries.agentCommissionLabel")} ({order.agentUserName}): {" "}
              {order.estimatedAgentCommission != null
                ? `${order.estimatedAgentCommission.toFixed(2)} ${t("common.currency")} (${t("deliveries.estimated")})`
                : t("common.notApplicable")}
            </p>
          )}
        </div>
      )}

      <div className="space-y-4">
        <div>
          <label className="label">{t("deliveries.originLocation")}</label>
          {!customFarmSearch ? (
            <>
              <select className="input" value={originFarmerId} onChange={(e) => onSelectFarmer(e.target.value)}>
                <option value="">{t("deliveries.selectFarm")}</option>
                {(farmers ?? []).filter((f) => f.isActive).map((f) => (
                  <option key={f.id} value={f.id}>{f.name}{f.location ? ` - ${f.location}` : ""}</option>
                ))}
              </select>
              <button type="button" className="mt-1 text-xs text-evergreen-700 underline" onClick={() => setCustomFarmSearch(true)}>
                {t("deliveries.typeInsteadLink")}
              </button>
            </>
          ) : (
            <>
              <input className="input" value={originLocation} onChange={(e) => setOriginLocation(e.target.value)} placeholder={t("deliveries.originLocationPlaceholder")} />
              <button type="button" className="mt-1 text-xs text-evergreen-700 underline" onClick={() => setCustomFarmSearch(false)}>
                {t("deliveries.selectInsteadLink")}
              </button>
            </>
          )}
        </div>
        <div>
          <label className="label">{t("customers.title")}</label>
          <select className="input" value={destinationCustomerId} onChange={(e) => onSelectCustomer(e.target.value)}>
            <option value="">{t("deliveries.selectHotel")}</option>
            {(customers ?? []).filter((c) => c.isActive).map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">{t("deliveries.destinationAddress")}</label>
          <input className="input" value={destinationAddress} onChange={(e) => setDestinationAddress(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("deliveries.tripPrice")}</label>
          <input type="number" className="input" value={tripPrice} onChange={(e) => setTripPrice(e.target.value)} placeholder={t("deliveries.tripPriceHint")} />
          <p className="mt-1 text-xs text-ink-300">{t("deliveries.tripPriceIsDriverOnlyHint")}</p>
        </div>
      </div>

      {error && <div className="mt-4"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-4" onClick={submit} disabled={saving || !destinationAddress}>
        {saving ? t("common.creating") : t("deliveries.createDelivery")}
      </button>
    </div>
  );
}

/** Captures proof of delivery: recipient name, an on-screen signature, and GPS if the browser grants location access. */
function ProofOfDeliveryForm({ deliveryId, onDone }: { deliveryId: string; onDone: () => void }) {
  const { t } = useTranslation();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const drawing = useRef(false);
  const [receivedByName, setReceivedByName] = useState("");
  const [notes, setNotes] = useState("");
  const [coords, setCoords] = useState<{ lat: number; lng: number } | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) => setCoords({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
      () => setCoords(null),
      { timeout: 5000 }
    );
  }, []);

  function getPos(e: React.MouseEvent | React.TouchEvent) {
    const canvas = canvasRef.current!;
    const rect = canvas.getBoundingClientRect();
    const point = "touches" in e ? e.touches[0] : e;
    return { x: point.clientX - rect.left, y: point.clientY - rect.top };
  }

  function startDraw(e: React.MouseEvent | React.TouchEvent) {
    drawing.current = true;
    const ctx = canvasRef.current!.getContext("2d")!;
    const { x, y } = getPos(e);
    ctx.beginPath();
    ctx.moveTo(x, y);
  }

  function draw(e: React.MouseEvent | React.TouchEvent) {
    if (!drawing.current) return;
    const ctx = canvasRef.current!.getContext("2d")!;
    const { x, y } = getPos(e);
    ctx.lineTo(x, y);
    ctx.strokeStyle = "#1B1B18";
    ctx.lineWidth = 2;
    ctx.stroke();
  }

  function endDraw() {
    drawing.current = false;
  }

  function clearSignature() {
    const canvas = canvasRef.current!;
    canvas.getContext("2d")!.clearRect(0, 0, canvas.width, canvas.height);
  }

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      const signatureImageBase64 = canvasRef.current?.toDataURL("image/png");
      await apiClient.post(`/deliveries/${deliveryId}/delivered`, {
        notes: notes || null,
        receivedByName: receivedByName || null,
        signatureImageBase64,
        photoUrl: null,
        latitude: coords?.lat ?? null,
        longitude: coords?.lng ?? null
      });
      onDone();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="rounded-md border border-evergreen-100 bg-evergreen-50/40 p-4">
      <p className="mb-3 text-sm font-medium text-evergreen-900">{t("deliveries.proofOfDelivery")}</p>
      <div className="space-y-3">
        <div>
          <label className="label">{t("deliveries.receivedBy")}</label>
          <input className="input" value={receivedByName} onChange={(e) => setReceivedByName(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("common.notes")}</label>
          <input className="input" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>
        <div>
          <label className="label">{t("deliveries.signature")}</label>
          <canvas
            ref={canvasRef}
            width={320}
            height={120}
            className="w-full touch-none rounded-md border border-ink-300/40 bg-white"
            onMouseDown={startDraw}
            onMouseMove={draw}
            onMouseUp={endDraw}
            onMouseLeave={endDraw}
            onTouchStart={startDraw}
            onTouchMove={draw}
            onTouchEnd={endDraw}
          />
          <button className="btn-secondary mt-1 px-2 py-1 text-xs" onClick={clearSignature}>{t("common.cancel")}</button>
        </div>
        {coords && <p className="text-xs text-ink-300">GPS: {coords.lat.toFixed(5)}, {coords.lng.toFixed(5)}</p>}
      </div>

      {error && <div className="mt-3"><ErrorState message={error} /></div>}

      <button className="btn-primary mt-3" onClick={submit} disabled={saving}>
        {saving ? t("common.saving") : t("deliveries.markDelivered")}
      </button>
    </div>
  );
}
