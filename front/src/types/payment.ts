export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  skip: number;
  take: number;
}

export interface ApiError {
  error: {
    code: string;
    message: string;
    user_message?: string;
    category?: string;
    details?: Record<string, unknown>;
    field_errors?: Array<{ field: string; message: string }>;
    request_id?: string;
    correlation_id?: string;
  };
}

// Wallet types
export interface WalletAccount {
  id: string;
  tenantId: string;
  ownerType: string;
  ownerId: string;
  currency: string;
  availableBalance: number;
  lockedBalance: number;
  pendingBalance: number;
  status: string;
  version: number;
  createdAt: string;
  updatedAt: string;
}

export interface LedgerEntry {
  id: string;
  walletAccountId: string;
  tenantId: string;
  entryType: string;
  direction: string;
  amount: number;
  currency: string;
  balanceAvailableBefore: number;
  balanceAvailableAfter: number;
  balanceLockedBefore: number;
  balanceLockedAfter: number;
  balancePendingBefore: number;
  balancePendingAfter: number;
  reason: string | null;
  referenceType: string | null;
  referenceId: string | null;
  previousEntryHash: string;
  entryHash: string;
  createdAt: string;
}

// Payment types
export interface PaymentIntent {
  id: string;
  tenantId: string;
  amount: number;
  currency: string;
  status: string;
  providerCode: string | null;
  description: string | null;
  externalReferenceType: string | null;
  externalReferenceId: string | null;
  walletBehavior: string;
  returnUrl: string | null;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PaymentTransaction {
  id: string;
  tenantId: string;
  paymentIntentId: string;
  gatewayConfigId: string;
  providerCode: string;
  status: string;
  amount: number;
  currency: string;
  authority: string | null;
  gatewayReferenceId: string | null;
  traceNumber: string | null;
  rrn: string | null;
  cardPanMasked: string | null;
  responseCode: string | null;
  verifiedAt: string | null;
  createdAt: string;
}

// Gateway types
export interface GatewayProvider {
  id: string;
  code: string;
  displayName: string;
  adapterType: string;
  enabled: boolean;
}

export interface GatewayConfig {
  id: string;
  tenantId: string | null;
  providerCode: string;
  name: string;
  adapterType: string;
  status: string;
  environment: string;
  priority: number;
  weight: number;
  minAmount: number | null;
  maxAmount: number | null;
  currencies: string[];
  callbackBaseUrl: string | null;
  createdAt: string;
}

// Withdrawal types
export interface WithdrawalRequest {
  id: string;
  tenantId: string;
  walletId: string;
  payoutAccountId: string;
  amount: number;
  currency: string;
  status: string;
  feeAmount: number;
  netAmount: number;
  ownerType: string;
  ownerId: string;
  bankTrackingNumber: string | null;
  bankReferenceId: string | null;
  adminNote: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PayoutAccount {
  id: string;
  tenantId: string;
  accountType: string;
  currency: string;
  holderName: string;
  bankName: string | null;
  cardNumberMasked: string | null;
  iban: string | null;
  accountNumberMasked: string | null;
  status: string;
  createdAt: string;
}

// Webhook types
export interface WebhookSubscription {
  id: string;
  tenantId: string;
  applicationCode: string | null;
  name: string;
  url: string;
  status: string;
  eventTypes: string[];
  maxRetryCount: number;
  timeoutSeconds: number;
  createdAt: string;
}

export interface WebhookDelivery {
  id: string;
  tenantId: string;
  outboxEventId: string;
  webhookSubscriptionId: string;
  eventType: string;
  status: string;
  attemptCount: number;
  lastStatusCode: number | null;
  lastErrorMessage: string | null;
  nextAttemptAt: string | null;
  deliveredAt: string | null;
  createdAt: string;
}

// Outbox types
export interface OutboxEvent {
  id: string;
  tenantId: string | null;
  eventType: string;
  eventVersion: number;
  aggregateType: string;
  aggregateId: string;
  status: string;
  occurredAt: string;
  retryCount: number;
  nextRetryAt: string | null;
  errorMessage: string | null;
  deadLetteredAt: string | null;
}

// Security types
export interface RiskCase {
  id: string;
  tenantId: string;
  entityType: string;
  entityId: string;
  severity: string;
  status: string;
  description: string;
  openedAt: string;
  resolvedAt: string | null;
}

export interface ReconciliationBatch {
  id: string;
  tenantId: string;
  reconciliationType: string;
  status: string;
  mismatchCount: number;
  resolvedCount: number;
  createdAt: string;
  completedAt: string | null;
}

export interface ApprovalRequest {
  id: string;
  tenantId: string;
  operationType: string;
  requestedByUserId: string;
  status: string;
  payload: string;
  requestedAt: string;
  decidedAt: string | null;
}

// Audit types
export interface AuditLog {
  id: string;
  tenantId: string | null;
  actorUserId: string | null;
  action: string;
  entityType: string;
  entityId: string;
  reason: string | null;
  ipAddress: string | null;
  correlationId: string | null;
  createdAt: string;
}

// Dashboard types
export interface DashboardOverview {
  totalWallets: number;
  walletsWithIssues: number;
  openRiskCases: number;
  criticalRiskCases: number;
  unresolvedRecItems: number;
  pendingApprovals: number;
  liability: Array<{ currency: string; availableTotal: number; lockedTotal: number }>;
}

export interface EventOverview {
  outbox: Record<string, number>;
  webhooks: Record<string, number>;
  subscriptions: { active: number; disabled: number };
  notifier: { enabled: boolean; published: number; failed: number };
  message_bus: { enabled: boolean; published: number; failed: number };
}
