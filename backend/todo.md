# Payment Backend — Remaining Tasks & Improvements

> Auto-generated audit of all remaining work, bugs, stubs, and refinements across the payment service.
> Generated: 2026-07-07 | Based on code analysis of 136 C# files across Domain, Application, Infrastructure, and API layers.

---

## 🔴 Critical Runtime Bugs

These are bugs that will cause incorrect behavior or data loss at runtime.

| # | Bug | Location | Severity |
|---|---|---|---|
| 1 | ✅ | **Risk evaluations persisted to DB** | `RiskEvaluationService.cs` | FIXED |
| | RiskEvaluation and RiskCase now saved via PaymentDbContext.AddAsync + SaveChangesAsync. FK relationship fixed with RiskEvaluation.RiskCaseId property. | | |
| 2 | ✅ | **AdminApprovalService uses DbContext (not in-memory)** | `AdminApprovalService.cs` | FIXED |
| | Replaced `List<AdminApprovalRequest> _requests = []` with `PaymentDbContext`. All CRUD operations now persist to DB. | | |
| 3 | ✅ | **RiskEvaluationService FK race fixed** | `RiskEvaluationService.cs` | FIXED |
| | Both entities persisted BEFORE setting FK references. RiskEvaluation.RiskCaseId FK property added to entity. | | |
| 4 | ✅ | **LedgerHashService backfill uses implicit tracking** | `LedgerHashService.cs` | FALSE ALARM |
| | Entities are modified in-place and saved via SaveChangesAsync. Repository must use tracking (NOT AsNoTracking). Working correctly. | | |
| 5 | ✅ | **WebhookDelivery records created during dispatch** | `OutboxDispatcherHostedService.cs` | FIXED |
| | Dispatcher now looks up active subscriptions, creates WebhookDelivery record per subscription, and tracks delivery status (delivered/failed/dead_lettered). | | |
| 6 | ✅ | **Webhook dispatcher delivers to ALL matching subscriptions** | `OutboxDispatcherHostedService.cs` | FIXED |
| | PublishToWebhooksAsync queries all active subscriptions matching the event (by tenant, application, event_type). Creates separate delivery per subscription. | | |
| 7 | ✅ | **Event routing implemented** | `OutboxDispatcherHostedService.cs` | FIXED |
| | Dispatcher now checks EventRoutingRule before publishing to each publisher. Default: allow all. Rules: only matching target+event_type. | | |

---

## 🟠 Incomplete Implementations (Stubs / TODOs)

Features that exist as skeletons but lack real logic.

| # | Item | Location | What's Missing |
|---|---|---|---|
| 8 | ✅ | **ParbadGatewayAdapter wired to real IOnlinePayment** | `ParbadGatewayAdapter.cs` | FIXED |
| 9 | **StubUserComplianceService returns hardcoded "KYC passed"** | `RiskEvaluationService.cs:105-111` | Always returns `KycCompleted=true, IsBlocked=false`. No real integration with Auth/User service. KYC/AML checks are non-functional. |
| 10 | ✅ | **FinancialSecurityReportsController wired to real data** | `Controllers/FinancialSecurityReportsController.cs` | FIXED |
| | All 7 endpoints now query actual DB: wallet counts, risk cases, reconciliation items, approval requests, limit usage, ledger integrity checks. Liability by currency included. | | |
| 11 | **ResultPageController — no HTML views** | `Controllers/ResultPageController.cs:42,61` | Two TODOs: "Add Razor views for HTML result page". Payment success/failure pages return JSON only. |
| 12 | **AdminPaymentLinkController — cursor pagination placeholder** | `Controllers/AdminPaymentLinkController.cs:37` | Uses basic skip/take, TODO says "Use GetByTenantAsync with cursor-based pagination". |
| 13 | ✅ | **WithdrawalService/PayoutAccountService — null tenant now throws** | `WithdrawalService.cs`, `PayoutAccountService.cs` | FIXED |
| | AdminListAsync/GetSummaryAsync/GetPendingAsync now throw ValidationException instead of silently returning empty lists. | | |
| 14 | ✅ | **SettingsService DTO mapping complete for Phase 5 fields** | `SettingsService.cs` + `SettingsDtos.cs` | FIXED |
| | Added 9 withdrawal fields to both TenantPaymentSettingsDto and UpdateTenantPaymentSettingsRequest. SettingsService maps all new fields in Update and Get operations. | | |
| 15 | ✅ | **PaymentDbContext precision added for TenantPaymentSettings** | `PaymentDbContext.cs` | FIXED |
| | `.HasPrecision(30, 10)` added for MinWithdrawalAmount, MaxWithdrawalAmount, WithdrawalDailyAmountLimit, WithdrawalMonthlyAmountLimit. | | |
| 16 | ✅ | **OutboxEvent ApplicationCode column added** | `InfrastructureEntities.cs` + `PaymentDbContext.cs` + repo | FIXED |
| | ApplicationCode property added to entity, DbContext config (HasMaxLength, index), and repository ListAsync/CountAsync now support filtering by it. | | |
| 17 | ✅ | **WalletConsistencyService full checks added** | `WalletConsistencyService.cs` | FIXED |
| 18 | ✅ | **DiagnosticsController DeepCheck enhanced (via WalletConsistencyService)** | `DiagnosticsController.cs` | FIXED |
| 19 | ✅ | **EventReportsController wired to real data** | `EventReportsController.cs` | FIXED |

---

## 🟡 Missing Services (Phase 5 Spec Items Never Created)

These were specified in Phase 5 but no service was implemented.

| # | Missing Service | Spec Reference | Notes |
|---|---|---|---|
| 20 | ✅ | **Reconciliation Service implemented** | `ReconciliationService.cs` (NEW) | FIXED |
| | Four reconciliation types: wallet_balance (ledger replay vs wallet), gateway_payments (verified txn vs wallet posting), withdrawal_payouts (paid vs payout record), payment_wallet_posting (succeeded intent vs posting). Creates batches + items. | | |
| 21 | ✅ | **Limit Usage Service implemented** | `LimitUsageService.cs` (NEW) | FIXED |
| | Tracks daily/monthly count and amount via PaymentLimitUsage with atomic UPSERT. Reads limits from TenantPaymentSettings. Wired into WithdrawalService.CreateAsync for check+track. | | |
| 22 | ✅ | **Bank Statement Import Service implemented** | `BankStatementImportService.cs` (NEW) | FIXED |
| | CSV import with configurable parser (date, amount, currency, direction, description, tracking, counterparty). Entry matching/ignore with audit logging. Import listing and entry retrieval. | | |
| 23 | **MediatR Event Handlers** | Phase 5 Part L | Domain events (`WalletBalanceChangedEvent`, etc.) are raised via `RaiseDomainEvent()` but there are no MediatR `INotificationHandler` implementations. No events are dispatched to outbox or handlers. |
| 24 | ✅ | **Outbox event creation from domain events** | `UnitOfWork.cs` | FIXED |
| | UnitOfWork.SavechangesAsync now collects domain events from IHasDomainEvents entities, creates OutboxEvent records atomically in the same transaction. Domain events are bridged to outbox. | | |
| 25 | ✅ | **RejectAsync TOCTOU fixed** | `WithdrawalService.cs` | FIXED |
| | RejectAsync now re-reads withdrawal after Redis lock + guards against Paid/Cancelled status (matching CancelAsync pattern). | | |

---

## 🔵 In-Memory / Non-Persistent Stores

Data that is not persisted to database and will be lost on restart.

| # | Item | Location |
|---|---|---|
| 26 | ✅ | **AdminApprovalService** — in-memory store replaced | `AdminApprovalService.cs` |
| 27 | ✅ | **ParbadGatewayAdapter** — stub replaced with real Parbad IOnlinePayment wire | `ParbadGatewayAdapter.cs` |
| 28 | ✅ | **RiskEvaluationService** — evaluations/cases now saved | `RiskEvaluationService.cs` |
| 29 | **StubUserComplianceService** — hardcoded compliance | `RiskEvaluationService.cs:109` |
| 30 | **Risk Evaluation rules (in Parbad PaymentExtension)** — gateway configs use `AddInMemory` | `PaymentExtension.cs:28,41,54` |

---

## ⚪ Missing Wiring

Interfaces and implementations exist but are not connected.

| # | Item | Details |
|---|---|---|
| 31 | ✅ | **IUserComplianceService wired into WithdrawalService** | WithdrawalService.CreateAsync now checks KYC/blocked/high-risk compliance status before processing withdrawals. | | |
| 32 | ✅ | **IRiskEvaluationService wired into WithdrawalService** | WithdrawalService.CreateAsync now calls EvaluateAsync and blocks on "block" decision. | | |
| 33 | ✅ | **IAdminApprovalService wired into AdminWalletService** | AdminWalletService creates approval requests for high-amount admin credit/debit (above threshold). | | |
| 34 | ✅ | **Domain events dispatched to outbox** | UnitOfWork.SavechangesAsync collects and persists domain events as outbox events atomically. | | |
| 35 | **No outbox event creation** | `IOutboxEventRepository.AddAsync` exists but is never called to create outbox events from business operations. |
| 36 | ✅ | **ILedgerHashService auto-computes on ledger creation** | `WalletLedgerRepository.cs` | FIXED |
| | WalletLedgerRepository.AddAsync now auto-computes hash chain (previous hash + entry hash) for tamper-evident ledger. | | |
| 37 | ✅ | **Audit hash auto-populate on creation** | `InfrastructureRepositories.cs` | FIXED |
| | AuditLog.AddAsync now auto-computes SHA256 hash chain (previous hash from DB) for tamper-evident audit trail. | | |

---

## 📋 Tests

| # | Item | Details |
|---|---|---|
| 38 | **Only 6 test files, mostly placeholders** | `PaymentServiceTests.cs` — `Assert.True(true); // Placeholder`. `WalletServiceTests.cs` — same. Tests aren't validating any real behavior. |
| 39 | **No tests for any Phase 1-6 feature** | Wallet, ledger, payment intents, gateways, public links, withdrawals, payout accounts, hash chains, reconciliation, risk engine, outbox, webhooks — ZERO real tests. |
| 40 | **E2E tests minimal** | `PaymentsApiE2ETests.cs` — likely placeholder like the others. |
| 41 | **No integration tests** | No tests for DB transactions, Redis locking, outbox dispatch, webhook delivery. |

---

## 📖 Documentation

| # | Item | Details |
|---|---|---|
| 42 | **Phase 5 docs never created** | Spec listed 8 docs: `financial-security.md`, `ledger-integrity.md`, `reconciliation.md`, `risk-monitoring.md`, `admin-approval-workflow.md`, `bank-statement-import.md`, `compliance-limits.md`, `security-reports.md`. None created. |
| 43 | **Phase 6 docs never created** | Spec listed 9 docs: `events.md`, `outbox-dispatcher.md`, `webhooks.md`, `webhook-signing.md`, `notifier-integration.md`, `message-bus.md`, `event-routing.md`, `event-contracts.md`, `event-delivery-troubleshooting.md`. None created. |
| 44 | **No API contract documentation** | No OpenAPI/Swagger annotations on any controller endpoints. Swagger UI works structurally but has no endpoint descriptions, parameter docs, or example payloads. |
| 45 | **No architecture diagram updates** | README and existing architecture docs may not reflect Phase 3-6 additions. |
| 46 | **No webhook signing verification docs for consumers** | External consumers need docs on how to verify `X-MiniSource-Signature`. |

---

## 🔧 Refactoring

| # | Item | Details |
|---|---|---|
| 47 | ✅ | **OutboxAdminController.RetryEvent uses ExecuteUpdateAsync** | `OutboxAdminController.cs` + `InfrastructureRepositories.cs` + `WalletRepositories.cs` | FIXED |
| 48 | ✅ | **InfrastructureRepositories god class split** | Split into 4 files: SettingsRepository, IdempotencyRecordRepository, AuditLogRepository, OutboxEventRepository | FIXED |
| 49 | ✅ | **WalletRepositories.cs split** | Moved Settings/Idempotency/Audit/Outbox interfaces to InfrastructureRepositories.cs. Wallet file now contains only wallet interfaces. | FIXED |
| 50 | ✅ | **Controllers refactored to use repos** | WebhooksAdminController now uses IWebhookSubscriptionRepository + IWebhookDeliveryRepository. EventReportsController uses IOutboxEventRepository + repos + PaymentDbContext for complex report queries. | FIXED |
| 51 | **No cancellation token propagation** | Many async methods accept `CancellationToken` but don't pass it to all inner calls (especially DB queries). |
| 52 | ✅ | **Granular permissions added** | `Program.cs` | FIXED |
| 53 | ✅ | **LedgerHash non-nullable** | `WalletLedgerEntry.cs` | FIXED |
| 54 | ✅ | **Jitter added to outbox retry backoff** | `OutboxDispatcherHostedService.cs` | FIXED |
| | ComputeBackoff now adds ±25% random jitter to prevent thundering herd on retry. | | |
| 55 | **OutboxEvent.Headers stored as raw string** | Should be typed as `JsonDocument` or at minimum use `System.Text.Json` consistently. Currently parsed ad-hoc in multiple places. |

---

## 🔒 Security

| # | Item | Details |
|---|---|---|
| 56 | ✅ | **Webhook URL HTTPS enforced** | `WebhooksAdminController.cs` | FIXED |
| 57 | **No webhook IP allowlist** | Optional but recommended per spec. |
| 58 | ✅ | **Rate limiting on admin retry endpoints** | `OutboxAdminController.cs` + `RequestRateLimitAttribute.cs` (NEW) | FIXED |
| 59 | **Payload not redacted before external publish** | `WebhookEventPublisher` sends the raw outbox `Payload` to webhook consumers. No field-level redaction of PII/sensitive data. |
| 60 | **No access token/secret logging prevention** | No guard against accidentally logging webhook secrets or gateway secrets. |
| 61 | **Webhook secret returned only once — but not enforced in transport** | Secret is returned once in create/regenerate response, but the controller doesn't strip it from serialized responses in other endpoints. `SafeSubscription()` handles this, but review all code paths. |
| 62 | **No HTTPS-only cookie/response flagging** | Not specifically a payment issue, but worth auditing. |

---

## 📊 Summary

| Category | Count | Fixed |
|---|---|---|
| Critical Runtime Bugs | 7 | 6 ✅ |
| Incomplete Stubs/TODOs | 12 | 7 ✅ |
| Missing Services | 6 | 2 ✅ |
| In-Memory/Non-Persistent | 5 | 3 ✅ |
| Missing Wiring | 7 | 6 ✅ |
| Tests | 4 | 0 |
| Documentation | 5 | 0 |
| Refactoring | 9 | 6 ✅ |
| Security | 7 | 2 ✅ |
| **Total** | **62** | **32 fixed** |

> Last updated: 2026-07-07 — 29 of 62 items resolved.

---

## 🎯 Suggested Priority Order

1. **Fix Critical Bugs** (#1, #2, #4, #5, #6) — data loss and non-functional features
2. **Wire existing services** (#31, #32, #33, #34) — connect DI-registered services to business flows
3. **Create missing services** (#20, #21, #22) — reconciliation, limit usage, bank statement import
4. **Implement outbox event creation** (#24) — bridge domain events → outbox events
5. **Wire Parbad gateway** (#8) — payment gateway is non-functional
6. **Add tests** (#38-41) — 6 placeholder tests won't catch regressions
7. **Add Swagger annotations** (#44) — document all endpoints
8. **Security hardening** (#56-62) — before production deployment
9. **Docs** (#42-43) — user-facing and developer documentation
10. **Refactoring** (#47-55) — tech debt cleanup
