/**
 * Centralized bilingual translations for the Payment Admin Frontend.
 * Uses the same fa/en language detection as shared/i18n/language.ts.
 *
 * Usage: import { t } from '@/shared/i18n/translations';
 *        t('error.somethingWentWrong') // returns fa or en based on resolveLanguage()
 */

import { resolveLanguage } from './language';

type Lang = 'fa' | 'en';

// ─── Translation Maps ─────────────────────────────────

const translations: Record<string, Record<Lang, string>> = {
  // ── Error States ──────────────────────────────────
  'error.somethingWentWrong':       { fa: 'خطایی رخ داد',                          en: 'Something went wrong' },
  'error.unexpectedError':          { fa: 'یک خطای غیرمنتظره رخ داد',               en: 'An unexpected error occurred' },
  'error.tryAgain':                 { fa: 'تلاش مجدد',                             en: 'Try Again' },
  'error.notFound':                 { fa: 'پیدا نشد',                              en: 'Not Found' },
  'error.failedToLoad':             { fa: 'خطا در بارگذاری داده‌ها',                en: 'Failed to load data' },

  // ── Empty States ──────────────────────────────────
  'empty.noData':                   { fa: 'داده‌ای یافت نشد',                       en: 'No data found' },
  'empty.noResults':                { fa: 'نتیجه‌ای یافت نشد',                      en: 'No results found' },
  'empty.noTransactions':           { fa: 'تراکنشی یافت نشد',                       en: 'No transactions' },
  'empty.noLedgerEntries':          { fa: 'ثبت دفتری یافت نشد',                     en: 'No ledger entries' },
  'empty.noTimeline':               { fa: 'داده‌ی timeline موجود نیست',              en: 'No timeline data available' },
  'empty.noWalletLinks':            { fa: 'لینک کیف پولی وجود ندارد',                en: 'No wallet links' },
  'empty.noGatewayTransactions':    { fa: 'تراکنش درگاهی یافت نشد',                 en: 'No gateway transactions' },
  'empty.noDecisions':              { fa: 'تصمیمی ثبت نشده',                        en: 'No decisions recorded' },
  'empty.noSecretFields':           { fa: 'فیلد محرمانه‌ای برای این ارائه‌دهنده وجود ندارد', en: 'No secret fields for this provider' },

  // ── Page Not Found ────────────────────────────────
  'notFound.wallet':                { fa: 'کیف پول پیدا نشد',                       en: 'Wallet not found' },
  'notFound.paymentIntent':         { fa: 'درخواست پرداخت پیدا نشد',                 en: 'Payment intent not found' },
  'notFound.paymentTransaction':    { fa: 'تراکنش پرداخت پیدا نشد',                  en: 'Payment transaction not found' },
  'notFound.paymentLink':           { fa: 'لینک پرداخت پیدا نشد',                    en: 'Payment link not found' },
  'notFound.withdrawal':            { fa: 'درخواست برداشت پیدا نشد',                 en: 'Withdrawal not found' },
  'notFound.payoutAccount':         { fa: 'حساب پرداخت پیدا نشد',                    en: 'Payout account not found' },
  'notFound.approval':              { fa: 'درخواست تأیید پیدا نشد',                  en: 'Approval request not found' },
  'notFound.approvalPolicy':        { fa: 'سیاست تأیید پیدا نشد',                    en: 'Approval policy not found' },
  'notFound.gatewayConfig':         { fa: 'تنظیمات درگاه پیدا نشد',                  en: 'Gateway config not found' },
  'notFound.gatewayProvider':       { fa: 'ارائه‌دهنده درگاه پیدا نشد',              en: 'Gateway provider not found' },
  'notFound.routingPolicy':         { fa: 'سیاست مسیریابی پیدا نشد',                 en: 'Routing policy not found' },
  'notFound.routingRule':           { fa: 'قانون مسیریابی پیدا نشد',                 en: 'Routing rule not found' },
  'notFound.auditLog':              { fa: 'گزارش حسابرسی پیدا نشد',                  en: 'Audit log not found' },
  'notFound.ledgerEntry':           { fa: 'ثبت دفتری پیدا نشد',                      en: 'Ledger entry not found' },
  'notFound.outboxEvent':           { fa: 'رویداد outbox پیدا نشد',                  en: 'Outbox event not found' },
  'notFound.webhookSubscription':   { fa: 'اشتراک webhook پیدا نشد',                en: 'Webhook subscription not found' },
  'notFound.webhookDelivery':       { fa: 'تحویل webhook پیدا نشد',                 en: 'Webhook delivery not found' },
  'notFound.refund':                { fa: 'درخواست بازپرداخت پیدا نشد',              en: 'Refund not found' },
  'notFound.reconciliationBatch':   { fa: 'دسته تسویه پیدا نشد',                     en: 'Reconciliation batch not found' },
  'notFound.reconciliationItem':    { fa: 'آیتم تسویه پیدا نشد',                     en: 'Reconciliation item not found' },
  'notFound.riskEvaluation':        { fa: 'ارزیابی ریسک پیدا نشد',                   en: 'Risk evaluation not found' },
  'notFound.riskCase':              { fa: 'کیس ریسک پیدا نشد',                       en: 'Risk case not found' },

  // ── Common Actions ────────────────────────────────
  'common.back':                    { fa: 'بازگشت',                                 en: 'Back' },
  'common.refresh':                 { fa: 'بازخوانی',                               en: 'Refresh' },
  'common.cancel':                  { fa: 'انصراف',                                 en: 'Cancel' },
  'common.confirm':                 { fa: 'تأیید',                                  en: 'Confirm' },
  'common.save':                    { fa: 'ذخیره',                                  en: 'Save' },
  'common.search':                  { fa: 'جستجو',                                  en: 'Search' },

  // ── List Page Empty Messages ──────────────────────
  'list.empty.wallets':             { fa: 'کیف پولی یافت نشد',                       en: 'No wallets found' },
  'list.empty.paymentIntents':      { fa: 'درخواست پرداختی یافت نشد',                en: 'No payment intents found' },
  'list.empty.paymentTransactions': { fa: 'تراکنش پرداختی یافت نشد',                 en: 'No payment transactions found' },
  'list.empty.paymentLinks':        { fa: 'لینک پرداختی یافت نشد',                   en: 'No payment links found' },
  'list.empty.refunds':             { fa: 'درخواست بازپرداختی یافت نشد',             en: 'No refund requests found' },
  'list.empty.withdrawals':         { fa: 'درخواست برداشتی یافت نشد',                en: 'No withdrawal requests found' },
  'list.empty.gatewayConfigs':      { fa: 'تنظیمات درگاهی یافت نشد',                 en: 'No gateway configs found' },
  'list.empty.gatewayProviders':    { fa: 'ارائه‌دهنده درگاهی یافت نشد',             en: 'No gateway providers found' },
  'list.empty.routingPolicies':     { fa: 'سیاست مسیریابی یافت نشد',                 en: 'No routing policies found' },
  'list.empty.approvals':           { fa: 'درخواست تأییدی یافت نشد',                 en: 'No approval requests found' },
  'list.empty.approvalPolicies':    { fa: 'سیاست تأییدی یافت نشد',                   en: 'No approval policies found' },
  'list.empty.auditLogs':           { fa: 'گزارش حسابرسی یافت نشد',                  en: 'No audit logs found' },
  'list.empty.ledgerEntries':       { fa: 'ثبت دفتری یافت نشد',                      en: 'No ledger entries found' },
  'list.empty.payoutAccounts':      { fa: 'حساب پرداختی یافت نشد',                   en: 'No payout accounts found' },
  'list.empty.webhookSubscriptions':{ fa: 'اشتراک webhook یافت نشد',                en: 'No webhook subscriptions found' },
  'list.empty.outboxEvents':        { fa: 'رویداد outbox یافت نشد',                  en: 'No outbox events found' },
  'list.empty.routingRules':        { fa: 'قانون مسیریابی یافت نشد',                 en: 'No routing rules found' },

  // ── Refund Detail ─────────────────────────────────
  'refund.notFound':                { fa: 'این درخواست بازپرداخت وجود ندارد یا حذف شده', en: 'This refund request does not exist or has been deleted' },
  'refund.notFoundTitle':           { fa: 'درخواست بازپرداخت پیدا نشد',              en: 'Refund Not Found' },
};

// ─── Exports ────────────────────────────────────────

export { translations };

/**
 * Non-reactive translate: reads language from localStorage.
 * For reactive translations inside React components, use useT() from LanguageProvider.
 */
/**
 * Humanize a missing key instead of rendering the raw dotted key in the UI,
 * e.g. 'notFound.wallet' -> 'Wallet'. Keeps untranslated labels readable and
 * matches the fallback behavior of auth/front.
 */
function humanizeKey(key: string): string {
  const lastPart = key.split('.').pop() || key;
  return lastPart.replace(/([A-Z])/g, ' $1').replace(/^./, (str) => str.toUpperCase()).trim()
}

export function t(key: string): string {
  const lang = resolveLanguage();
  const entry = translations[key];
  if (!entry) return humanizeKey(key);
  return entry[lang] ?? entry['en'] ?? humanizeKey(key);
}

/**
 * Returns the current resolved language ('fa' or 'en').
 */
export function useLanguage(): Lang {
  return resolveLanguage();
}

export type TranslationKey = keyof typeof translations;
