namespace CardService.src.CardActions.Api.Models
{
    public record CardDetails(
        string CardNumber, 
        CardType CardType, 
        CardStatus CardStatus, 
        bool IsPinSet);
}
