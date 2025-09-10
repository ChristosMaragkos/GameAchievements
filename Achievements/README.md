# Achievements Library (Stateless Root Pattern)

A composable achievements framework using a stateless root criteria pattern:
- Central singleton root criteria (e.g. Criteria.Kills / Score / Deaths) expose Create(condition) which returns per-achievement handles.
- Runtime events (kills, score changes, deaths) are pushed once into the root (root.Evaluate(context)), which fans out to every handle.
- Achievement trees are composed from handles (no direct game state snapshot passing, no per-achievement polling logic).

## Core Concepts
- AbstractCriterion<TCondition,TContext,TProgress>: Public base for creating a root criterion. You implement creation of initial progress and mutation logic; framework manages handles & fan-out.
- Condition record/class: Immutable requirement data (EnemyType, Required, etc.).
- Progress (TProgress): Arbitrary mutable state tracked per handle (e.g. int count, struct with multiple counters).
- Handle (IUpdatableCriterion): Internal per-achievement progress state (framework-generated; not user implemented).
- Criteria registry: Static singleton instances (Criteria.Kills, Criteria.Score, Criteria.Deaths).
- AchievementBuilder: Fluent AllOf / AnyOf composition of handles.
- AchievementTracker: Evaluates unlock transitions and persists via IAchievementStore.

## Why This Pattern?
- Single authoritative event stream per context type.
- Many achievements share same events with distinct conditions.
- No duplicated event dispatch logic per instance.
- Simple persistence (usually only unlocked names).

## Example: Defining Achievements
```csharp
var kill10Zombies = Criteria.Kills.Create("Kill 10 Zombies", new KillTypeCountRoot.Condition("Zombie", 10));
var score25       = Criteria.Score.Create("Score 25", new ScoreRoot.Condition(25));
var dieTwice      = Criteria.Deaths.Create("Die Twice", new DeathRoot.Condition(2));

var achievement = AchievementBuilder
    .CreateNew("Pressure", "Kill 10 zombies AND (Score 25 OR Die Twice)")
    .AllOf("Root", all =>
    {
        all.Criterion("10 Zombies", kill10Zombies);
        all.AnyOf("Branch", any =>
        {
            any.Criterion("Score 25", score25);
            any.Criterion("Die Twice", dieTwice);
        });
    })
    .Build();
```

## Pushing Events
```csharp
Criteria.Kills.Evaluate("Zombie");
Criteria.Score.Evaluate(5);
Criteria.Deaths.Evaluate(1);
```

## Tracking Unlocks
```csharp
var tracker = new AchievementTracker();
tracker.Register(achievement);
tracker.EvaluateAll();
```

## Built-in Root Criteria
- KillTypeCountRoot (string enemyType)
- ScoreRoot (int deltaScore)
- DeathRoot (int deltaDeaths)

## Creating a Custom Criterion
Implement a condition + criterion class only.
```csharp
public sealed class DistanceRoot : AbstractCriterion<DistanceRoot.Condition, double, double>
{
    public sealed record Condition(double RequiredMeters);
    protected override double CreateInitialProgress(Condition c) => 0d;
    protected override bool UpdateProgress(ref double progress, Condition c, double deltaMeters)
    {
        progress += deltaMeters;
        return progress >= c.RequiredMeters;
    }
}
// Register
public static class Criteria
{
    public static readonly DistanceRoot Distance = new();
}
// Use
var run100 = Criteria.Distance.Create("Run 100m", new DistanceRoot.Condition(100));
```

## Optional DSL Extensions
```csharp
public static class AchievementBuilderExtensions
{
    public static AchievementBuilder Kill(this AchievementBuilder b, string label, string enemy, int count)
        => b.Criterion(label, Criteria.Kills.Create(label, new KillTypeCountRoot.Condition(enemy, count)));
}
```

## Persistence
Store unlocked achievement names (IAchievementStore). Mid-progress serialization optional; expose Handles() if needed.

## Migration Notes
Legacy root base replaced by AbstractCriterion<TCondition,TContext,TProgress>.

## License
MIT
