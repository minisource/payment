/**
 * Centralized permission constants for Payment Admin Frontend.
 *
 * Every permission string used in the UI should be defined here.
 * This prevents scattered permission strings across components/pages.
 *
 * Backend remains the source of truth — these are UX-only guards.
 */
export const PaymentPermissions = {
  // ── Wallet ──────────────────────────────────────────────
  WalletViewAdmin: 'payment.wallet.view_admin',
  WalletManage: 'WALLET_MANAGE',

  // ── Payment Intent ─────────────────────────────────────
  IntentViewAdmin: 'payment.intent.view_admin',
  IntentManage: 'PAYMENT_MANAGE',

  // ── Payment Transaction ────────────────────────────────
  TransactionViewAdmin: 'payment.transaction.view_admin',

  // ── Gateway Config ─────────────────────────────────────
  GatewayConfigView: 'payment.gateway.config.view',
  GatewayConfigManage: 'payment.gateway.config.manage',

  // ── Payment Link ───────────────────────────────────────
  PaymentLinkViewAdmin: 'payment.payment_link.view_admin',

  // ── Withdrawal ─────────────────────────────────────────
  WithdrawalViewAdmin: 'payment.withdrawal.view_admin',

  // ── Payout Account ─────────────────────────────────────
  PayoutAccountViewAdmin: 'payment.payout_account.view_admin',

  // ── Risk ───────────────────────────────────────────────
  RiskView: 'payment.risk.view',
  RiskManage: 'payment.risk.manage',

  // ── Reconciliation ─────────────────────────────────────
  ReconciliationView: 'payment.reconciliation.view',

  // ── Admin Approval ─────────────────────────────────────
  AdminApprovalView: 'payment.admin_approval.view',
  AdminApprovalDecide: 'payment.admin_approval.decide',

  // ── Outbox ─────────────────────────────────────────────
  OutboxViewAdmin: 'payment.outbox.view_admin',

  // ── Webhook ────────────────────────────────────────────
  WebhookViewAdmin: 'payment.webhook.view_admin',
  WebhookAdmin: 'payment.webhook.create_admin',

  // ── Audit ──────────────────────────────────────────────
  AuditViewAdmin: 'payment.audit.view_admin',

  // ── Reports ────────────────────────────────────────────
  ReportsViewAdmin: 'payment.security.reports.view_admin',

  // ── Settings ───────────────────────────────────────────
  SettingsRead: 'SETTINGS_READ',
  SettingsManage: 'SETTINGS_MANAGE',

  // ── Diagnostics ───────────────────────────────────────
  DiagnosticsView: 'payment.diagnostics.view',
} as const;

export type PaymentPermission = (typeof PaymentPermissions)[keyof typeof PaymentPermissions];
