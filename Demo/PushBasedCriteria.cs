// -------------------- Example Concrete Push-Based Criteria --------------------

using Achievements;

namespace Demo;

public sealed class KillTypeCountCondition
{
    public string EnemyType { get; init; } = string.Empty;
    public int RequiredCount { get; init; }
}

public sealed class KillTypeCountCriterion : UpdatableCriterion<KillTypeCountCondition, string>
{
    private int _count;
    private KillTypeCountCriterion(string name, KillTypeCountCondition condition) : base(name, condition) { }

    public static KillTypeCountCriterion Create(string name, string enemyType, int required) =>
        new(name, new KillTypeCountCondition { EnemyType = enemyType, RequiredCount = required });

    public override void Evaluate(string enemyType)
    {
        if (IsSatisfied) return; // already done
        if (enemyType == Condition.EnemyType)
        {
            _count++;
            if (_count >= Condition.RequiredCount) IsSatisfied = true;
        }
    }
}

public sealed class ScoreAccumulationCondition
{
    public int RequiredScore { get; init; }
}

public sealed class ScoreAccumulationCriterion : UpdatableCriterion<ScoreAccumulationCondition, int>
{
    private int _score;
    private ScoreAccumulationCriterion(string name, ScoreAccumulationCondition condition) : base(name, condition) { }

    public static ScoreAccumulationCriterion Create(string name, int requiredScore) =>
        new(name, new ScoreAccumulationCondition { RequiredScore = requiredScore });

    public override void Evaluate(int deltaScore)
    {
        if (IsSatisfied) return;
        _score += deltaScore;
        if (_score >= Condition.RequiredScore) IsSatisfied = true;
    }
}

public sealed class DeathCountCondition
{
    public int RequiredDeaths { get; init; }
}

public sealed class DeathCountCriterion : UpdatableCriterion<DeathCountCondition, int>
{
    private int _deaths;
    private DeathCountCriterion(string name, DeathCountCondition condition) : base(name, condition) { }

    public static DeathCountCriterion Create(string name, int requiredDeaths) =>
        new(name, new DeathCountCondition { RequiredDeaths = requiredDeaths });

    public override void Evaluate(int deathIncrement)
    {
        if (IsSatisfied) return;
        _deaths += deathIncrement;
        if (_deaths >= Condition.RequiredDeaths) IsSatisfied = true;
    }
}