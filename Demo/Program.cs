using Achievements;
using Demo;

// Push-based demo: criteria hold their own state; game/events push context directly.
// Builder only references existing criterion instances via AddUpdatable (no context providers / snapshots).
// Same criterion can participate in multiple achievements simultaneously.

// --- Static (or long-lived) criterion instances ---
var kill10Zombies = KillTypeCountCriterion.Create("Kill 10 Zombies", "Zombie", 10);
var kill5Zombies = KillTypeCountCriterion.Create("Kill 5 Zombies", "Zombie", 5);
var kill2Skeletons = KillTypeCountCriterion.Create("Kill 2 Skeletons", "Skeleton", 2);
var kill5Skeletons = KillTypeCountCriterion.Create("Kill 5 Skeletons", "Skeleton", 5);
var kill3Ogres = KillTypeCountCriterion.Create("Kill 3 Ogres", "Ogre", 3);
var score10 = ScoreAccumulationCriterion.Create("Score 10", 10);
var score25 = ScoreAccumulationCriterion.Create("Score 25", 25);
var score50 = ScoreAccumulationCriterion.Create("Score 50", 50);
var score100 = ScoreAccumulationCriterion.Create("Score 100", 100);
var deaths1 = DeathCountCriterion.Create("Die Once", 1);
var deaths2 = DeathCountCriterion.Create("Die Twice", 2);

// Example composite usage: achievements referencing overlapping criteria
var aUndeadPressure = AchievementBuilder
    .CreateNew("Undead Pressure", "Kill 10 zombies AND (Score 10 OR Die 2 times)")
    .AllOf("Root AND", all =>
    {
        all.Criterion("10 Zombies", kill10Zombies);
        all.AnyOf("Score OR Deaths", any =>
        {
            any.Criterion("Score 10", score10);
            any.Criterion("Deaths 2", deaths2);
        });
    })
    .Build();

var aRiskyStart = AchievementBuilder
    .CreateNew("Risky Start", "Score 5 AND Die once")
    .AllOf("Score & Death", all =>
    {
        all.Criterion("Score 10 (>=5 placeholder)", score10); // reuse Score10 as minimal example (first 10 covers 5)
        all.Criterion("Die Once", deaths1);
    })
    .Build();

var aBonesOrFortune = AchievementBuilder
    .CreateNew("Bones or Fortune", "Kill 2 skeletons OR Score 50")
    .AnyOf("Either", any =>
    {
        any.Criterion("2 Skeletons", kill2Skeletons);
        any.Criterion("Score 50", score50);
    })
    .Build();

var aBranchingNemesis = AchievementBuilder
    .CreateNew("Branching Nemesis", "Kill 5 skeletons OR 5 zombies")
    .AnyOf("Either Path", any =>
    {
        any.Criterion("5 Skeletons", kill5Skeletons);
        any.Criterion("5 Zombies", kill5Zombies);
    })
    .Build();

var aOgrePressure = AchievementBuilder
    .CreateNew("Ogre Pressure", "Kill 3 ogres AND Score 25")
    .AllOf("AND", all =>
    {
        all.Criterion("3 Ogres", kill3Ogres);
        all.Criterion("Score 25", score25);
    })
    .Build();

var aHighScoreMaster = AchievementBuilder
    .CreateNew("High Score Master", "(Score 100 OR (Score 50 AND Die 0 times)) AND (10 Zombies OR 5 Skeletons)")
    .AllOf("Root", root =>
    {
        root.AnyOf("Score Paths", any =>
        {
            any.Criterion("Score 100", score100);
            any.AllOf("Score 50 AND Survive", all =>
            {
                all.Criterion("Score 50", score50);
                // Survival path: simply requires Deaths1 NOT to trigger; for demo we approximate by absence (no separate criterion)
                // Could implement a dedicated 'NoDeathCriterion'. Skipped for brevity.
            });
        });
        root.AnyOf("Kill Mix", any =>
        {
            any.Criterion("10 Zombies", kill10Zombies);
            any.Criterion("5 Skeletons", kill5Skeletons);
        });
    })
    .Build();

var aGrandMaster = AchievementBuilder
    .CreateNew("Grand Master", "Complex nested demo using shared criteria")
    .AllOf("GM Root", root =>
    {
        root.Criterion("Score 100", score100);
        root.Criterion("10 Zombies", kill10Zombies);
        root.AnyOf("Ogres OR Skeleton/Zombie Mix", any =>
        {
            any.Criterion("3 Ogres", kill3Ogres);
            any.AllOf("Skel/Zom Path", all =>
            {
                all.Criterion("5 Skeletons", kill5Skeletons);
                all.Criterion("5 Zombies", kill5Zombies);
            });
        });
        root.AnyOf("Risk Branch", any =>
        {
            any.AllOf("Risky", all =>
            {
                all.Criterion("Die Twice", deaths2);
                all.Criterion("Score 50", score50);
            });
            any.Criterion("Score 25", score25);
        });
    })
    .Build();

var achievements = new[]
{
    aUndeadPressure,
    aRiskyStart,
    aBonesOrFortune,
    aBranchingNemesis,
    aOgrePressure,
    aHighScoreMaster,
    aGrandMaster
};

var tracker = new AchievementTracker();
tracker.OnUnlocked += (a, when) => Console.WriteLine($"[Unlocked] {a.Name} @ {when:HH:mm:ss}");
foreach (var a in achievements) tracker.Register(a, evaluateImmediately:false);

// Event stream: type, value (enemyType for kills or delta score / deaths)
var events = new (string Kind, string Data, int Amount)[]
{
    ("kill","Zombie",1),
    ("score","+",3),
    ("kill","Skeleton",1),
    ("kill","Zombie",1),
    ("death","+",1),
    ("score","+",7), // Score10 reached
    ("kill","Skeleton",1), // 2 skeletons
    ("kill","Ogre",1),
    ("kill","Zombie",3),
    ("kill","Ogre",1),
    ("kill","Zombie",2),
    ("score","+",15), // Score25 & Score50 partial
    ("kill","Ogre",1), // 3 ogres
    ("kill","Zombie",2),
    ("death","+",1),  // Deaths2
    ("score","+",25), // Score50
    ("score","+",50), // Score100
    ("kill","Skeleton",3),
    ("kill","Zombie",1),
};

Console.WriteLine("Starting push-based achievement simulation...\n");
var step = 0;
foreach (var e in events)
{
    step++;
    switch (e.Kind)
    {
        case "kill":
            // Push enemy type into all kill-related criteria (they internally filter)
            kill10Zombies.Evaluate(e.Data);
            kill5Zombies.Evaluate(e.Data);
            kill2Skeletons.Evaluate(e.Data);
            kill5Skeletons.Evaluate(e.Data);
            kill3Ogres.Evaluate(e.Data);
            break;
        case "score":
            score10.Evaluate(e.Amount);
            score25.Evaluate(e.Amount);
            score50.Evaluate(e.Amount);
            score100.Evaluate(e.Amount);
            break;
        case "death":
            deaths1.Evaluate(e.Amount);
            deaths2.Evaluate(e.Amount);
            break;
    }

    tracker.EvaluateAll();

    Console.WriteLine($"Event {step,-2}: {e.Kind} {e.Data} (+{e.Amount})");
    foreach (var a in achievements)
        Console.WriteLine($"   * {a.Name}: {(a.IsUnlocked ? "UNLOCKED" : "locked")}");
    Console.WriteLine();
}

Console.WriteLine("Final States:\n");
foreach (var a in achievements)
    Console.WriteLine($" - {a.Name}: {(a.IsUnlocked?"UNLOCKED":"locked")}");
