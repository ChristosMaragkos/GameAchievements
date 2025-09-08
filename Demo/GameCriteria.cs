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
    private readonly KillCountCondition _condition;
    public KillCountCriterion(int required) => _condition = new KillCountCondition { RequiredKills = required };
    protected override KillCountCondition Condition => _condition;
}

public sealed class TimePlayedCriterion : AbstractCriterion<TimePlayedCondition>
{
    private readonly TimePlayedCondition _condition;
    public TimePlayedCriterion(double requiredMinutes) => _condition = new TimePlayedCondition { RequiredMinutes = requiredMinutes };
    protected override TimePlayedCondition Condition => _condition;
}

public sealed class ScoreCriterion : AbstractCriterion<ScoreCondition>
{
    private readonly ScoreCondition _condition;
    public ScoreCriterion(int requiredScore) => _condition = new ScoreCondition { RequiredScore = requiredScore };
    protected override ScoreCondition Condition => _condition;
}

