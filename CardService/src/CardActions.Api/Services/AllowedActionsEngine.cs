using System;
using System.Collections.Frozen;
using System.Diagnostics;
using CardService.CardActions.Api.Models;

namespace CardService.CardActions.Api.Services
{
    public sealed class AllowedActionsEngine : IAllowedActionsEngine
    {
        public IReadOnlyList<AllowedAction> GetAllowedActions(CardDetails card)
        {
            ArgumentNullException.ThrowIfNull(card);

            return Rules
                .Where(rule => rule.Matches(card))
                .Select(rule => rule.Action)
                .ToArray();
        }

        #region Card Types

        private static readonly FrozenSet<CardType> AllCardTypes = Types(
            CardType.Prepaid,
            CardType.Debit,
            CardType.Credit);

        #endregion

        #region Card Statuses

        private static readonly FrozenSet<CardStatus> AllCardStatuses = Statuses(
            CardStatus.Ordered,
            CardStatus.Inactive,
            CardStatus.Active,
            CardStatus.Restricted,
            CardStatus.Blocked,
            CardStatus.Expired,
            CardStatus.Closed);

        private static readonly FrozenSet<CardStatus> InitialStatuses = Statuses(
            CardStatus.Ordered,
            CardStatus.Inactive,
            CardStatus.Active);

        private static readonly FrozenSet<CardStatus> InitialOrBlockedStatuses = Statuses(
            CardStatus.Ordered,
            CardStatus.Inactive,
            CardStatus.Active,
            CardStatus.Blocked);

        #endregion

        private static readonly ActionRule[] Rules =
        [
            new(AllowedAction.Action1,
                ToClause(AllCardTypes, Statuses(CardStatus.Active))),

            new(AllowedAction.Action2,
                ToClause(AllCardTypes, Statuses(CardStatus.Inactive))),

            new(AllowedAction.Action3,
                ToClause(AllCardTypes, AllCardStatuses)),

            new(AllowedAction.Action4,
                ToClause(AllCardTypes, AllCardStatuses)),

            new(AllowedAction.Action5,
                ToClause(Types(CardType.Credit), AllCardStatuses)),

            new(AllowedAction.Action6,
                ToClause(AllCardTypes, InitialOrBlockedStatuses, PinRequirement.Set)),

            new(AllowedAction.Action7,
                ToClause(AllCardTypes, InitialStatuses, PinRequirement.NotSet),
                ToClause(AllCardTypes, Statuses(CardStatus.Blocked), PinRequirement.Set)),

            new(AllowedAction.Action8,
                ToClause(AllCardTypes, InitialOrBlockedStatuses)),

            new(AllowedAction.Action9,
                ToClause(AllCardTypes, AllCardStatuses)),

            new(AllowedAction.Action10,
                ToClause(AllCardTypes, InitialStatuses)),

            new(AllowedAction.Action11,
                ToClause(AllCardTypes, Statuses(CardStatus.Inactive, CardStatus.Active))),

            new(AllowedAction.Action12,
                ToClause(AllCardTypes, InitialStatuses)),

            new(AllowedAction.Action13,
                ToClause(AllCardTypes, InitialStatuses)),
        ];

        private static FrozenSet<CardType> Types(params CardType[] cardTypes) =>
            cardTypes.ToFrozenSet();

        private static FrozenSet<CardStatus> Statuses(params CardStatus[] statuses) =>
            statuses.ToFrozenSet();

        private static Clause ToClause(
            IReadOnlySet<CardType> cardTypes,
            IReadOnlySet<CardStatus> cardStatuses,
            PinRequirement pin = PinRequirement.Any) => new(cardTypes, cardStatuses, pin);

        private sealed record Clause
        {
            private IReadOnlySet<CardType> _cardTypes { get; }
            private IReadOnlySet<CardStatus> _cardStatuses { get; }
            private PinRequirement _pin { get; }

            public Clause(IReadOnlySet<CardType> cardTypes, 
                IReadOnlySet<CardStatus> cardStatuses,
                PinRequirement pin)
            {
                ArgumentNullException.ThrowIfNull(cardTypes, nameof(cardTypes));
                ArgumentNullException.ThrowIfNull(cardStatuses, nameof(cardStatuses));

                if (cardTypes.Count == 0)
                    throw new ArgumentException("At least one card type is required.", nameof(cardTypes));

                if (cardStatuses.Count == 0)
                    throw new ArgumentException("At least one card status is required.", nameof(cardStatuses));

                _cardTypes = cardTypes;
                _cardStatuses = cardStatuses;
                _pin = pin;
            }
            public bool Matches(CardDetails card) =>
                _cardTypes.Contains(card.CardType) &&
                _cardStatuses.Contains(card.CardStatus) &&
                MatchesPin(card.IsPinSet);

            private bool MatchesPin(bool isPinSet) =>
                _pin switch
                {
                    PinRequirement.Any => true,
                    PinRequirement.Set => isPinSet,
                    PinRequirement.NotSet => !isPinSet,
                    _ => throw new UnreachableException($"Unhandled {nameof(PinRequirement)}: {_pin}")
                };
        }

        private sealed record ActionRule
        {
            public AllowedAction Action { get; }
            private readonly Clause[] _clauses;

            public ActionRule(AllowedAction action, params Clause[] clauses)
            {
                if (clauses.Length == 0)
                    throw new ArgumentException("At least one clause is required.", nameof(clauses));

                Action = action;
                _clauses = clauses;
            }

            public bool Matches(CardDetails card) => _clauses.Any(c => c.Matches(card));
        }
        private enum PinRequirement
        {
            Any,
            Set,
            NotSet
        }
    }
}
