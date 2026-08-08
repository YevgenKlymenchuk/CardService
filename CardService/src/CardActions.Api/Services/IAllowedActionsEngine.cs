using CardService.src.CardActions.Api.Models;

namespace CardService.src.CardActions.Api.Services
{
    public interface IAllowedActionsEngine
    {
        IReadOnlyList<AllowedAction> GetAllowedActions(CardDetails card);
    }
}
