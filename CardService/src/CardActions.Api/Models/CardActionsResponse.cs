namespace CardService.src.CardActions.Api.Models
{
    public sealed record CardActionsResponse(
        string UserId,
        string CardNumber,
        IReadOnlyList<AllowedAction> AllowedActions);
}
