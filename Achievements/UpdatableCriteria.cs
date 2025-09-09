namespace Achievements;

/// <summary>
/// Non-generic base for any criterion that keeps internal progress/state and exposes a satisfied flag.
/// External code pushes context into such criteria via Evaluate(context) on the generic form.
/// Achievements composed with these criteria only need to read IsSatisfied (no context providers required).
/// </summary>
public interface IUpdatableCriterion
{
    string Name { get; }
    bool IsSatisfied { get; }
}

/// <summary>
/// Generic updatable criterion that receives runtime context externally and updates internal progress.
/// </summary>
public interface IUpdatableCriterion<in TContext> : IUpdatableCriterion
{
    void Evaluate(TContext context);
}

/// <summary>
/// Base class for push / event driven criteria where requirements are encapsulated in a condition object.
/// </summary>
public abstract class UpdatableCriterion<TCondition, TContext> : IUpdatableCriterion<TContext>
    where TCondition : class
{
    public string Name { get; }
    protected TCondition Condition { get; }
    public bool IsSatisfied { get; protected set; }

    protected UpdatableCriterion(string name, TCondition condition)
    {
        Name = name;
        Condition = condition;
    }

    public abstract void Evaluate(TContext context);
}

