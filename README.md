# CardService

[![CI](https://github.com/YevgenKlymenchuk/CardService/actions/workflows/ci.yml/badge.svg)](https://github.com/YevgenKlymenchuk/CardService/actions/workflows/ci.yml)

.NET 8 Web API for the "allowed card actions" task. Give it a userId and a card number, get back the list of actions allowed on that card, based on card type, status and PIN.

## Structure

Everything's under `src/CardActions.Api`. One controller, `CardActionsController`, depends on `ICardRepository` and `IAllowedActionsEngine`. Enums/records in `Models`. `GlobalExceptionHandler` in `ErrorHandling`.

Kept card lookup and rule evaluation as two separate classes rather than one.

## Rules engine

The table is 13 actions x 3 types x 7 statuses, some also depending on PIN. Didn't want a long if/else, so `AllowedActionsEngine` has a static list of rules - action + one or more clauses (types + statuses + optional PIN requirement), matches if any clause matches.

Action7 has two clauses:

```csharp
new(AllowedAction.Action7,
    ToClause(AllCardTypes, InitialStatuses, PinRequirement.NotSet),
    ToClause(AllCardTypes, Statuses(CardStatus.Blocked), PinRequirement.Set)),
```

allowed without PIN in ordered/inactive/active, allowed with PIN when blocked.

## Data

`CardRepository` — same sample data generation as in the task (User1-User3, Card11, Card12...), plus a 1s delay to fake network latency. Behind `ICardRepository`.

## Errors

- Not found -> 404, ProblemDetails.
- Unhandled -> `GlobalExceptionHandler`, logged, 500 ProblemDetails, no stack trace to client.
- Empty/missing userId or cardNumber -> 400 automatically. With `[ApiController]` + nullable reference types on, a non-nullable string route param is treated as required, so this is already covered without extra checks - tested it with a blank/whitespace value to be sure.
- CancellationToken passed down to Task.Delay so a dropped client doesn't just keep it running for no reason.

## Running

```bash
cd CardService
dotnet run
```

Default `http` profile (`http://localhost:5299`). For https: `dotnet run --launch-profile https`.

In Development, root (`/`) redirects to `/swagger`.

```
GET /api/CardActions/users/User1/cards/Card11/actions
```

```json
{
  "userId": "User1",
  "cardNumber": "Card11",
  "allowedActions": ["ACTION3", "ACTION4", "..."]
}
```

Enums serialize as upper snake_case (ACTION1, not Action1).

## Tests

```bash
dotnet test
```

CardService.Tests covers three things separately:

- `AllowedActionsEngineTests` - the whole rules table (3 types x 7 statuses x PIN set/not set) + examples from the task description as standalone cases.
- `CardRepositoryTests` - known/unknown user and card, cancelled token.
- `CardActionsControllerTests` - 200 with the engine's result mapped into the response, 404 with ProblemDetails when the card isn't found, and that the engine isn't called at all in that case. Repository and engine are mocked (Moq) so this only checks the controller's own wiring, not the rules themselves.