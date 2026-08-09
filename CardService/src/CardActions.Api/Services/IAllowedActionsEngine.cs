using CardService.CardActions.Api.Models;

namespace CardService.CardActions.Api.Services
{
    public interface IAllowedActionsEngine
    {
        IReadOnlyList<AllowedAction> GetAllowedActions(CardDetails card);
    }
}
