using CardService.src.CardActions.Api.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CardService.src.CardActions.Api.Models
{
    public sealed partial class AllowedActionsEngine : IAllowedActionsEngine
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

        private static readonly HashSet<CardType> AllCardTypes = Types(
            CardType.Prepaid,
            CardType.Debit,
            CardType.Credit);

        #endregion

        #region Card Statuses

        private static readonly HashSet<CardStatus> AllCardStatuses = Statuses(
            CardStatus.Ordered,
            CardStatus.Inactive,
            CardStatus.Active,
            CardStatus.Restricted,
            CardStatus.Blocked,
            CardStatus.Expired,
            CardStatus.Closed);

        private static readonly HashSet<CardStatus> InitialStatuses = Statuses(
            CardStatus.Ordered,
            CardStatus.Inactive,
            CardStatus.Active);

        private static readonly HashSet<CardStatus> InitialOrBlockedStatuses = Statuses(
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
                ToClause(CardType.Credit.ToHashSet(), AllCardStatuses)),

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

        private static HashSet<CardType> Types(params CardType[] cardTypes) =>
            cardTypes.ToHashSet();

        private static HashSet<CardStatus> Statuses(params CardStatus[] statuses) =>
            statuses.ToHashSet();

        private static Clause ToClause(
            IReadOnlySet<CardType> cardTypes,
            IReadOnlySet<CardStatus> cardStatuses,
            PinRequirement pin = PinRequirement.Any) => new(cardTypes, cardStatuses, pin);

        private sealed record Clause(
            IReadOnlySet<CardType> CardTypes,
            IReadOnlySet<CardStatus> CardStatuses,
            PinRequirement Pin)
        {
            public bool Matches(CardDetails card) =>
                CardTypes.Contains(card.CardType) &&
                CardStatuses.Contains(card.CardStatus) &&
                MatchesPin(card.IsPinSet);

            private bool MatchesPin(bool isPinSet) =>
                Pin switch
                {
                    PinRequirement.Any => true,
                    PinRequirement.Set => isPinSet,
                    PinRequirement.NotSet => !isPinSet,
                    _ => throw new UnreachableException($"Unhandled {nameof(PinRequirement)}: {Pin}")
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
    }
}
