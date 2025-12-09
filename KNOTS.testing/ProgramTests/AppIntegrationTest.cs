using Microsoft.AspNetCore.Mvc.Testing;

namespace TestProject1.ProgramTests;

public class AppIntegrationTest : IClassFixture<WebApplicationFactory<KNOTS.Components.App>> {
    private readonly WebApplicationFactory<KNOTS.Components.App> _factory;
    public AppIntegrationTest(WebApplicationFactory<KNOTS.Components.App> factory) { _factory = factory; }
    [Fact]
    public async Task GetRoot_ReturnsSuccess() {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}