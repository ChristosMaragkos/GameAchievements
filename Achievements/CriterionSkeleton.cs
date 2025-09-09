namespace Achievements;

// Marker interface for non-generic storage/registration
public interface ICriterion { }

// Generic criterion interface allowing runtime context injection
public interface ICriterion<in TContext> : ICriterion
{
    bool IsMet(TContext context);
}

// Condition (requirement) interface – compares a runtime context against stored requirement data
public interface ICondition<in TContext>
{
    bool RequirementsMet(TContext contextToCompare);
}

// Base class for a criterion that owns a requirement (condition) and evaluates a runtime context
public abstract class AbstractCriterion<TCondition, TContext> : ICriterion<TContext>
    where TCondition : ICondition<TContext>
{
    protected abstract TCondition Condition { get; }
    public virtual bool IsMet(TContext context) => Condition.RequirementsMet(context);
}

// Convenience base type for the common case where the requirement type and the runtime context type are the same.
public abstract class AbstractCriterion<TContext> : AbstractCriterion<CriterionCondition<TContext>, TContext>
    where TContext : CriterionCondition<TContext>
{
    // In this self-referential scenario the derived class will still implement the Condition property.
}

// Backwards-compatible style condition base (acts as the requirement object)
public abstract class CriterionCondition<TContext> : ICondition<TContext>
{
    public abstract bool RequirementsMet(TContext contextToCompare);
}