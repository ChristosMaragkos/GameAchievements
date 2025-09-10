using Achievements;

namespace Demo;

// Condition types encapsulate requirement + progress. They implement ICriterionCondition<TContext>.
public sealed class KillTypeCountCondition : ICriterionCondition<KillTypeCountCondition>, IResettableCondition
{
    public string EnemyType { get; }
    public int Required { get; }
    private int _count;
    public bool IsSatisfied => _count >= Required;
    public bool Apply(KillTypeCountCondition context)
    {
        if (IsSatisfied) return false;
        if (!string.Equals(context.EnemyType, EnemyType, StringComparison.OrdinalIgnoreCase)) return false;
        _count += context.Required;
        return IsSatisfied;
    }

    public float GetProgress()
    {
        return _count/(float)Required;
    }

    public KillTypeCountCondition(string enemyType, int amountRequired)
    {
        EnemyType = enemyType;
        Required = amountRequired;
    }

    public void Reset() => _count = 0;
}

public sealed class ScoreAccumulationCondition : ICriterionCondition<int>, IResettableCondition
{
    public int RequiredScore { get; }
    private int _score;
    public bool IsSatisfied => _score >= RequiredScore;
    public ScoreAccumulationCondition(int requiredScore) => RequiredScore = requiredScore;
    public bool Apply(int delta)
    {
        if (IsSatisfied) return false;
        _score += delta;
        return IsSatisfied;
    }

    public float GetProgress()
    {
        return  _score / (float)RequiredScore;
    }

    public void Reset() => _score = 0;
}

public sealed class DeathCountCondition : ICriterionCondition<int>, IResettableCondition
{
    public int RequiredDeaths { get; }
    private int _deaths;
    public bool IsSatisfied => _deaths >= RequiredDeaths;
    public DeathCountCondition(int requiredDeaths) => RequiredDeaths = requiredDeaths;
    public bool Apply(int delta)
    {
        if (IsSatisfied) return false;
        _deaths += delta;
        return IsSatisfied;
    }

    public float GetProgress()
    {
        return _deaths / (float)RequiredDeaths;
    }

    public void Reset() => _deaths = 0;
}

public sealed class PlayTimeCondition : ICriterionCondition<int>, IResettableCondition
{
    public int RequiredPlayTime { get; }
    private int _playTime;
    
    public bool IsSatisfied =>  _playTime >= RequiredPlayTime;

    public PlayTimeCondition(int requiredPlayTime) => RequiredPlayTime = requiredPlayTime;

    public bool Apply(int delta)
    {
        if (IsSatisfied) return false;
        _playTime += delta;
        return IsSatisfied;
    }

    public float GetProgress()
    {
        return (float)_playTime / RequiredPlayTime;
    }

    public void Reset()
    {
        _playTime = 0;
    }
}

// Root criterion singletons (stateless, just dispatch to registered conditions via AbstractCriterion infrastructure).
public sealed class KillTypeCountCriterion : AbstractCriterion<KillTypeCountCondition, KillTypeCountCondition>;
public sealed class ScoreCriterion : AbstractCriterion<ScoreAccumulationCondition, int>;
public sealed class DeathCriterion : AbstractCriterion<DeathCountCondition, int>;

public sealed class PlayTimeCriterion : AbstractCriterion<PlayTimeCondition, int>;

public static class Criteria
{
    public static readonly KillTypeCountCriterion Kills = new();
    public static readonly ScoreCriterion Score = new();
    public static readonly DeathCriterion Deaths = new();
    public static readonly PlayTimeCriterion PlayTime = new();
}