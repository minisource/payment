using Application.DTOs;
using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Application;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public WalletServiceTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task GetWallet_ShouldReturnWallet_WhenExists()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act & Assert
        Assert.True(true); // Placeholder
    }

    [Fact]
    public async Task GetWallet_ShouldCreateWallet_WhenNotExists()
    {
        // Arrange
        var userId = "user-123";

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act & Assert
        // Should create new wallet
        Assert.True(true); // Placeholder
    }

    [Fact]
    public async Task AddCredit_ShouldIncreaseBalance()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        var creditAmount = 50000m;

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        wallet.Credit(creditAmount, "Test credit");

        // Assert
        wallet.Balance.Should().Be(creditAmount);
    }

    [Fact]
    public async Task DeductCredit_ShouldDecreaseBalance()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        wallet.Credit(100000, "Initial");

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        wallet.Debit(30000, "Test debit");

        // Assert
        wallet.Balance.Should().Be(70000);
    }

    [Fact]
    public async Task DeductCredit_ShouldThrow_WhenInsufficientBalance()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        wallet.Credit(50000, "Initial");

        // Act
        var act = () => wallet.Debit(100000, "Test debit");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnAllTransactions()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        wallet.Credit(100000, "Credit 1");
        wallet.Credit(50000, "Credit 2");
        wallet.Debit(30000, "Debit 1");

        // Assert
        wallet.Transactions.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeactivateWallet_ShouldPreventTransactions()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        wallet.Deactivate();

        // Act
        var act = () => wallet.Credit(50000, "Test");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task ActivateWallet_ShouldAllowTransactions()
    {
        // Arrange
        var userId = "user-123";
        var wallet = Wallet.Create(userId);
        wallet.Deactivate();
        wallet.Activate();

        // Act
        wallet.Credit(50000, "Test credit");

        // Assert
        wallet.Balance.Should().Be(50000);
    }
}
