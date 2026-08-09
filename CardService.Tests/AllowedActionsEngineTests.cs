using Microsoft.VisualBasic;
using Xunit;

using CardService.CardActions.Api.Models;
using CardService.CardActions.Api.Services;

namespace CardService.Tests.Services
{
    public class AllowedActionsEngineTests
    {
        private readonly AllowedActionsEngine _engine = new AllowedActionsEngine();

        public static IEnumerable<object[]> AllCombinations()
        {
            foreach (CardType type in Enum.GetValues<CardType>())
                foreach (CardStatus status in Enum.GetValues<CardStatus>())
                    foreach (var isPinSet in new[] { false, true })
                    {
                        yield return new object[] { type, status, isPinSet, ExpectedActions(type, status, isPinSet) };
                    }
        }

        private static AllowedAction[] ExpectedActions(CardType type, CardStatus status, bool isPinSet)
        {
            var actions = new List<AllowedAction>();

            switch (status)
            {
                case CardStatus.Ordered:
                    actions.AddRange(new[]
                    {
                        AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action8,
                        AllowedAction.Action9, AllowedAction.Action10, AllowedAction.Action12, AllowedAction.Action13
                    });
                    actions.Add(isPinSet ? AllowedAction.Action6 : AllowedAction.Action7);
                    break;

                case CardStatus.Inactive:
                    actions.AddRange(new[]
                    {
                        AllowedAction.Action2, AllowedAction.Action3, AllowedAction.Action4,
                        AllowedAction.Action8, AllowedAction.Action9, AllowedAction.Action10,
                        AllowedAction.Action11, AllowedAction.Action12, AllowedAction.Action13
                    });
                    actions.Add(isPinSet ? AllowedAction.Action6 : AllowedAction.Action7);
                    break;

                case CardStatus.Active:
                    actions.AddRange(new[]
                    {
                        AllowedAction.Action1, AllowedAction.Action3, AllowedAction.Action4,
                        AllowedAction.Action8, AllowedAction.Action9, AllowedAction.Action10,
                        AllowedAction.Action11, AllowedAction.Action12, AllowedAction.Action13
                    });
                    actions.Add(isPinSet ? AllowedAction.Action6 : AllowedAction.Action7);
                    break;

                case CardStatus.Restricted:
                    actions.AddRange(new[] 
                    { 
                        AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action9 
                    });
                    break;

                case CardStatus.Blocked:
                    actions.AddRange(new[]
                    {
                        AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action8, AllowedAction.Action9
                    });
                    if (isPinSet)
                    {
                        actions.Add(AllowedAction.Action6);
                        actions.Add(AllowedAction.Action7);
                    }
                    break;

                case CardStatus.Expired:
                case CardStatus.Closed:
                    actions.AddRange(new[] 
                    { 
                        AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action9 
                    });
                    break;
            }

            // Additional rules based on card type
            if (type == CardType.Credit)
                actions.Add(AllowedAction.Action5);

            return actions.ToArray();
        }

        [Theory(DisplayName = "All available combinations")]
        [MemberData(nameof(AllCombinations))]
        public void GetAllowedActions_MatchesTaskTable(
            CardType type, 
            CardStatus status, 
            bool isPinSet, 
            AllowedAction[] expected)
        {
            var card = new CardDetails(
                $"{type.GetHashCode():D5}{status.GetHashCode():D6}{isPinSet.GetHashCode():D5}", 
                type, 
                status, 
                isPinSet);

            var result = _engine.GetAllowedActions(card);

            Assert.Equal(expected.ToHashSet(), result.ToHashSet());
        }

        [Fact(DisplayName = "Prepared condition: Prepaid with Closed")]
        public void Prepaid_Closed_ReturnsAction3_4_9()
        {
            var card = new CardDetails("Card1", CardType.Prepaid, CardStatus.Closed, IsPinSet: false);

            var result = _engine.GetAllowedActions(card);

            Assert.Equal(
                new[] 
                { 
                    AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action9 
                }.ToHashSet(),
                result.ToHashSet());
        }

        [Fact(DisplayName = "Prepared condition: Credit with Blocked when PIN is set")]
        public void Credit_Blocked_WithPin_ReturnsAction3_4_5_6_7_8_9()
        {
            var card = new CardDetails("Card2", CardType.Credit, CardStatus.Blocked, IsPinSet: true);

            var result = _engine.GetAllowedActions(card);

            Assert.Equal(
                new[]
                {
                    AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action5,
                    AllowedAction.Action6, AllowedAction.Action7, AllowedAction.Action8, AllowedAction.Action9
                }.ToHashSet(),
                result.ToHashSet());
        }

        [Fact(DisplayName = "Prepared condition: Credit with Blocked when PIN is not set")]
        public void Credit_Blocked_WithoutPin_ReturnsAction3_4_5_8_9()
        {
            var card = new CardDetails("Card3", CardType.Credit, CardStatus.Blocked, IsPinSet: false);

            var result = _engine.GetAllowedActions(card);

            Assert.Equal(
                new[]
                {
                    AllowedAction.Action3, AllowedAction.Action4, AllowedAction.Action5,
                    AllowedAction.Action8, AllowedAction.Action9
                }.ToHashSet(),
                result.ToHashSet());
        }

        [Fact(DisplayName = "Prepared condition: Null card")]
        public void NullCard_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _engine.GetAllowedActions(null!));
        }
    }
}