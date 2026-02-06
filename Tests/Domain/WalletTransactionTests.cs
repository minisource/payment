using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.Domain;

public class WalletTransactionTests
{
    [Fact]
    public void CreateCredit_ShouldCreateCreditTransaction()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var amount = 50000m;
        var description = "Test credit";

        // Act
        var transaction = WalletTransaction.CreateCredit(walletId, amount, description);

        // Assert
        transaction.Should().NotBeNull();
        transaction.WalletId.Should().Be(walletId);
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(TransactionType.Credit);
        transaction.Description.Should().Be(description);
    }

    [Fact]
    public void CreateDebit_ShouldCreateDebitTransaction()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var amount = 30000m;
        var description = "Test debit";

        // Act
        var transaction = WalletTransaction.CreateDebit(walletId, amount, description);

        // Assert
        transaction.Should().NotBeNull();
        transaction.WalletId.Should().Be(walletId);
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(TransactionType.Debit);
        transaction.Description.Should().Be(description);
    }

    [Fact]
    public void CreateCredit_ShouldIncludeReference()
    {
        // Arrange
        var walletId = Guid.NewGuid();
        var referenceId = "payment-123";
        var referenceType = "Payment";

        // Act
        var transaction = WalletTransaction.CreateCredit(
            walletId,
            50000m,
            "Test credit",
            referenceId,
            referenceType
        );

        // Assert
        transaction.ReferenceId.Should().Be(referenceId);
        transaction.ReferenceType.Should().Be(referenceType);
    }

    [Fact]
    public void CreateDebit_ShouldThrow_WhenAmountIsZero()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        // Act
        var act = () => WalletTransaction.CreateDebit(walletId, 0, "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateCredit_ShouldThrow_WhenAmountIsNegative()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        // Act
        var act = () => WalletTransaction.CreateCredit(walletId, -100, "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
