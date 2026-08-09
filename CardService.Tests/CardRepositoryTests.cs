using CardService.CardActions.Api.Models;
using CardService.CardActions.Api.Services;
using Xunit;

namespace CardService.Tests.Services
{
    public class CardRepositoryTests
    {
        private readonly CardRepository _repository = new CardRepository();

        [Fact(DisplayName = "GetCardDetails returns the card for a known user and card")]
        public async Task GetCardDetails_KnownUserAndCard_ReturnsCard()
        {
            var result = await _repository.GetCardDetails("User1", "Card11", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Card11", result!.CardNumber);
        }

        [Fact(DisplayName = "GetCardDetails returns null when user does not exist")]
        public async Task GetCardDetails_UnknownUser_ReturnsNull()
        {
            var result = await _repository.GetCardDetails("UserXXX", "Card11", CancellationToken.None);

            Assert.Null(result);
        }

        [Fact(DisplayName = "GetCardDetails returns null when card does not exist")]
        public async Task GetCardDetails_UnknownCard_ReturnsNull()
        {
            var result = await _repository.GetCardDetails("User1", "CardXXX", CancellationToken.None);

            Assert.Null(result);
        }

        [Fact(DisplayName = "GetCardDetails throws for cancellation token which already cancelled")]
        public async Task GetCardDetails_AlreadyCancelledToken_Throws()
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => _repository.GetCardDetails("User1", "Card1", cts.Token));
            }
        }

        [Theory(DisplayName = "GetCardDetails check all prepared combinations of CardType and CardStatus")]
        [InlineData("User1")]
        [InlineData("User2")]
        [InlineData("User3")]
        public async Task GetCardDetails_CheckAllCombinationsOfCardTypeAndCardStatus(string userId)
        {
            var expectedCombinations = Enum.GetValues<CardType>()
                .SelectMany(type => Enum.GetValues<CardStatus>(), (type, status) => (type, status))
                .ToHashSet();

            var lookups = Enumerable.Range(1, expectedCombinations.Count)
                .Select(index => _repository.GetCardDetails(
                    userId, 
                    $"Card{userId.Substring(4)}{index}", 
                    CancellationToken.None));

            var cards = await Task.WhenAll(lookups);

            Assert.All(cards, card => Assert.NotNull(card));

            var actualCombinations = cards.Select(c => (c!.CardType, c.CardStatus)).ToHashSet();
            Assert.Equal(expectedCombinations, actualCombinations);
        }
    }
}