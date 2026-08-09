using CardService.CardActions.Api.Models;

namespace CardService.CardActions.Api.Services
{
    public interface ICardRepository
    {
        public Task<CardDetails?> GetCardDetails(string userId, string cardNumber, CancellationToken cancellationToken);
    }
}