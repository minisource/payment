using System.Security.Cryptography;
using System.Text.Json;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Minisource.Common.Domain;
using Minisource.Common.Exceptions;

namespace Application.Features.PayoutAccounts;

public class PayoutAccountService : IPayoutAccountService
{
    private readonly IPayoutAccountRepository _repo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PayoutAccountService> _logger;

    public PayoutAccountService(IPayoutAccountRepository repo, IAuditLogRepository auditRepo, IUnitOfWork uow, ILogger<PayoutAccountService> logger)
    { _repo = repo; _auditRepo = auditRepo; _uow = uow; _logger = logger; }

    public async Task<PayoutAccountDto> CreateAsync(Guid tenantId, Guid ownerId, string ownerType, CreatePayoutAccountRequest request, CancellationToken ct)
    {
        // Secure hashing of sensitive fields
        string? cardNumberHash = null, cardNumberMasked = null;
        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            cardNumberHash = HashSensitive(tenantId, request.CardNumber);
            cardNumberMasked = MaskCard(request.CardNumber);
        }
        string? ibanHash = null, iban = null;
        if (!string.IsNullOrWhiteSpace(request.Iban))
        {
            ibanHash = HashSensitive(tenantId, request.Iban);
            iban = request.Iban; // Store raw IBAN for payout display (encrypted at rest by DB if needed)
        }
        string? accountNumberHash = null, accountNumberMasked = null;
        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            accountNumberHash = HashSensitive(tenantId, request.AccountNumber);
            accountNumberMasked = MaskAccountNumber(request.AccountNumber);
        }

        // Check for duplicate IBAN
        if (!string.IsNullOrWhiteSpace(ibanHash))
        {
            var existing = await _repo.GetByIbanHashAsync(tenantId, ibanHash, ct);
            if (existing != null)
                throw new BusinessException("Payout account with this IBAN already exists", "payout_account_duplicate");
        }

        var account = PayoutAccount.Create(tenantId, ownerType, ownerId, request.AccountType,
            request.HolderName, request.Currency,
            bankName: request.BankName, bankCode: request.BankCode,
            cardNumberMasked: cardNumberMasked, cardNumberHash: cardNumberHash,
            iban: iban, ibanHash: ibanHash,
            accountNumberMasked: accountNumberMasked, accountNumberHash: accountNumberHash,
            metadata: request.Metadata, createdByUserId: ownerId);

        await _repo.AddAsync(account, ct);
        await _auditRepo.AddAsync(Audit("payout_account.created", tenantId, ownerId, account), ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Payout account {AccountId} created for owner {OwnerId}", account.Id, ownerId);
        return MapToDto(account);
    }

    public async Task<List<PayoutAccountDto>> GetMyAccountsAsync(Guid tenantId, string ownerType, Guid ownerId, CancellationToken ct)
    {
        var accounts = await _repo.GetByOwnerAsync(tenantId, ownerType, ownerId, ct: ct);
        return accounts.Where(a => a.DeletedAt == null).Select(MapToDto).ToList();
    }

    public async Task<PayoutAccountDto> GetAsync(Guid accountId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        return MapToDto(a);
    }

    public async Task<PayoutAccountDto> UpdateAsync(Guid accountId, Guid tenantId, Guid ownerId, UpdatePayoutAccountRequest request, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        if (a.TenantId != tenantId || a.OwnerId != ownerId) throw new ForbiddenException();
        a.Update(request.HolderName, request.BankName, request.BankCode, request.Metadata);
        await _repo.UpdateAsync(a, ct);
        await _auditRepo.AddAsync(Audit("payout_account.updated", tenantId, ownerId, a), ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(a);
    }

    public async Task SetDefaultAsync(Guid accountId, Guid tenantId, Guid ownerId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        if (a.TenantId != tenantId || a.OwnerId != ownerId) throw new ForbiddenException();

        // Clear default on all other accounts
        var all = await _repo.GetByOwnerAsync(tenantId, a.OwnerType, ownerId, ct: ct);
        foreach (var other in all.Where(x => x.Id != accountId && x.IsDefault))
        {
            other.ClearDefault();
            await _repo.UpdateAsync(other, ct);
        }
        a.SetDefault();
        await _repo.UpdateAsync(a, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid accountId, Guid tenantId, Guid ownerId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        if (a.TenantId != tenantId || a.OwnerId != ownerId) throw new ForbiddenException();
        a.SoftDelete();
        await _repo.UpdateAsync(a, ct);
        await _auditRepo.AddAsync(Audit("payout_account.deleted", tenantId, ownerId, a), ct);
        await _uow.SaveChangesAsync(ct);
    }

    // Admin
    public async Task<List<PayoutAccountDto>> AdminListAsync(Guid? tenantId, PayoutAccountStatus? status, int skip, int take, CancellationToken ct)
    {
        if (!tenantId.HasValue) throw new ValidationException("tenantId", "TenantId is required for admin payout account listing");
        var accounts = await _repo.GetByTenantAsync(tenantId.Value, status, skip, take, ct);
        return accounts.Where(a => a.DeletedAt == null).Select(MapToDto).ToList();
    }

    public async Task<PayoutAccountDto> AdminGetAsync(Guid accountId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        return MapToDto(a);
    }

    public async Task AdminVerifyAsync(Guid accountId, Guid actorUserId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        a.Verify(actorUserId);
        await _repo.UpdateAsync(a, ct);
        await _auditRepo.AddAsync(Audit("payout_account.verified", a.TenantId, actorUserId, a), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AdminRejectAsync(Guid accountId, Guid actorUserId, string reason, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        a.Reject(reason);
        await _repo.UpdateAsync(a, ct);
        var audit = Audit("payout_account.rejected", a.TenantId, actorUserId, a);
        audit.Reason = reason;
        await _auditRepo.AddAsync(audit, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AdminDisableAsync(Guid accountId, CancellationToken ct)
    {
        var a = await _repo.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("PayoutAccount", accountId);
        a.Disable();
        await _repo.UpdateAsync(a, ct);
        await _auditRepo.AddAsync(Audit("payout_account.disabled", a.TenantId, null, a), ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ═══ Helpers ═════════════════════════════════════════

    internal static string HashSensitive(Guid tenantId, string value)
    {
        var raw = $"{tenantId}:{value.Trim().ToUpperInvariant()}";
        return Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));
    }

    private static string MaskCard(string card)
    {
        var clean = new string(card.Where(char.IsDigit).ToArray());
        return clean.Length >= 8 ? $"{clean[..4]}****{clean[^4..]}" : "****";
    }

    private static string MaskAccountNumber(string acct)
    {
        var clean = new string(acct.Where(char.IsDigit).ToArray());
        return clean.Length >= 6 ? $"****{clean[^4..]}" : "****";
    }

    private static string MaskIban(string iban)
    {
        return iban.Length >= 8 ? $"{iban[..2]}****{iban[^4..]}" : "****";
    }

    internal static PayoutAccountDto MapToDto(PayoutAccount a) => new()
    {
        Id = a.Id, TenantId = a.TenantId, OwnerType = a.OwnerType, OwnerId = a.OwnerId,
        AccountType = a.AccountType, BankName = a.BankName, BankCode = a.BankCode,
        CardNumberMasked = a.CardNumberMasked,
        IbanMasked = a.Iban != null ? MaskIban(a.Iban) : null,
        AccountNumberMasked = a.AccountNumberMasked,
        HolderName = a.HolderName, Currency = a.Currency,
        Status = a.Status.ToString().ToLowerInvariant(), IsDefault = a.IsDefault,
        VerifiedAt = a.VerifiedAt, RejectionReason = a.RejectionReason,
        CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };

    private static AuditLog Audit(string action, Guid tenantId, Guid? actorUserId, PayoutAccount a) => new()
    {
        TenantId = tenantId, ActorUserId = actorUserId,
        Action = action, EntityType = "PayoutAccount", EntityId = a.Id.ToString(),
        AfterSnapshot = JsonSerializer.Serialize(new { a.AccountType, a.BankName, a.HolderName, a.Currency, a.Status })
    };
}
