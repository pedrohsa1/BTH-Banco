using System.Net;
using System.Net.Http.Json;
using Bth.Common.Application.Clock;
using Bth.Modules.Transfers.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Bth.IntegrationTests;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 9, 25, 18, 0, 0, DateTimeKind.Utc);
}

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public BankingApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Factory = new BankingApiFactory(_container.GetConnectionString());
        using HttpClient client = Factory.CreateClient();
        HttpResponseMessage health = await client.GetAsync("/health");
        health.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}

public sealed class BankingApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    public FakeDateTimeProvider Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", connectionString);
        builder.UseSetting("Seed:Enabled", "false");
        builder.UseSetting("Hangfire:Enabled", "false");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(Clock);
        });
    }
}

[CollectionDefinition("integration")]
public sealed class IntegrationCollection : ICollectionFixture<PostgresFixture>;

[Collection("integration")]
public sealed class TransferScenarios(PostgresFixture fixture)
{
    [Fact]
    public async Task Transfers_with_sufficient_balance()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 500m, 0m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage response = await Transfer(client, source, destination, 200m);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(300m, (await GetAccount(client, source)).Balance);
        Assert.Equal(200m, (await GetAccount(client, destination)).Balance);
    }

    [Fact]
    public async Task Uses_overdraft_and_allows_negative_balance()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 500m, 1000m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage response = await Transfer(client, source, destination, 1000m);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(-500m, (await GetAccount(client, source)).Balance);
        Assert.Equal(1000m, (await GetAccount(client, destination)).Balance);
    }

    [Fact]
    public async Task Rejects_transfer_above_overdraft()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 500m, 1000m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage response = await Transfer(client, source, destination, 1501m);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(500m, (await GetAccount(client, source)).Balance);
        Assert.Equal(0m, (await GetAccount(client, destination)).Balance);
    }

    [Fact]
    public async Task Incoming_transfer_covers_used_overdraft()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 200m, 1000m);
        Guid destination = await CreateAccount(client, 2000m, 0m);

        Assert.Equal(HttpStatusCode.Created, (await Transfer(client, source, destination, 1000m)).StatusCode);
        Assert.Equal(-800m, (await GetAccount(client, source)).Balance);

        Assert.Equal(HttpStatusCode.Created, (await Transfer(client, destination, source, 1000m)).StatusCode);
        Assert.Equal(200m, (await GetAccount(client, source)).Balance);
    }

    [Fact]
    public async Task Rejects_amount_above_the_hourly_limit()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 10000m, 0m, dayAmount: 5000m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage response = await Transfer(client, source, destination, 5001m);
        TransferBody body = (await response.Content.ReadFromJsonAsync<TransferBody>())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Failed", body.Status);
        Assert.Contains("valor", body.FailureReason, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(10000m, (await GetAccount(client, source)).Balance);
    }

    [Fact]
    public async Task Rejected_transfers_count_as_attempts()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 10m, 0m, dayAttempts: 2);
        Guid destination = await CreateAccount(client, 0m, 0m);

        Assert.Equal(HttpStatusCode.Created, (await Transfer(client, source, destination, 1m)).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Transfer(client, source, destination, 100m)).StatusCode);
        HttpResponseMessage third = await Transfer(client, source, destination, 1m);
        TransferBody body = (await third.Content.ReadFromJsonAsync<TransferBody>())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, third.StatusCode);
        Assert.Contains("tentativas", body.FailureReason, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(9m, (await GetAccount(client, source)).Balance);
    }

    [Fact]
    public async Task Applies_the_night_limit()
    {
        fixture.Factory.Clock.UtcNow = Night;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 5000m, 0m, dayAmount: 5000m, nightAmount: 1000m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage rejected = await Transfer(client, source, destination, 1001m);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);

        HttpResponseMessage accepted = await Transfer(client, source, destination, 1000m);
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        Assert.Equal(4000m, (await GetAccount(client, source)).Balance);
    }

    [Fact]
    public async Task Scheduled_transfer_fails_when_balance_is_insufficient_at_execution()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 100m, 0m);
        Guid destination = await CreateAccount(client, 0m, 0m);
        DateTime scheduledAt = Day.AddHours(2);

        HttpResponseMessage scheduled = await client.PostAsJsonAsync("/api/transfers/scheduled", new
        {
            sourceAccountId = source,
            destinationAccountId = destination,
            amount = 500m,
            scheduledAtUtc = scheduledAt
        });
        TransferBody body = (await scheduled.Content.ReadFromJsonAsync<TransferBody>())!;
        Assert.Equal(HttpStatusCode.Created, scheduled.StatusCode);
        Assert.Equal("Scheduled", body.Status);

        fixture.Factory.Clock.UtcNow = scheduledAt.AddMinutes(1);
        await using AsyncServiceScope scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ITransferExecutor>().ExecuteDueAsync(body.Id, CancellationToken.None);

        TransferBody updated = (await client.GetFromJsonAsync<TransferBody>($"/api/transfers/{body.Id}"))!;
        Assert.Equal("Failed", updated.Status);
        Assert.Equal(100m, (await GetAccount(client, source)).Balance);
        Assert.Equal(0m, (await GetAccount(client, destination)).Balance);
    }

    [Fact]
    public async Task Cancels_a_scheduled_transfer_before_execution()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 300m, 0m);
        Guid destination = await CreateAccount(client, 0m, 0m);

        HttpResponseMessage scheduled = await client.PostAsJsonAsync("/api/transfers/scheduled", new
        {
            sourceAccountId = source,
            destinationAccountId = destination,
            amount = 50m,
            scheduledAtUtc = Day.AddHours(3)
        });
        TransferBody body = (await scheduled.Content.ReadFromJsonAsync<TransferBody>())!;

        HttpResponseMessage cancelled = await client.PostAsync($"/api/transfers/{body.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);

        fixture.Factory.Clock.UtcNow = Day.AddHours(4);
        await using AsyncServiceScope scope = fixture.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ITransferExecutor>().ExecuteDueAsync(body.Id, CancellationToken.None);

        TransferBody updated = (await client.GetFromJsonAsync<TransferBody>($"/api/transfers/{body.Id}"))!;
        Assert.Equal("Cancelled", updated.Status);
        Assert.Equal(300m, (await GetAccount(client, source)).Balance);
    }

    [Fact]
    public async Task Two_concurrent_transfers_cannot_spend_the_same_balance()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 100m, 0m, dayAmount: 5000m, dayAttempts: 10);
        Guid firstDestination = await CreateAccount(client, 0m, 0m);
        Guid secondDestination = await CreateAccount(client, 0m, 0m);

        Task<HttpResponseMessage> first = Transfer(client, source, firstDestination, 100m);
        Task<HttpResponseMessage> second = Transfer(client, source, secondDestination, 100m);
        HttpResponseMessage[] responses = await Task.WhenAll(first, second);

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity));
        Assert.Equal(0m, (await GetAccount(client, source)).Balance);
        decimal received = (await GetAccount(client, firstDestination)).Balance + (await GetAccount(client, secondDestination)).Balance;
        Assert.Equal(100m, received);
    }

    [Fact]
    public async Task Inactive_account_cannot_send_or_receive()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid inactive = await CreateAccount(client, 400m, 0m);
        Guid other = await CreateAccount(client, 400m, 0m);
        HttpResponseMessage status = await client.PatchAsJsonAsync($"/api/accounts/{inactive}/status", new { status = "Inactive" });
        status.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Transfer(client, inactive, other, 10m)).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Transfer(client, other, inactive, 10m)).StatusCode);
        Assert.Equal(400m, (await GetAccount(client, inactive)).Balance);
        Assert.Equal(400m, (await GetAccount(client, other)).Balance);
    }

    [Fact]
    public async Task Repeating_the_idempotency_key_does_not_debit_twice()
    {
        fixture.Factory.Clock.UtcNow = Day;
        HttpClient client = fixture.Factory.CreateClient();
        Guid source = await CreateAccount(client, 300m, 0m);
        Guid destination = await CreateAccount(client, 0m, 0m);
        string key = Guid.NewGuid().ToString();

        HttpResponseMessage first = await Transfer(client, source, destination, 40m, key);
        HttpResponseMessage second = await Transfer(client, source, destination, 40m, key);
        TransferBody firstBody = (await first.Content.ReadFromJsonAsync<TransferBody>())!;
        TransferBody secondBody = (await second.Content.ReadFromJsonAsync<TransferBody>())!;

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(firstBody.Id, secondBody.Id);
        Assert.True(secondBody.Replayed);
        Assert.Equal(260m, (await GetAccount(client, source)).Balance);
    }

    private static readonly DateTime Day = new(2026, 9, 25, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Night = new(2026, 9, 26, 2, 30, 0, DateTimeKind.Utc);

    private static async Task<Guid> CreateAccount(
        HttpClient client,
        decimal balance,
        decimal overdraft,
        decimal dayAmount = 5000m,
        int dayAttempts = 5,
        decimal nightAmount = 1000m,
        int nightAttempts = 3)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/accounts", new
        {
            holderName = "Pessoa teste",
            document = Guid.NewGuid().ToString("N")[..14],
            initialBalance = balance,
            overdraftLimit = overdraft,
            dayMaxAmountPerHour = dayAmount,
            dayMaxAttemptsPerHour = dayAttempts,
            nightMaxAmountPerHour = nightAmount,
            nightMaxAttemptsPerHour = nightAttempts
        });
        response.EnsureSuccessStatusCode();
        CreatedAccount created = (await response.Content.ReadFromJsonAsync<CreatedAccount>())!;
        return created.Id;
    }

    private static Task<HttpResponseMessage> Transfer(
        HttpClient client,
        Guid source,
        Guid destination,
        decimal amount,
        string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transfers")
        {
            Content = JsonContent.Create(new
            {
                sourceAccountId = source,
                destinationAccountId = destination,
                amount
            })
        };

        if (idempotencyKey is not null)
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        return client.SendAsync(request);
    }

    private static async Task<AccountBody> GetAccount(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<AccountBody>($"/api/accounts/{id}"))!;

    private sealed record CreatedAccount(Guid Id);
    private sealed record AccountBody(decimal Balance, string Status);
    private sealed record TransferBody(Guid Id, string Status, string? FailureReason, bool Replayed);
}
