// Mirrors the DTOs / enums exposed by FHSMS.API - keep in sync with the backend.

export type UserRole = "SuperAdmin" | "HotelAgent" | "FarmerAgent" | "HotelCustomer" | "PublicPortalUser" | "Driver";

export type TruckType = "Pickup" | "SmallTruck" | "MediumTruck" | "Isuzu" | "HeavyTruck" | "Trailer" | "Other";

export type TaxType = "Vat" | "SalesTax" | "WithholdingTax" | "Other";
export type TaxCalculationMode = "Exclusive" | "Inclusive";
export type TaxProfileType = "StandardVat" | "ZeroRated" | "TaxExempt" | "NoTax";

export type OrderStatus = "Draft" | "Pending" | "Confirmed" | "Preparing" | "Shipped" | "Delivered" | "Completed" | "Rejected" | "Cancelled" | "Returned";
export type OrderSourceType = "HotelPortal" | "HotelAgent" | "FarmerAgent" | "Telegram" | "PublicPortal";
export type InvoiceStatus = "Draft" | "Issued" | "PartiallyPaid" | "Paid" | "Cancelled";
export type PaymentMethod = "Cash" | "Bank" | "Telebirr" | "Chapa" | "CbeBirr" | "Credit";

export type InventoryTransactionType = "Receiving" | "Issuing" | "Adjustment" | "Damage" | "Wastage";
export type DeliveryStatus = "Pending" | "InTransit" | "Delivered" | "Failed";
export type AgentType = "HotelAgent" | "FarmerAgent";
export type CommissionStatus = "Accrued" | "Approved" | "Paid" | "Cancelled";
export type BankTransactionType = "Deposit" | "Withdrawal";
export type ReconciliationStatus = "Unmatched" | "Matched" | "Disputed";
export type WorkOrderStatus = "Pending" | "InProgress" | "Completed" | "Cancelled";
export type WorkOrderPriority = "Low" | "Normal" | "High" | "Urgent";

export interface LoginResult {
  token: string;
  refreshToken: string;
  fullName: string;
  role: string;
  requiresTwoFactor: boolean;
  pendingUserId?: string;
}

export interface SetupTwoFactorResult {
  secret: string;
  otpAuthUri: string;
}

export interface TaxConfigurationDto {
  id: string;
  name: string;
  taxType: TaxType;
  isEnabled: boolean;
  calculationMode: TaxCalculationMode;
  exemptionAllowed: boolean;
  currentRate?: number;
  currentRateEffectiveFrom?: string;
  pendingRate?: number;
  pendingRateEffectiveFrom?: string;
}

export interface TaxRateHistoryDto {
  id: string;
  rate: number;
  effectiveFrom: string;
  effectiveTo?: string;
}

export interface PlatformCommissionConfigurationDto {
  id: string;
  name: string;
  isEnabled: boolean;
  currentRate?: number;
  currentRateEffectiveFrom?: string;
  pendingRate?: number;
  pendingRateEffectiveFrom?: string;
}

export interface PlatformCommissionRateHistoryDto {
  id: string;
  rate: number;
  effectiveFrom: string;
  effectiveTo?: string;
}

export type PriceKind = "Buying" | "Selling";

export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description?: string;
  categoryId: string;
  categoryCode?: string;
  categoryName?: string;
  unitId: string;
  unitCode?: string;
  unitName?: string;
  unitAbbreviation?: string;
  // Both are role-filtered server-side (see ProductPriceVisibility on the
  // backend): a FarmerAgent session never receives currentSellingPrice, a
  // HotelAgent/HotelCustomer/PublicPortalUser session never receives
  // currentBuyingPrice. SuperAdmin and Driver receive both.
  currentBuyingPrice?: number;
  buyingPriceEffectiveFrom?: string;
  currentSellingPrice?: number;
  sellingPriceEffectiveFrom?: string;
  lowStockThreshold?: number;
  taxProfile: TaxProfileType;
  isActive: boolean;
}

export interface ProductPriceHistoryDto {
  id: string;
  price: number;
  kind: PriceKind;
  effectiveFrom: string;
  effectiveTo?: string;
  reason?: string;
  changedBy?: string;
}

export interface CategoryDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface UnitDto {
  id: string;
  code: string;
  name: string;
  abbreviation: string;
  isActive: boolean;
}

export interface CustomerDto {
  id: string;
  code: string;
  name: string;
  contactPerson?: string;
  phone?: string;
  email?: string;
  address?: string;
  creditLimit?: number;
  isActive: boolean;
}

export interface CustomerCreditStatusDto {
  customerId: string;
  creditLimit?: number;
  currentExposure: number;
  availableCredit?: number;
  overLimit: boolean;
  outstandingInvoiceCount: number;
}

export interface FarmerDto {
  id: string;
  code: string;
  name: string;
  contactPerson?: string;
  phone?: string;
  location?: string;
  bankAccountNumber?: string;
  isActive: boolean;
}

export interface UserDto {
  id: string;
  code?: string;
  fullName: string;
  email: string;
  role: UserRole;
  phone?: string;
  location?: string;
  isActive: boolean;
  isOnline: boolean;
  lastSeenAt?: string;
  createdAt: string;
}

export interface BankAccountDto {
  id: string;
  bankName: string;
  accountName: string;
  accountNumber: string;
  isActive: boolean;
}

export interface OrderItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  status: OrderStatus;
  source: OrderSourceType;
  orderDate: string;
  requestedDeliveryDate?: string;
  subtotal: number;
  notes?: string;
  agentUserId?: string;
  agentUserName?: string;
  estimatedAgentCommission?: number;
  deliveryStatus?: string;
  items: OrderItemDto[];
}

export type FarmerInvoiceStatus = "PendingApproval" | "Approved";

export type DriverPaymentStatus = "PendingApproval" | "Approved";

export interface DriverPaymentDto {
  id: string;
  deliveryId: string;
  orderId?: string;
  destinationAddress?: string;
  driverId?: string;
  driverName?: string;
  amount: number;
  status: DriverPaymentStatus;
  driverWasPaid: boolean;
  createdAt: string;
  approvedAt?: string;
}

export interface FarmerInvoiceDto {
  id: string;
  invoiceNumber: string;
  inventoryTransactionId: string;
  productId: string;
  productName?: string;
  unitAbbreviation?: string;
  farmerId?: string;
  farmerName?: string;
  agentUserId?: string;
  agentName?: string;
  quantity: number;
  buyingPriceApplied: number;
  totalAmount: number;
  status: FarmerInvoiceStatus;
  createdAt: string;
  approvedAt?: string;
}

export interface InvoiceItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineSubtotal: number;
  taxProfileApplied: TaxProfileType;
  taxRateApplied: number;
  taxableAmount: number;
  taxAmount: number;
  lineTotal: number;
}

export interface InvoiceDto {
  id: string;
  invoiceNumber: string;
  orderId: string;
  customerId: string;
  invoiceDate: string;
  status: InvoiceStatus;
  taxWasEnabled: boolean;
  taxMode?: TaxCalculationMode;
  subtotal: number;
  taxableAmount: number;
  taxAmount: number;
  discount: number;
  // Frozen at issue time - see Invoice.ApplyCompanyCharges on the backend.
  // GrandTotal = subtotal + taxAmount + platformCommissionAmount + hotelAgentBonusAmount - discount.
  platformCommissionRateApplied: number;
  platformCommissionAmount: number;
  hotelAgentBonusAmount: number;
  grandTotal: number;
  amountPaid: number;
  balanceDue: number;
  items: InvoiceItemDto[];
}

export interface RecordPaymentResultDto {
  paymentId: string;
  paymentNumber: string;
  receiptId: string;
  receiptNumber: string;
}

export interface ReceiptDto {
  id: string;
  receiptNumber: string;
  paymentId: string;
  invoiceId: string;
  amount: number;
  method: string;
  transactionReference?: string;
  issuedAt: string;
  issuedBy?: string;
}

export interface StockLevelDto {
  productId: string;
  productName: string;
  unitAbbreviation?: string;
  quantityOnHand: number;
}

export interface InventoryTransactionDto {
  id: string;
  productId: string;
  productName?: string;
  type: InventoryTransactionType;
  quantityChange: number;
  reference?: string;
  notes?: string;
  createdAt: string;
  farmerId?: string;
  farmerName?: string;
  agentUserId?: string;
  agentName?: string;
  isConfirmed: boolean;
  confirmedAt?: string;
  farmerPaymentConfirmed: boolean;
  amountPaidToFarmer?: number;
  // The auto-generated farmer-side invoice for this Receiving transaction - undefined for other transaction types.
  farmerInvoiceId?: string;
  farmerInvoiceNumber?: string;
  farmerInvoiceStatus?: FarmerInvoiceStatus;
  farmerInvoiceTotalAmount?: number;
}

export interface DeliveryDto {
  id: string;
  orderId: string;
  orderNumber?: string;
  destinationAddress: string;
  originLocation?: string;
  tripPrice?: number;
  driverId?: string;
  driverName?: string;
  vehicleInfo?: string;
  driverConfirmed: boolean;
  recipientConfirmed: boolean;
  recipientConfirmedAt?: string;
  status: DeliveryStatus;
  dispatchedAt?: string;
  deliveredAt?: string;
  receivedByName?: string;
  hasSignature: boolean;
  photoUrl?: string;
  deliveryLatitude?: number;
  deliveryLongitude?: number;
}

export interface DriverDto {
  id: string;
  code: string;
  userId: string;
  fullName: string;
  phone?: string;
  plateNumber?: string;
  truckType: TruckType;
  isActive: boolean;
}

export interface TripDto {
  deliveryId: string;
  orderId: string;
  orderNumber: string;
  originLocation?: string;
  destinationAddress: string;
  destinationName: string;
  productSummary: string;
  totalQuantity: number;
  tripPrice?: number;
  orderDate: string;
  status: string;
  driverId?: string;
  driverConfirmed: boolean;
}

export interface DriverEarningsDto {
  totalEarned: number;
  thisMonthEarned: number;
  pendingTripValue: number;
  completedTripCount: number;
  pendingTripCount: number;
}

export interface AdminDriverDto {
  id: string;
  code: string;
  fullName: string;
  phone?: string;
  plateNumber?: string;
  truckType: TruckType;
  isActive: boolean;
  pendingTripCount: number;
  completedTripCount: number;
  totalEarned: number;
}

export type CommissionBasis = "PercentageOfInvoice" | "FlatRatePerQuantity";
export type CommissionSourceType = "Invoice" | "StockReceipt";

export interface CommissionDto {
  id: string;
  agentUserId: string;
  sourceType: CommissionSourceType;
  invoiceId?: string;
  inventoryTransactionId?: string;
  basis: CommissionBasis;
  baseAmount: number;
  percentage?: number;
  flatRateAmount?: number;
  commissionAmount: number;
  status: CommissionStatus;
  createdAt: string;
}

export interface CommissionUnitRateDto {
  unitId: string;
  unitName: string;
  unitAbbreviation: string;
  rateAmount: number;
}

export interface CommissionRuleDto {
  id: string;
  agentType: AgentType;
  basis: CommissionBasis;
  percentage?: number;
  flatRateAmount?: number;
  isActive: boolean;
  unitRates: CommissionUnitRateDto[];
}

export interface BankTransactionDto {
  id: string;
  bankAccountId: string;
  type: BankTransactionType;
  amount: number;
  transactionDate: string;
  description?: string;
  reconciliationStatus: ReconciliationStatus;
  matchedPaymentId?: string;
}

export interface AuditLogDto {
  id: string;
  entityName: string;
  entityId: string;
  action: string;
  performedBy?: string;
  performedAt: string;
  changes?: string;
}

export interface WorkOrderDto {
  id: string;
  title: string;
  description?: string;
  orderId?: string;
  assignedToUserId?: string;
  status: WorkOrderStatus;
  priority: WorkOrderPriority;
  dueDate?: string;
  completedAt?: string;
  createdAt: string;
}

export interface SalesSummaryDto {
  invoiceCount: number;
  totalRevenue: number;
  totalTaxCollected: number;
  totalDiscount: number;
  averageInvoiceValue: number;
  totalOutstanding: number;
  orderCount: number;
  cancelledOrderCount: number;
  totalCommission: number;
  commissionRatePercent?: number;
  agentBonus: number;
  hotelAgentBonusTotal: number;
  farmerAgentBonusTotal: number;
  productSalesRevenue: number;
  amountPaidToFarmers: number;
  grossProfitOnGoods: number;
  netProfit: number;
  netProfitFormula: string;
  driverTripCost: number;
}

export interface RevenuePeriodDto {
  period: string;
  revenue: number;
  taxCollected: number;
  invoiceCount: number;
}

export interface TopProductDto {
  productId: string;
  productName: string;
  quantitySold: number;
  revenue: number;
}

export interface CommissionSummaryDto {
  agentType: string;
  commissionCount: number;
  totalCommission: number;
}

export interface TopCustomerDto {
  customerId: string;
  customerName: string;
  quantityKg: number;
  revenue: number;
}

export interface NotificationDto {
  id: string;
  channel: string;
  subject: string;
  body: string;
  status: string;
  sentAt?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface RegisteredAgentDto {
  id: string;
  code?: string;
  fullName: string;
  role: string;
  phone?: string;
  location?: string;
  isActive: boolean;
  isOnline: boolean;
  todayOrderedKg: number;
  totalCommissionEarned: number;
  unpaidCommission: number;
}

export interface AgentsManagementSummaryDto {
  dailyOrderCount: number;
  grossRevenue: number;
  taxCollected: number;
  totalCommission: number;
  commissionRatePercent?: number;
  agentBonus: number;
  hotelAgentBonusTotal: number;
  farmerAgentBonusTotal: number;
  productSalesRevenue: number;
  amountPaidToFarmers: number;
  grossProfitOnGoods: number;
  netProfit: number;
  driverTripCost: number;
  agents: RegisteredAgentDto[];
}

export interface PublicStatsDto {
  registeredFarmers: number;
  registeredHotels: number;
  avgDailyKgDelivered: number;
  serviceSatisfactionPercent?: number;
}
