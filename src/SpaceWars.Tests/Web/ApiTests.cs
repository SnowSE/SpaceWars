using SpaceWars.Web.Types;
using System.Net.Http.Json;

namespace SpaceWars.Tests.Web;

public class ApiTests : IClassFixture<SpaceWarsWebApplicationFactory>
{
    private HttpClient httpClient;

    public ApiTests(SpaceWarsWebApplicationFactory webAppFactory)
    {
        httpClient = webAppFactory.CreateDefaultClient();
    }

    [Fact]
    public async Task JoiningGameGivesTokenAndShopItems()
    {
        var response = await httpClient.GetAsync("/game/join?name=jonathan");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JoinGameResponse>();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Shop.Any(s => s.Name == "Basic Cannon");
    }

    [Fact]
    public async Task MoveActionQueueForPlayer()
    {
        List<QueueActionRequest> actionRequest = [new("move", "250")];

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";
        var queueResponse = await httpClient.PostAsJsonAsync(url, actionRequest);
        queueResponse.EnsureSuccessStatusCode();

        var result = await queueResponse.Content.ReadFromJsonAsync<QueueActionResponse>();
        result.Message.Should().Be("Action queued");
    }

    [Fact]
    public async Task MoveActionQueueManyForPlayer()
    {
        List<QueueActionRequest> actionRequest = [new("move", "250"), new("move", "0")];

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";
        var queueResponse = await httpClient.PostAsJsonAsync(url, actionRequest);
        queueResponse.EnsureSuccessStatusCode();

        var result = await queueResponse.Content.ReadFromJsonAsync<QueueActionResponse>();
        result.Message.Should().Be("Action queued");
    }

    [Fact]
    public async Task FireActionQueueForPlayer()
    {
        List<QueueActionRequest> actionRequest = [new("fire", "basicCannon")];

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";
        var queueResponse = await httpClient.PostAsJsonAsync(url, actionRequest);
        queueResponse.EnsureSuccessStatusCode();

        var result = await queueResponse.Content.ReadFromJsonAsync<QueueActionResponse>();
        result.Message.Should().Be("Action queued");
    }

    [Fact]
    public async Task RepairActionQueueForPlayer()
    {
        List<QueueActionRequest> actionRequest = [new("repair", null)];

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";
        var queueResponse = await httpClient.PostAsJsonAsync(url, actionRequest);
        queueResponse.EnsureSuccessStatusCode();

        var result = await queueResponse.Content.ReadFromJsonAsync<QueueActionResponse>();
        result.Message.Should().Be("Action queued");
    }

    [Fact]
    public async Task ClearActionsQueueForPlayer()
    {
        List<QueueActionRequest> moveRequest = [new("move", "250")];
        List<QueueActionRequest> clearRequest = [new("clear", null)];

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";
        var queueResponse = await httpClient.PostAsJsonAsync(url, moveRequest);
        queueResponse.EnsureSuccessStatusCode();

        queueResponse = await httpClient.PostAsJsonAsync(url, clearRequest);
        queueResponse.EnsureSuccessStatusCode();

        var result = await queueResponse.Content.ReadFromJsonAsync<QueueActionResponse>();
        result.Message.Should().Be("Actions cleared");
    }


    [Fact]
    public async Task ShootPowerFist()
    {

        var response = await httpClient.GetAsync("/game/join?name=zack");
        response.EnsureSuccessStatusCode();
        var gameResponse = await response.Content.ReadFromJsonAsync<JoinGameResponse>();

        string url = $"/game/{gameResponse.Token}/queue";

        List<QueueActionRequest> purchaseRequest = [new("purchase", "Power Fist")];
        var purchaseResponse = await httpClient.PostAsJsonAsync(url, purchaseRequest);
        (await purchaseResponse.Content.ReadFromJsonAsync<QueueActionResponse>()).Message.Should().Be("Action queued");

        List<QueueActionRequest> changeWeaponRequest = [new("fire", "Power Fist")];
        var changeResponse = await httpClient.PostAsJsonAsync(url, changeWeaponRequest);
        (await changeResponse.Content.ReadFromJsonAsync<QueueActionResponse>()).Message.Should().Be("Action queued");
    }
}
