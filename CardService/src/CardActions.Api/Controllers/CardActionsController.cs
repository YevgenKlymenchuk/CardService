using Microsoft.AspNetCore.Mvc;
using CardService.src.CardActions.Api.Models;
using CardService.src.CardActions.Api.Services;

namespace CardService.src.CardActions.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CardActionsController : ControllerBase
    {
        private readonly Services.CardService _cards;

        private readonly ILogger<CardActionsController> _logger;
        private readonly IAllowedActionsEngine _allowActionEngine;

        public CardActionsController(
            Services.CardService service,
            IAllowedActionsEngine allowedActionsEngine,
            ILogger<CardActionsController> logger)
        {
            _cards = service;
            _allowActionEngine = allowedActionsEngine;
            _logger = logger;
        }

        [HttpGet("users/{userId}/cards/{cardNumber}/actions")]
        [ProducesResponseType<CardActionsResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CardActionsResponse>> GetAllowedActions(
            string userId,
            string cardNumber)
        {
            var card = await _cards.GetCardDetails(userId, cardNumber);
            if (card is null)
            {
                _logger.LogDebug("User '{UserId}' or card '{CardNumber}' was not found.", userId, cardNumber);

                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Card or user not found",
                    detail: $"User '{userId}' or card '{cardNumber}' was not found.",
                    instance: HttpContext.Request.Path);
            }

            return Ok(new CardActionsResponse(userId, card.CardNumber, _allowActionEngine.GetAllowedActions(card)));
        }

    }
}
