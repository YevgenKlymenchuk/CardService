using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using CardService.CardActions.Api.Controllers;
using CardService.CardActions.Api.Models;
using CardService.CardActions.Api.Services;

namespace CardService.Tests.Controllers
{
    public class CardActionsControllerTests
    {
        private readonly Mock<ICardRepository> _repository = new Mock<ICardRepository>();
        private readonly Mock<IAllowedActionsEngine> _engine = new Mock<IAllowedActionsEngine>();
        private readonly CardActionsController _cardActionsController = null!;

        public CardActionsControllerTests()
        {
            _cardActionsController = new CardActionsController(
                _repository.Object,
                _engine.Object,
                NullLogger<CardActionsController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact(DisplayName = "GetAllowedActions returns 200 with allowed actions when card is found")]
        public async Task GetAllowedActions_CardFound_Returns200WithActions()
        {
            var card = new CardDetails("Card11", CardType.Prepaid, CardStatus.Active, IsPinSet: true);
            var expectedActions = new[] { AllowedAction.Action1, AllowedAction.Action3 };

            _repository
                .Setup(r => r.GetCardDetails("User1", "Card11", It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);
            _engine
                .Setup(e => e.GetAllowedActions(card))
                .Returns(expectedActions);

            var result = await _cardActionsController.GetAllowedActions("User1", "Card11", CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<CardActionsResponse>(okResult.Value);
            Assert.Equal("User1", response.UserId);
            Assert.Equal("Card11", response.CardNumber);
            Assert.Equal(expectedActions, response.AllowedActions);

            _engine.Verify(e => e.GetAllowedActions(card), Times.Once);
        }

        [Fact(DisplayName = "GetAllowedActions returns 404 with error details when card is not found")]
        public async Task GetAllowedActions_CardNotFound_Returns404WithDetails()
        {
            _repository
                .Setup(r => r.GetCardDetails("User1", "CardXXX", It.IsAny<CancellationToken>()))
                .ReturnsAsync((CardDetails?)null);

            var result = await _cardActionsController.GetAllowedActions("User1", "CardXXX", CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        }

        [Fact(DisplayName = "GetAllowedActions returns 404 and does not call engine when card is not found")]
        public async Task GetAllowedActions_CardNotFound_CheckEngineNotCallEngine()
        {
            _repository
                .Setup(r => r.GetCardDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CardDetails?)null);

            await _cardActionsController.GetAllowedActions("User1", "CardXXX", CancellationToken.None);

            _engine.Verify(e => e.GetAllowedActions(It.IsAny<CardDetails>()), Times.Never);
        }
    }
}