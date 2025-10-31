using System.Text.Json.Serialization;
using Achievements.Criteria;

namespace Achievements.AchievementLogic;

public class Achievement
{
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }
    [JsonIgnore] public DateTime? UnlockDate { get; private set; }
    [JsonIgnore] public bool IsUnlocked { get; private set; }
    [JsonPropertyName("criteria")] public List<ICondition> Conditions { get; init; }

    [JsonConstructor]
    public Achievement(string name, string description, List<ICondition> conditions)
    {
        Name = name;
        Description = description;
        IsUnlocked = false;
        Conditions = conditions;
    }

    public Achievement(string name, string description, params ICondition[] conditions)
    {
        Name = name;
        Description = description;
        IsUnlocked = false;
        Conditions = conditions.ToList();
    }

    /// <summary>
    /// Sets the current <see cref="Achievement">Achievement</see> to unlocked.
    /// Note that this method does perform any evaluation;
    /// That is strictly the job of the criteria.
    /// </summary>
    public void Unlock()
    {
        IsUnlocked = true;
        UnlockDate = DateTime.Now;
        OnUnlocked.Invoke(this);
    }
    
    public event Action<Achievement> OnUnlocked = delegate { };
}