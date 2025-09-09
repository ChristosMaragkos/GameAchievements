# Achievements Library

A small, composable achievements framework with:
- Pluggable criteria (AbstractCriterion<TCondition, TContext>) and conditions (ICondition<TContext>). Convenience self-referential base: AbstractCriterion<TContext> with CriterionCondition<TContext>.
- Heterogeneous evaluators that can be nested (CompositeEvaluator) and bound to independent, lazily provided runtime contexts.
- A fluent AchievementBuilder to define achievements with readable AllOf/AnyOf groups and explicit Add or AddCustom methods for flexible context injection.

## Core Types
- ICriterion: Marker for a criterion.
- ICriterion<TContext>: Runtime-evaluable criterion.
- ICondition<TContext>: Requirement data that can test a runtime TContext.
- CriterionCondition<TContext>: Convenience abstract base when condition & context share the same type.
- AbstractCriterion<TCondition, TContext>: Base when requirement type (TCondition) differs from runtime context (TContext).
- AbstractCriterion<TContext>: Convenience base for self-referential (same-type) requirement/context.
- ICriterionEvaluator: Evaluates a unit or group of criteria.
- SingleEvaluator<TCondition,TContext>: Binds a criterion to a Func<TContext> context provider.
- SingleEvaluator<TContext>: Backwards compatible self-referential wrapper.
- CompositeEvaluator: Groups evaluators with All (AND) or Any (OR) semantics; allows nesting.
- Achievement: Name, Description, and a root evaluator; IsUnlocked() evaluates the tree.
- AchievementBuilder: Fluent API to compose achievements via Add (self-referential) / AddCustom (distinct types) plus AllOf/AnyOf then Build().

## Quick Start (Self-Referential Condition & Context)
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
    public KillCountCriterion(int required) => Condition = new KillCountCondition { RequiredKills = required };
    protected override KillCountCondition Condition { get; }
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
    .AllOf("Outer AND", outer =>
    {
        outer.Add("A: >= 5 kills", new KillCountCriterion(5), KillsCtx);
        outer.AllOf("Inner AND", innerAnd =>
        {
            innerAnd.AnyOf("(B OR C)", innerOr =>
            {
                innerOr.Add("B: 1500 score", new ScoreCriterion(1500), ScoreCtx);
                innerOr.Add("C: >= 10 kills", new KillCountCriterion(10), KillsCtx);
            });
            innerAnd.Add("D: >= 60 minutes", new TimePlayedCriterion(60), TimeCtx);
        });
    })
    .Build();
```

## Distinct Requirement vs Runtime Context (AddCustom)
Sometimes your stored requirement type differs from the runtime context snapshot. Example: store only a threshold but evaluate against a broader PlayerState.
```csharp
public sealed class LevelRequirement : ICondition<PlayerState>
{
    public int RequiredLevel { get; init; }
    public bool RequirementsMet(PlayerState ctx) => ctx.Level >= RequiredLevel;
}

public sealed class LevelCriterion : AbstractCriterion<LevelRequirement, PlayerState>
{
    public LevelCriterion(int minLevel) => Condition = new LevelRequirement { RequiredLevel = minLevel };
    protected override LevelRequirement Condition { get; }
}

public sealed class PlayerState { public int Level { get; set; } }

var player = new PlayerState();
var levelAchievement = AchievementBuilder
    .CreateNew("Novice", "Reach level 5")
    .AddCustom("Level >= 5", new LevelCriterion(5), () => player)
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
- Keep condition objects small & immutable; they are requirement data.
- Context providers should build a fresh snapshot each evaluation (avoid mutation inside the object returned unless intentional).
- Use AddCustom when the requirement (condition) type differs from the runtime context object.
- Achievements evaluate on demand; add a tracker if you need evented unlocks or persistence.
