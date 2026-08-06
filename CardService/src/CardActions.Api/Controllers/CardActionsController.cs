using CardService.src.CardActions.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CardService.src.CardActions.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CardActionsController : Controller
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

        [HttpGet]
        public IActionResult Health()
        {
            return Ok("Healthy");
        }

        [HttpGet("users/{userId}/cards/{cardNumber}/actions")]
        [ProducesResponseType<CardActionsResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<CardActionsResponse>> GetAllowedActions(
            string userId,
            string cardNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(cardNumber))
            {
                return BadRequest();
            }

            var card = await _cards.GetCardDetails(userId, cardNumber);
            if (card is null)
            {
                _logger.LogDebug("Card '{0}' or  user '{1}' was not found.", userId, cardNumber);

                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Card or user not found",
                    Detail = $"Card '{cardNumber}' or  user '{userId}' was not found."
                });
            }

            return Ok(new CardActionsResponse(userId, card.CardNumber, _allowActionEngine.GetAllowedActions(card)));
        }

    }
}
