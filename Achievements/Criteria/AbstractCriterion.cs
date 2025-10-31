using Achievements.AchievementLogic;

namespace Achievements.Criteria;

public abstract class AbstractCriterion;

public interface ICondition;

public abstract class AbstractCriterion<TConditions> : AbstractCriterion
    where TConditions : ICondition
{
    protected readonly List<Achievement> Achievements;

    protected AbstractCriterion(params Achievement[] achievements)
    {
        Achievements = achievements.ToList();
    }
    
    protected void Trigger(Predicate<TConditions> predicate)
    {
        foreach (var achievement in Achievements.ToList())
        {
            foreach (var condition in achievement.Conditions.ToList())
            {
                if (condition is TConditions realConditions && predicate(realConditions))
                {
                    achievement.Conditions.Remove(condition);
                }
            }

            if (achievement.Conditions.Count != 0) continue;
            
            // If all the achievement's conditions have been satisfied
            // (i.e. the conditions list is empty), we consider the achievement unlocked.
            achievement.Unlock();
            Achievements.Remove(achievement);
        }
    }
}