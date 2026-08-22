using Minisource.Common.Domain;

namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// Immutable and ensures business rules around money operations.
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a new Money instance.
    /// </summary>
    public static Money Create(decimal amount, string currency = "IRR")
        => new(amount, currency);

    /// <summary>
    /// Creates zero money with specified currency.
    /// </summary>
    public static Money Zero(string currency = "IRR")
        => new(0, currency);

    /// <summary>
    /// Adds two money values. Must be same currency.
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts money value. Must be same currency.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Subtraction would result in negative amount");
        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplies money by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Checks if this money is greater than other.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Checks if this money is greater than or equal to other.
    /// </summary>
    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    /// <summary>
    /// Checks if this is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N0} {Currency}";

    // Operators
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
    public static bool operator <(Money left, Money right) => right.IsGreaterThan(left);
    public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
    public static bool operator <=(Money left, Money right) => right.IsGreaterThanOrEqual(left);
}
