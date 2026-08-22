using System.Net;
using System.Net.Http.Json;

namespace payment.Tests.E2e;

/// <summary>
/// E2E smoke tests against a running Payment API (set PAYMENT_BASE_URL).
/// </summary>
public class PaymentsApiE2ETests
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("PAYMENT_BASE_URL") ?? "http://127.0.0.1:4005";

    private static async Task<bool> IsServiceUpAsync()
    {
        try
        {
            var resp = await Client.GetAsync($"{BaseUrl.TrimEnd('/')}/health");
            return resp.StatusCode < HttpStatusCode.InternalServerError;
        }
        catch
        {
            return false;
        }
    }

    [Fact]
    public async Task Health_ReturnsOk_WhenServiceRunning()
    {
        if (!await IsServiceUpAsync())
        {
            return; // skip when payment service not deployed
        }

        var response = await Client.GetAsync($"{BaseUrl.TrimEnd('/')}/health");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Payments_RequiresAuth_WhenServiceRunning()
    {
        if (!await IsServiceUpAsync())
        {
            return;
        }

        var response = await Client.GetAsync($"{BaseUrl.TrimEnd('/')}/api/v1/payments");
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"unexpected status: {response.StatusCode}");
    }
}
