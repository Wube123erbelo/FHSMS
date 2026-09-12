import { Routes, Route } from "react-router-dom";
import ProtectedRoute from "./components/ProtectedRoute";
import Layout from "./components/Layout";
import LoginPage from "./pages/auth/LoginPage";
import LandingPage from "./pages/landing/LandingPage";
import ForgotPasswordPage from "./pages/auth/ForgotPasswordPage";
import ResetPasswordPage from "./pages/auth/ResetPasswordPage";
import ChangePasswordPage from "./pages/auth/ChangePasswordPage";
import TwoFactorSettingsPage from "./pages/auth/TwoFactorSettingsPage";
import DashboardPage from "./pages/dashboard/DashboardPage";
import ProductsPage from "./pages/products/ProductsPage";
import CategoriesPage from "./pages/categories/CategoriesPage";
import UnitsPage from "./pages/units/UnitsPage";
import CustomersPage from "./pages/customers/CustomersPage";
import FarmersPage from "./pages/farmers/FarmersPage";
import OrdersPage from "./pages/orders/OrdersPage";
import OrderDetailPage from "./pages/orders/OrderDetailPage";
import InvoicesPage from "./pages/invoices/InvoicesPage";
import InvoiceDetailPage from "./pages/invoices/InvoiceDetailPage";
import FarmerInvoicesPage from "./pages/farmer-invoices/FarmerInvoicesPage";
import InventoryPage from "./pages/inventory/InventoryPage";
import DeliveriesPage from "./pages/deliveries/DeliveriesPage";
import WorkOrdersPage from "./pages/workorders/WorkOrdersPage";
import CommissionsPage from "./pages/commissions/CommissionsPage";
import BankReconciliationPage from "./pages/bank/BankReconciliationPage";
import NotificationsPage from "./pages/notifications/NotificationsPage";
import AuditLogsPage from "./pages/audit/AuditLogsPage";
import TaxSettingsPage from "./pages/tax/TaxSettingsPage";
import TotalCommissionSettingsPage from "./pages/commission-settings/TotalCommissionSettingsPage";
import SettingsPage from "./pages/settings/SettingsPage";
import UsersPage from "./pages/users/UsersPage";
import ReportsPage from "./pages/reports/ReportsPage";
import DriverPortalPage from "./pages/drivers/DriverPortalPage";
import DriverPaymentsPage from "./pages/driver-payments/DriverPaymentsPage";
import AdminDriversPage from "./pages/admin-drivers/AdminDriversPage";
import AgentsManagementPage from "./pages/agents-management/AgentsManagementPage";

export default function App() {
  return (
    <Routes>
      <Route path="/welcome" element={<LandingPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          {/* Available to every logged-in role */}
          <Route path="/" element={<DashboardPage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
          <Route path="/two-factor" element={<TwoFactorSettingsPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/notifications" element={<NotificationsPage />} />

          {/* Ordering is a hotel-side concern - a farmer agent's job is stock-in, not placing hotel orders */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "HotelAgent", "HotelCustomer"]} />}>
            <Route path="/orders" element={<OrdersPage />} />
            <Route path="/orders/:orderId" element={<OrderDetailPage />} />
          </Route>

          {/* Hotel-side: SuperAdmin + HotelAgent (registers hotels, takes their orders, sees invoices) */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "HotelAgent"]} />}>
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/invoices" element={<InvoicesPage />} />
            <Route path="/invoices/:invoiceId" element={<InvoiceDetailPage />} />
          </Route>

          {/* Farm-side: SuperAdmin + FarmerAgent (registers farmers, logs stock received) */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "FarmerAgent"]} />}>
            <Route path="/farmers" element={<FarmersPage />} />
            <Route path="/inventory" element={<InventoryPage />} />
          </Route>

          {/* Farm-side buying invoices: SuperAdmin + FarmerAgent + Driver (mirrors FarmerInvoicesController's Authorize) */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "FarmerAgent", "Driver"]} />}>
            <Route path="/farmer-invoices" element={<FarmerInvoicesPage />} />
          </Route>

          {/* Either kind of agent earns commission and can see their own */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "HotelAgent", "FarmerAgent"]} />}>
            <Route path="/commissions" element={<CommissionsPage />} />
          </Route>

          {/* Driver-side: Driver only - this is the self-registration + trip board screen, not for admin */}
          <Route element={<ProtectedRoute roles={["Driver"]} />}>
            <Route path="/driver-portal" element={<DriverPortalPage />} />
          </Route>

          {/* Driver payments: SuperAdmin + Driver (mirrors DriverPaymentsController's Authorize) */}
          <Route element={<ProtectedRoute roles={["SuperAdmin", "Driver"]} />}>
            <Route path="/driver-payments" element={<DriverPaymentsPage />} />
          </Route>

          {/* Admin-only: system configuration and back-office records */}
          <Route element={<ProtectedRoute roles={["SuperAdmin"]} />}>
            <Route path="/drivers" element={<AdminDriversPage />} />
            <Route path="/deliveries" element={<DeliveriesPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/units" element={<UnitsPage />} />
            <Route path="/work-orders" element={<WorkOrdersPage />} />
            <Route path="/bank-reconciliation" element={<BankReconciliationPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
            <Route path="/tax-settings" element={<TaxSettingsPage />} />
            <Route path="/commission-settings" element={<TotalCommissionSettingsPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/agents-management" element={<AgentsManagementPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
}
