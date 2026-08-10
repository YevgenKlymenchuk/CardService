using Xunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CardService.Tests.Integration
{
    public class CardActionsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;

        public CardActionsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact(DisplayName = "API returns 200 with allowed actions when card and user are found")]
        public async Task GetAllowedActions_UserAndCardExists_Returns200()
        {
            string userId = "User1";
            string cardNumber = "Card17";

            var response = await _httpClient.GetAsync($"/api/CardActions/users/{userId}/cards/{cardNumber}/actions");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CardActionsApiResponse>();

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(cardNumber, result.CardNumber);

            Assert.Equal(new[] { "ACTION3", "ACTION4", "ACTION9" }, result.AllowedActions);
        }

        [Fact(DisplayName = "API returns 404 when card is not found")]
        public async Task GetAllowedActions_UnknownCard_Returns404()
        {
            var response = await _httpClient.GetAsync("/api/CardActions/users/User1/cards/CardXXX/actions");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "API returns 404 when route segment is missing")]
        public async Task GetAllowedActions_MissingUserId_Returns404()
        {
            var response = await _httpClient.GetAsync("/api/CardActions/users//cards/Card11/actions");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "API returns 400 when user id or card number is whitespace")]
        public async Task GetAllowedActions_WhitespaceForUserIdorCardNumber_Returns400()
        {
            var responseUserId = await _httpClient.GetAsync("/api/CardActions/users/%20/cards/Card11/actions");
            
            var responseCardNumber = await _httpClient.GetAsync("/api/CardActions/users/User1/cards/%20/actions");

            Assert.Equal(HttpStatusCode.BadRequest, responseUserId.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, responseCardNumber.StatusCode);
        }

        private sealed record CardActionsApiResponse(
            string UserId,
            string CardNumber,
            string[] AllowedActions);
    }
}