namespace Achievements;

public interface ICriterion;

public abstract class AbstractCriterion<T> : ICriterion where T : CriterionCondition<T>
{
    protected abstract T Condition { get; }
    
    public virtual bool IsMet(T context)
    {
        return Condition.RequirementsMet(context);
    }
}

public interface ICondition;

public abstract class CriterionCondition<T> : ICondition where T : ICondition
{
    public abstract bool RequirementsMet(T contextToCompare);
}