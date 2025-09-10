using Achievements;
using Demo;

// Simplified criteria demo using generic CriterionRoot + condition objects.
// Final expected states:
//  - Undead Pressure: locked
//  - Risky Start: UNLOCKED
//  - Bones or Fortune: UNLOCKED
//  - Branching Nemesis: UNLOCKED
//  - Ogre Pressure: UNLOCKED
//  - High Score Master: locked
//  - Grand Master: locked
//  - #NoLife: UNLOCKED

// Create per-achievement handles via root criteria registry (condition instances carry progress).
var hKill10Zombies   = Criteria.Kills.Create("Kill 10 Zombies", new KillTypeCountCondition("Zombie", 10));
var hKill5Zombies    = Criteria.Kills.Create("Kill 5 Zombies",  new KillTypeCountCondition("Zombie", 5));
var hKill2Skeletons  = Criteria.Kills.Create("Kill 2 Skeletons",new KillTypeCountCondition("Skeleton", 2));
var hKill5Skeletons  = Criteria.Kills.Create("Kill 5 Skeletons",new KillTypeCountCondition("Skeleton", 5));
var hKill3Ogres      = Criteria.Kills.Create("Kill 3 Ogres",    new KillTypeCountCondition("Ogre", 3));

var hScore5   = Criteria.Score.Create("Score 5",   new ScoreAccumulationCondition(5));
var hScore10  = Criteria.Score.Create("Score 10",  new ScoreAccumulationCondition(10));
var hScore25  = Criteria.Score.Create("Score 25",  new ScoreAccumulationCondition(25));
var hScore50  = Criteria.Score.Create("Score 50",  new ScoreAccumulationCondition(50));
var hScore100 = Criteria.Score.Create("Score 100", new ScoreAccumulationCondition(100));

var hDieOnce  = Criteria.Deaths.Create("Die Once",  new DeathCountCondition(1));
var hDieTwice = Criteria.Deaths.Create("Die Twice", new DeathCountCondition(2));

var hPlay10Minutes = Criteria.PlayTime.Create("Play 10 minutes",new PlayTimeCondition(10));

// Achievement definitions.
var aUndeadPressure = AchievementBuilder
    .CreateNew("Undead Pressure", "Kill 10 zombies AND (Score 10 OR Die 2 times)")
    .AllOf("Root", all =>
    {
        all.Criterion("10 Zombies", hKill10Zombies);
        all.AnyOf("Score10 OR Deaths2", any =>
        {
            any.Criterion("Score 10", hScore10);
            any.Criterion("Die Twice", hDieTwice);
        });
    })
    .Build();

var aRiskyStart = AchievementBuilder
    .CreateNew("Risky Start", "Score 5 AND Die once")
    .AllOf("Both", all =>
    {
        all.Criterion("Score 5", hScore5);
        all.Criterion("Die Once", hDieOnce);
    })
    .Build();

var aBonesOrFortune = AchievementBuilder
    .CreateNew("Bones or Fortune", "Kill 2 skeletons OR Score 50")
    .AnyOf("Either", any =>
    {
        any.Criterion("2 Skeletons", hKill2Skeletons);
        any.Criterion("Score 50", hScore50);
    })
    .Build();

var aBranchingNemesis = AchievementBuilder
    .CreateNew("Branching Nemesis", "Kill 5 skeletons OR 5 zombies")
    .AnyOf("Either", any =>
    {
        any.Criterion("5 Skeletons", hKill5Skeletons);
        any.Criterion("5 Zombies", hKill5Zombies);
    })
    .Build();

var aOgrePressure = AchievementBuilder
    .CreateNew("Ogre Pressure", "Kill 3 ogres AND Score 25")
    .AllOf("Both", all =>
    {
        all.Criterion("3 Ogres", hKill3Ogres);
        all.Criterion("Score 25", hScore25);
    })
    .Build();

var aHighScoreMaster = AchievementBuilder
    .CreateNew("High Score Master", "(Score 100) AND (10 Zombies OR 5 Skeletons)")
    .AllOf("Root", root =>
    {
        root.Criterion("Score 100", hScore100);
        root.AnyOf("Kills", any =>
        {
            any.Criterion("10 Zombies", hKill10Zombies);
            any.Criterion("5 Skeletons", hKill5Skeletons);
        });
    })
    .Build();

var aGrandMaster = AchievementBuilder
    .CreateNew("Grand Master", "Score 100, Kill 10 Zombies, and branching extras")
    .AllOf("GM Root", root =>
    {
        root.Criterion("Score 100", hScore100);
        root.Criterion("10 Zombies", hKill10Zombies);
        root.AnyOf("Ogres OR Mixed", any =>
        {
            any.Criterion("3 Ogres", hKill3Ogres);
            any.AllOf("Skeleton/Zombie Mix", all =>
            {
                all.Criterion("5 Skeletons", hKill5Skeletons);
                all.Criterion("5 Zombies", hKill5Zombies);
            });
        });
        root.AnyOf("Risk Branch", any =>
        {
            any.AllOf("Die/Score Path", all =>
            {
                all.Criterion("Die Twice", hDieTwice);
                all.Criterion("Score 50", hScore50);
            });
            any.Criterion("Score 25", hScore25);
        });
    })
    .Build();

var aNoLife = AchievementBuilder
    .CreateNew("#NoLife", "Play for 10 minutes")
    .AllOf("Play 10 Minutes", root =>
    {
        root.Criterion("Play 10 Minutes", hPlay10Minutes);
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
    aGrandMaster,
    aNoLife
};

var tracker = new AchievementTracker();
tracker.OnUnlocked += (a, when) => Console.WriteLine($"[Unlocked] {a.Name} @ {when:HH:mm:ss}");
foreach (var a in achievements) tracker.Register(a, evaluateImmediately:false);

var events = new (string Kind, string Data, int Amount)[]
{
    ("kill","Skeleton",1), ("score","+",3), ("kill","Skeleton",1), ("death","+",1), ("score","+",2),
    ("kill","Ogre",1), ("score","+",5), ("kill","Skeleton",1), ("kill","Zombie",1), ("kill","Skeleton",1),
    ("score","+",10), ("kill","Ogre",1), ("kill","Zombie",1), ("kill","Skeleton",1), ("score","+",10),
    ("kill","Ogre",1), ("kill","Zombie",1), ("kill","Zombie",1),("time", "+", 5),("time","+",5)
};

Console.WriteLine("Starting simplified achievement simulation...\n");
var step = 0;
foreach (var e in events)
{
    step++;
    switch (e.Kind)
    {
        case "kill": Criteria.Kills.Evaluate(new KillTypeCountCondition(e.Data, e.Amount)); break;
        case "score": Criteria.Score.Evaluate(e.Amount); break;
        case "death": Criteria.Deaths.Evaluate(e.Amount); break;
        case "time":
        {
            Criteria.PlayTime.Evaluate(e.Amount);
            Console.WriteLine($"Play time current progress: {hPlay10Minutes.GetProgress():P1}");
            break;
        }
    }
    tracker.EvaluateAll();

    Console.WriteLine($"Event {step,-2}: {e.Kind} {e.Data} (+{e.Amount})");
    foreach (var a in achievements)
        Console.WriteLine($"   * {a.Name}: {(a.IsUnlocked() ? "UNLOCKED" : "locked")}");
    Console.WriteLine();
}

Console.WriteLine("Final States:\n");
foreach (var a in achievements)
    Console.WriteLine($" - {a.Name}: {(a.IsUnlocked()?"UNLOCKED":"locked")}");
