namespace Achievements;

// Interfaces moved here so only this file needed for refactor per instructions.
public interface ICriterionCondition<in TContext>
{
    bool IsSatisfied { get; }
    bool Apply(TContext context);
    
    float GetProgress();
}
public interface IResettableCondition { void Reset(); }

// Global bus to notify when any condition transitions to satisfied.
internal static class CriterionEvents
{
    internal sealed record ConditionSatisfied(object CriterionInstance, string ConditionName);
    internal static event Action<ConditionSatisfied> OnConditionSatisfied = delegate { };
    internal static void Raise(object criterionInstance, string conditionName)
        => OnConditionSatisfied(new ConditionSatisfied(criterionInstance, conditionName));
}

public abstract class AbstractCriterion<TCondition, TContext>
    where TCondition : ICriterionCondition<TContext>
{
    private sealed class Entry : IUpdatableCriterion
    {
        public string Name { get; }
        public TCondition Condition { get; }
        public bool IsSatisfied => Condition.IsSatisfied;
        public float GetProgress()
        {
            return Condition.GetProgress();
        }

        public Entry(string name, TCondition condition)
        {
            Name = name;
            Condition = condition;
        }
        public void Apply(TContext ctx) => Condition.Apply(ctx);
    }

    private readonly List<Entry> _entries = [];
    private volatile Entry[] _snapshot = [];
    private readonly object _sync = new();

    /// <summary>
    /// Registers a condition instance under this criterion root and returns an IUpdatableCriterion handle.
    /// </summary>
    public IUpdatableCriterion Create(string name, TCondition condition)
    {
        var e = new Entry(name, condition);
        lock (_sync)
        {
            _entries.Add(e);
            _snapshot = _entries.ToArray();
        }
        return e;
    }

    /// <summary>
    /// Pushes runtime context to all registered conditions.
    /// Raises a global event when any condition transitions to satisfied.
    /// </summary>
    public void Evaluate(TContext context)
    {
        var local = _snapshot; // immutable snapshot for lock-free iteration
        foreach (var entry in local)
        {
            var was = entry.IsSatisfied;
            entry.Apply(context);
            var now = entry.IsSatisfied;
            if (!was && now)
            {
                CriterionEvents.Raise(this, entry.Name);
            }
        }
    }

    /// <summary>
    /// Enumerates current registered condition handles and their condition instances.
    /// </summary>
    public IEnumerable<(string Name, bool IsSatisfied, TCondition Condition)> Handles()
    {
        var local = _snapshot;
        foreach (var e in local)
            yield return (e.Name, e.IsSatisfied, e.Condition);
    }

    /// <summary>
    /// Resets all conditions that support reset semantics (IResettableCondition).
    /// </summary>
    public void ResetAll()
    {
        var local = _snapshot;
        foreach (var e in local)
            if (e.Condition is IResettableCondition r) r.Reset();
    }
}

// Minimal per-achievement handle interface used by evaluators.
public interface IUpdatableCriterion
{
    string Name { get; }
    bool IsSatisfied { get; }

    float GetProgress();
}