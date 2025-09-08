# Achievements Library

A small, composable achievements framework with:
- Pluggable criteria (AbstractCriterion<TCondition>) and conditions (CriterionCondition<TCondition>).
- Heterogeneous evaluators that can be nested (CompositeEvaluator) and bound to independent contexts.
- A fluent AchievementBuilder to define achievements with readable All/Any groups.

## Core Types
- ICriterion: Marker for a criterion.
- AbstractCriterion<TCondition>: Base for concrete criteria; implements IsMet(TCondition).
- CriterionCondition<TCondition>: Base for condition state; implement RequirementsMet.
- ICriterionEvaluator: Evaluates a unit or group of criteria.
- SingleEvaluator<TCondition>: Binds a criterion to a Func<TCondition> context provider.
- CompositeEvaluator: Groups evaluators with All (AND) or Any (OR) semantics; allows nesting.
- Achievement: Name, Description, and a root evaluator; IsUnlocked() evaluates the tree.
- AchievementBuilder: Fluent API to compose achievements via Add/All/Any then Build().

## Quick Start
Create a criterion and condition:
```csharp
public sealed class KillCountCondition : CriterionCondition<KillCountCondition>
{
    public int Kills { get; init; }
    public int RequiredKills { get; init; }
    public override bool RequirementsMet(KillCountCondition ctx) => ctx.Kills >= RequiredKills;
}

public sealed class KillCountCriterion : AbstractCriterion<KillCountCondition>
{
    private readonly KillCountCondition _condition;
    public KillCountCriterion(int required) => _condition = new KillCountCondition { RequiredKills = required };
    protected override KillCountCondition Condition => _condition;
}
```
Bind to live state via a context provider and build achievements:
```csharp
var state = new GameState();
Func<KillCountCondition> KillsCtx = () => new KillCountCondition { Kills = state.Kills };
Func<TimePlayedCondition> TimeCtx = () => new TimePlayedCondition { MinutesPlayed = state.Minutes };
Func<ScoreCondition> ScoreCtx = () => new ScoreCondition { Score = state.Score };

// Single criterion
var firstBlood = AchievementBuilder
    .CreateNew("First Blood", "Get your first kill")
    .Add(">= 1 kill", new KillCountCriterion(1), KillsCtx)
    .Build();

// AND (All)
var grinder = AchievementBuilder
    .CreateNew("Grinder", "10 kills AND 30 minutes")
    .AllOf("All of", b =>
    {
        b.Add(">= 10 kills", new KillCountCriterion(10), KillsCtx);
        b.Add(">= 30 minutes", new TimePlayedCriterion(30), TimeCtx);
    })
    .Build();

// OR (Any)
var versatile = AchievementBuilder
    .CreateNew("Versatile", "1000 score OR 50 kills")
    .AnyOf("Either", b =>
    {
        b.Add("1000 score", new ScoreCriterion(1000), ScoreCtx);
        b.Add("50 kills", new KillCountCriterion(50), KillsCtx);
    })
    .Build();

// Nested: A AND ((B OR C) AND D)
var nested = AchievementBuilder
    .CreateNew("Nested Mastery", "A AND ((B OR C) AND D)")
    .All("Outer AND", outer =>
    {
        outer.Add("A: >= 5 kills", new KillCountCriterion(5), KillsCtx);
        outer.All("Inner AND", innerAnd =>
        {
            innerAnd.Any("(B OR C)", innerOr =>
            {
                innerOr.Add("B: 1500 score", new ScoreCriterion(1500), ScoreCtx);
                innerOr.Add("C: >= 10 kills", new KillCountCriterion(10), KillsCtx);
            });
            innerAnd.Add("D: >= 60 minutes", new TimePlayedCriterion(60), TimeCtx);
        });
    })
    .Build();
```

## Static readonly achievements
```csharp
public static class GameAchievements
{
    public static readonly Achievement FirstBlood = AchievementBuilder
        .CreateNew("First Blood", "Get your first kill")
        .Add(">= 1 kill", new KillCountCriterion(1), () => new KillCountCondition { Kills = Game.State.Kills })
        .Build();
}
```

## Demo
This repo includes a Demo console app that simulates state changes and prints unlock statuses.

Run:
```bash
 dotnet build
 dotnet run --project Demo/Demo.csproj
```

## Design Tips
- Keep conditions small, immutable data holders.
- Context providers should snapshot current state for each evaluation.
- Achievements evaluate on demand; add a tracker if you need evented unlocks or persistence.

