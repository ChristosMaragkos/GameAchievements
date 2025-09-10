namespace Achievements;

public interface IAchievementStore
{
    bool TryGetUnlockDate(string key, out DateTime unlockedAt);
    void MarkUnlocked(string key, DateTime unlockedAt);
}

public sealed class InMemoryAchievementStore : IAchievementStore
{
    private readonly Dictionary<string, DateTime> _map = new();
    public bool TryGetUnlockDate(string key, out DateTime unlockedAt) => _map.TryGetValue(key, out unlockedAt);
    public void MarkUnlocked(string key, DateTime unlockedAt) => _map[key] = unlockedAt;
}

public sealed class AchievementTracker(IAchievementStore? store = null)
{
    private sealed record AchievementEntry(string Key, Achievement Achievement)
    {
        public bool WasUnlockedMutable;
        public DateTime? UnlockedAtMutable;

        public AchievementEntry(string key, Achievement achievement, bool wasUnlocked, DateTime? unlockedAt)
            : this(key, achievement)
        {
            WasUnlockedMutable = wasUnlocked;
            UnlockedAtMutable = unlockedAt;
        }
    }

    private readonly IAchievementStore _store = store ?? new InMemoryAchievementStore();
    private readonly Dictionary<string, AchievementEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public event Action<Achievement, DateTime> OnUnlocked = delegate { };

    public void Register(Achievement achievement, bool evaluateImmediately = true)
    {
        var key = achievement.Name;
        if (_entries.ContainsKey(key))
            throw new InvalidOperationException($"Achievement with name '{key}' already registered.");

        DateTime? unlockedAt = null;
        var wasUnlocked = false;
        if (_store.TryGetUnlockDate(key, out var dt))
        {
            wasUnlocked = true;
            unlockedAt = dt;
        }

        var entry = new AchievementEntry(key, achievement, wasUnlocked, unlockedAt);
        _entries[key] = entry;

        if (evaluateImmediately)
        {
            Evaluate(achievement);
        }
    }

    public bool IsUnlocked(Achievement achievement) =>
        _entries.TryGetValue(achievement.Name,
            out var e) &&
        (e.WasUnlockedMutable || e.Achievement.IsUnlocked());

    public DateTime? GetUnlockDate(Achievement achievement) => _entries.TryGetValue(achievement.Name, out var e) ? e.UnlockedAtMutable : null;

    public IEnumerable<(string Key, Achievement Achievement, bool IsUnlocked, DateTime? UnlockedAt)> Snapshot()
    {
        return _entries.Values.Select(e => (e.Key, e.Achievement, e.WasUnlockedMutable, e.UnlockedAtMutable));
    }

    public int EvaluateAll()
    {
        return _entries.Values.Count(entry => Evaluate(entry.Achievement));
    }

    /// <summary>
    /// Evaluates a single achievement by key, firing OnUnlocked if it transitions to unlocked.
    /// Returns true if it was just unlocked, false otherwise.
    /// </summary>
    /// <param name="achievement">The achievement to evaluate.</param>
    /// <remarks>It is best practice to retain a static read-only reference to the </remarks>
    /// <returns>Whether the achievement was unlocked.</returns>
    public bool Evaluate(Achievement achievement)
    {
        var key = achievement.Name;
        
        if (!_entries.TryGetValue(key, out var e)) return false;

        if (e.WasUnlockedMutable)
            return false; // already recorded as unlocked

        if (!e.Achievement.IsUnlocked())
            return false; // still locked

        // transition to unlocked
        var now = DateTime.UtcNow;
        e.WasUnlockedMutable = true;
        e.UnlockedAtMutable = now;
        _store.MarkUnlocked(key, now);
        OnUnlocked(e.Achievement, now);
        return true;
    }
}



