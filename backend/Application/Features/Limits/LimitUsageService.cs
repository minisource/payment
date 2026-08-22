using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;

namespace Application.Features.Limits;

public class LimitUsageService : ILimitUsageService
{
    private readonly PaymentDbContext _ctx;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LimitUsageService> _logger;

    public LimitUsageService(PaymentDbContext ctx, ISettingsRepository settingsRepo,
        IUnitOfWork uow, ILogger<LimitUsageService> logger)
    { _ctx = ctx; _settingsRepo = settingsRepo; _uow = uow; _logger = logger; }

    public async Task TrackUsageAsync(Guid tenantId, string? ownerType, Guid? ownerId, string operationType,
        string? currency, decimal amount, CancellationToken ct)
    {
        var windows = GetWindows();
        foreach (var (windowType, start, end) in windows)
        {
            // Atomic upsert + increment using raw SQL to prevent lost-update race
            var sql = @"
                INSERT INTO ""PaymentLimitUsages"" (""Id"", ""TenantId"", ""OwnerType"", ""OwnerId"",
                    ""OperationType"", ""Currency"", ""WindowType"", ""WindowStart"", ""WindowEnd"",
                    ""AmountTotal"", ""CountTotal"", ""CreatedAt"", ""UpdatedAt"")
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, 1, now(), now())
                ON CONFLICT (""TenantId"", ""OwnerType"", ""OwnerId"", ""OperationType"", ""Currency"",
                    ""WindowType"", ""WindowStart"")
                DO UPDATE SET ""AmountTotal"" = ""PaymentLimitUsages"".""AmountTotal"" + {9},
                    ""CountTotal"" = ""PaymentLimitUsages"".""CountTotal"" + 1,
                    ""UpdatedAt"" = now()";
            await _ctx.Database.ExecuteSqlRawAsync(sql,
                Guid.NewGuid(), tenantId, (object?)ownerType ?? DBNull.Value, (object?)ownerId ?? DBNull.Value,
                operationType, (object?)currency ?? DBNull.Value, windowType, start, end, amount, ct);
        }
    }

    public async Task<(bool allowed, string? reason)> CheckLimitsAsync(Guid tenantId, string? ownerType, Guid? ownerId,
        string operationType, string? currency, decimal amount, CancellationToken ct)
    {
        // Read limits from tenant settings
        var settings = await _settingsRepo.GetTenantSettingsAsync(tenantId, ct);
        var dailyLimit = settings?.WithdrawalDailyAmountLimit ?? 100_000_000m;
        var monthlyLimit = settings?.WithdrawalMonthlyAmountLimit ?? 1_000_000_000m;
        var dailyCountLimit = settings?.WithdrawalDailyCountLimit ?? 100;
        var monthlyCountLimit = settings?.WithdrawalMonthlyCountLimit ?? 500;

        var windows = GetWindows();
        foreach (var (windowType, start, end) in windows)
        {
            var usage = await _ctx.PaymentLimitUsages.FirstOrDefaultAsync(u =>
                u.TenantId == tenantId && u.OwnerType == ownerType && u.OwnerId == ownerId &&
                u.OperationType == operationType && u.Currency == currency &&
                u.WindowType == windowType && u.WindowStart == start, ct);

            if (usage == null) continue;

            if (windowType == "daily")
            {
                if (usage.AmountTotal + amount > dailyLimit)
                    return (false, $"Daily {operationType} limit exceeded ({dailyLimit:N0})");
                if (usage.CountTotal >= dailyCountLimit)
                    return (false, $"Daily {operationType} count limit exceeded ({dailyCountLimit})");
            }
            else if (windowType == "monthly")
            {
                if (usage.AmountTotal + amount > monthlyLimit)
                    return (false, $"Monthly {operationType} limit exceeded ({monthlyLimit:N0})");
                if (usage.CountTotal >= monthlyCountLimit)
                    return (false, $"Monthly {operationType} count limit exceeded ({monthlyCountLimit})");
            }
        }
        return (true, null);
    }

    private static List<(string windowType, DateTime start, DateTime end)> GetWindows()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        return new List<(string, DateTime, DateTime)>
        {
            ("daily", today, today.AddDays(1)),
            ("monthly", monthStart, monthStart.AddMonths(1))
        };
    }
}
