using Achievements;

namespace Demo;

// --- Conditions ---
public sealed class KillCountCondition : CriterionCondition<KillCountCondition>
{
    public int Kills { get; init; }
    public int RequiredKills { get; init; }
    public override bool RequirementsMet(KillCountCondition contextToCompare) =>
        contextToCompare.Kills >= RequiredKills;
}

public sealed class TimePlayedCondition : CriterionCondition<TimePlayedCondition>
{
    public double MinutesPlayed { get; init; }
    public double RequiredMinutes { get; init; }
    public override bool RequirementsMet(TimePlayedCondition contextToCompare) =>
        contextToCompare.MinutesPlayed >= RequiredMinutes;
}

public sealed class ScoreCondition : CriterionCondition<ScoreCondition>
{
    public int Score { get; init; }
    public int RequiredScore { get; init; }
    public override bool RequirementsMet(ScoreCondition contextToCompare) =>
        contextToCompare.Score >= RequiredScore;
}

// --- Criteria ---
public sealed class KillCountCriterion : AbstractCriterion<KillCountCondition>
{
    public KillCountCriterion(int required) => Condition = new KillCountCondition { RequiredKills = required };
    protected override KillCountCondition Condition { get; }
}

public sealed class TimePlayedCriterion : AbstractCriterion<TimePlayedCondition>
{
    public TimePlayedCriterion(double requiredMinutes) => Condition = new TimePlayedCondition { RequiredMinutes = requiredMinutes };
    protected override TimePlayedCondition Condition { get; }
}

public sealed class ScoreCriterion : AbstractCriterion<ScoreCondition>
{
    public ScoreCriterion(int requiredScore) => Condition = new ScoreCondition { RequiredScore = requiredScore };
    protected override ScoreCondition Condition { get; }
}

