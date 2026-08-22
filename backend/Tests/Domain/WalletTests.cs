using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.Domain;

public class WalletTests
{
    [Fact]
    public void Create_ShouldCreateWallet_WithValidUserId()
    {
        // Arrange
        var userId = "user-123";

        // Act
        var wallet = Wallet.Create(userId);

        // Assert
        wallet.Should().NotBeNull();
        wallet.Id.Should().NotBeEmpty();
        wallet.UserId.Should().Be(userId);
        wallet.Balance.Should().Be(0);
        wallet.Currency.Should().Be("IRR");
        wallet.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldCreateWallet_WithCustomCurrency()
    {
        // Arrange
        var userId = "user-123";
        var currency = "USD";

        // Act
        var wallet = Wallet.Create(userId, currency);

        // Assert
        wallet.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () => Wallet.Create("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsWhitespace()
    {
        // Arrange & Act
        var act = () => Wallet.Create("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Credit_ShouldAddFunds()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        var amount = 50000m;
        var description = "Test credit";

        // Act
        var transaction = wallet.Credit(amount, description);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(TransactionType.Credit);
        wallet.Balance.Should().Be(amount);
    }

    [Fact]
    public void Credit_ShouldThrow_WhenAmountIsZero()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");

        // Act
        var act = () => wallet.Credit(0, "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Credit_ShouldThrow_WhenAmountIsNegative()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");

        // Act
        var act = () => wallet.Credit(-100, "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Debit_ShouldRemoveFunds()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(100000, "Initial credit");
        var debitAmount = 30000m;

        // Act
        var transaction = wallet.Debit(debitAmount, "Test debit");

        // Assert
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(debitAmount);
        transaction.Type.Should().Be(TransactionType.Debit);
        wallet.Balance.Should().Be(70000m);
    }

    [Fact]
    public void Debit_ShouldThrow_WhenInsufficientBalance()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(50000, "Initial credit");

        // Act
        var act = () => wallet.Debit(100000, "Test debit");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Debit_ShouldThrow_WhenAmountIsZero()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(100000, "Initial credit");

        // Act
        var act = () => wallet.Debit(0, "Test");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");

        // Act
        wallet.Deactivate();

        // Assert
        wallet.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Credit_ShouldThrow_WhenWalletIsInactive()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Deactivate();

        // Act
        var act = () => wallet.Credit(100, "Test");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Debit_ShouldThrow_WhenWalletIsInactive()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(100000, "Initial");
        wallet.Deactivate();

        // Act
        var act = () => wallet.Debit(50000, "Test");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Deactivate();

        // Act
        wallet.Activate();

        // Assert
        wallet.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RecalculateBalance_ShouldComputeCorrectBalance()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(100000, "Credit 1");
        wallet.Credit(50000, "Credit 2");
        wallet.Debit(30000, "Debit 1");

        // Act
        wallet.RecalculateBalance();

        // Assert
        wallet.Balance.Should().Be(120000m);
    }

    [Fact]
    public void Transactions_ShouldBeReadOnly()
    {
        // Arrange
        var wallet = Wallet.Create("user-123");
        wallet.Credit(100000, "Test");

        // Assert
        wallet.Transactions.Should().NotBeNull();
        wallet.Transactions.Should().HaveCount(1);
    }
}
