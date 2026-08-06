using Microsoft.AspNetCore.Mvc;

namespace CardService.src.CardActions.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardActionsController : Controller
    {
        private readonly ILogger<CardActionsController> _logger;

        public CardActionsController(ILogger<CardActionsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Health()
        {
            return Ok("Healthy");
        }
    }
}
