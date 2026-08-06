namespace CardService.src.CardActions.Api.Models
{
    public interface IAllowedActionsEngine
    {
        IReadOnlyList<AllowedAction> GetAllowedActions(CardDetails card);
    }
}
