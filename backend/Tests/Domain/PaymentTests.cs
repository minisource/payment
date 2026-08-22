using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Create_ShouldCreatePayment_WithValidParameters()
    {
        // Arrange
        var amount = 100000m;
        var gateway = "zibal";
        var callbackUrl = "http://example.com/callback";
        var returnUrl = "http://example.com/return";
        var userId = "user-123";

        // Act
        var payment = Payment.Create(
            amount: amount,
            gateway: gateway,
            callbackUrl: callbackUrl,
            returnUrl: returnUrl,
            userId: userId
        );

        // Assert
        payment.Should().NotBeNull();
        payment.Id.Should().NotBeEmpty();
        payment.Amount.Should().Be(amount);
        payment.Gateway.Should().Be(gateway);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldThrow_WhenAmountIsZero()
    {
        // Arrange & Act
        var act = () => Payment.Create(
            amount: 0,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenAmountIsNegative()
    {
        // Arrange & Act
        var act = () => Payment.Create(
            amount: -100,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenGatewayIsEmpty()
    {
        // Arrange & Act
        var act = () => Payment.Create(
            amount: 100000,
            gateway: "",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldGenerateTrackingNumber()
    {
        // Arrange & Act
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Assert
        payment.TrackingNumber.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MarkAsInitiated_ShouldUpdateStatusAndReference()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );
        var transactionRef = "TXN123456";

        // Act
        payment.MarkAsInitiated(transactionRef);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Initiated);
        payment.TransactionReference.Should().Be(transactionRef);
    }

    [Fact]
    public void MarkAsCompleted_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );
        payment.MarkAsInitiated("TXN123");

        // Act
        payment.MarkAsCompleted("verified-ref");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Act
        payment.MarkAsFailed("Insufficient funds");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void ApplyCredit_ShouldReduceAmountDue()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );
        var creditAmount = 30000m;

        // Act
        payment.ApplyCredit(creditAmount);

        // Assert
        payment.CreditApplied.Should().Be(creditAmount);
        payment.AmountDue.Should().Be(70000m);
    }

    [Fact]
    public void ApplyCredit_ShouldNotExceedAmount()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Act
        payment.ApplyCredit(150000m);

        // Assert
        payment.CreditApplied.Should().Be(100000m);
        payment.AmountDue.Should().Be(0);
    }

    [Fact]
    public void AddAttempt_ShouldAddPaymentAttempt()
    {
        // Arrange
        var payment = Payment.Create(
            amount: 100000,
            gateway: "zibal",
            callbackUrl: "http://example.com/callback",
            returnUrl: "http://example.com/return"
        );

        // Act
        var attempt = payment.AddAttempt("request-payload", "response-payload");

        // Assert
        attempt.Should().NotBeNull();
        payment.Attempts.Should().HaveCount(1);
    }
}
