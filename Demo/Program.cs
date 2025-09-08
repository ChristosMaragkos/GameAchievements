using Achievements;
using Demo;

// Simple game-like state and context providers
var state = new GameState();

// Create achievements via the fluent builder
var a1 = AchievementBuilder
    .CreateNew("First Blood", "Get your first kill")
    .Add(">= 1 kill", new KillCountCriterion(1), KillsCtx())
    .Build();

var a2 = AchievementBuilder
    .CreateNew("Grinder", "10 kills and 30 minutes played")
    .AllOf("All of", b =>
    {
        b.Add(">= 10 kills", new KillCountCriterion(10), KillsCtx());
        b.Add(">= 30 minutes", new TimePlayedCriterion(30), TimeCtx());
    })
    .Build();

var a3 = AchievementBuilder
    .CreateNew("Versatile", "Score 1000 points OR 50 kills")
    .AnyOf("Either", b =>
    {
        b.Add("1000 score", new ScoreCriterion(1000), ScoreCtx());
        b.Add("50 kills", new KillCountCriterion(50), KillsCtx());
    })
    .Build();

var a4 = AchievementBuilder
    .CreateNew("Hardcore", "2000 score AND 120 minutes")
    .AllOf("All of", b =>
    {
        b.Add("2000 score", new ScoreCriterion(2000), ScoreCtx());
        b.Add("120 minutes", new TimePlayedCriterion(120), TimeCtx());
    })
    .Build();

// Nested: A AND ((B OR C) AND D)
var a5 = AchievementBuilder
    .CreateNew("Nested Mastery", "A AND ((B OR C) AND D) => 5 kills AND ((1500 score OR 10 kills) AND 60 minutes)")
    .AllOf("Outer AND", outer =>
    {
        // A
        outer.Add("A: >= 5 kills", new KillCountCriterion(5), KillsCtx());
        // (B OR C) AND D
        outer.AllOf("Inner AND", innerAnd =>
        {
            innerAnd.AnyOf("(B OR C)", innerOr =>
            {
                innerOr.Add("B: 1500 score", new ScoreCriterion(1500), ScoreCtx());
                innerOr.Add("C: >= 10 kills", new KillCountCriterion(10), KillsCtx());
            });
            innerAnd.Add("D: >= 60 minutes", new TimePlayedCriterion(60), TimeCtx());
        });
    })
    .Build();

var all = new[] { a1, a2, a3, a4, a5 };

// Tracker demo: register achievements and listen for unlocks
var tracker = new AchievementTracker();
tracker.OnUnlocked += (ach, key, when) =>
    Console.WriteLine($"[Tracker] Unlocked '{ach.Name}' with key {key} at {when}");
tracker.Register(a1, "FirstBlood");
tracker.Register(a2, "Grinder");
tracker.Register(a3, "Versatile");
tracker.Register(a4, "Hardcore");
tracker.Register(a5, "NestedMastery");

Console.WriteLine("Starting simulation...\n");

Report();

// Step 1: one kill -> First Blood should unlock
state.AddKills(1);
Console.WriteLine("After 1 kill:");
Report();

// Step 2: grind to 10 kills and 25 minutes -> Grinder still locked (needs 30)
state.AddKills(9);
state.AddMinutes(25);
Console.WriteLine("After total 10 kills and 25 minutes:");
Report();

// Step 3: reach 30 minutes -> Grinder unlocks
state.AddMinutes(5);
Console.WriteLine("After 30 minutes:");
Report();

// Step 4: get 900 score -> Versatile still locked (needs 1000 score OR 50 kills)
state.AddScore(900);
Console.WriteLine("After 900 score:");
Report();

// Step 5: reach 1000 score -> Versatile unlocks
state.AddScore(100);
Console.WriteLine("After 1000 score:");
Report();

// Step 6: push to Hardcore conditions (2000 score AND 120 minutes)
state.AddScore(1000); // now 2000
state.AddMinutes(90); // now 120
Console.WriteLine("After 2000 score and 120 minutes:");
Report();

// Final: show that OR also unlocks via the alternative path (50 kills)
var a3B = AchievementBuilder
    .CreateNew("Versatile (Kills path)", "Score 1000 points OR 50 kills")
    .AnyOf("Either", b =>
    {
        b.Add("1000 score", new ScoreCriterion(1000), () => new ScoreCondition { Score = 0 });
        b.Add("50 kills", new KillCountCriterion(50), KillsCtx());
    })
    .Build();
Console.WriteLine("\nDemonstrate OR via kills path only (fresh instance):");
Console.WriteLine($"Before kills: {(a3B.IsUnlocked() ? "UNLOCKED" : "locked")}");
state.AddKills(40); // from 10 to 50
Console.WriteLine($"After reaching 50 kills: {(a3B.IsUnlocked() ? "UNLOCKED" : "locked")}");
return;

Func<KillCountCondition> KillsCtx() => () => new KillCountCondition { Kills = state.Kills };

Func<TimePlayedCondition> TimeCtx() => () => new TimePlayedCondition { MinutesPlayed = state.Minutes };

Func<ScoreCondition> ScoreCtx() => () => new ScoreCondition { Score = state.Score };

void Report()
{
    // Evaluate via tracker (fires events upon transitions)
    tracker.EvaluateAll();

    foreach (var a in all)
    {
        Console.WriteLine($"{a}: {(a.IsUnlocked() ? "UNLOCKED" : "locked")}");
    }

    Console.WriteLine();
}

// --- types ---
internal sealed class GameState
{
    public int Kills { get; private set; }
    public double Minutes { get; private set; }
    public int Score { get; private set; }

    public void AddKills(int n) => Kills += n;
    public void AddMinutes(double m) => Minutes += m;
    public void AddScore(int s) => Score += s;
}