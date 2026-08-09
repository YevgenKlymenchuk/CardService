using Xunit;
using CardService.CardActions.Api.Services;

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
    }
}