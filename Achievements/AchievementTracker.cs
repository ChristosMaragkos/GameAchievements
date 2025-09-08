namespace Achievements;

public interface IAchievementStore
{
    bool TryGetUnlocked(string key, out DateTime unlockedAt);
    void MarkUnlocked(string key, DateTime unlockedAt);
}

public sealed class InMemoryAchievementStore : IAchievementStore
{
    private readonly Dictionary<string, DateTime> _map = new();
    public bool TryGetUnlocked(string key, out DateTime unlockedAt) => _map.TryGetValue(key, out unlockedAt);
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
    private readonly Dictionary<string, AchievementEntry> _entries = new();

    public event Action<Achievement, string, DateTime> OnUnlocked = delegate { };

    public void Register(Achievement achievement, string? key = null, bool evaluateImmediately = true)
    {
        var k = key ?? achievement.Name;
        if (_entries.ContainsKey(k))
            throw new InvalidOperationException($"Achievement with key '{k}' already registered.");

        DateTime? unlockedAt = null;
        var wasUnlocked = false;
        if (_store.TryGetUnlocked(k, out var dt))
        {
            wasUnlocked = true;
            unlockedAt = dt;
        }

        var entry = new AchievementEntry(k, achievement, wasUnlocked, unlockedAt);
        _entries[k] = entry;

        if (evaluateImmediately)
        {
            Evaluate(k);
        }
    }

    public IReadOnlyCollection<string> Keys => _entries.Keys;

    public bool IsUnlocked(string key) => _entries.TryGetValue(key, out var e) && (e.WasUnlockedMutable || e.Achievement.IsUnlocked());

    public DateTime? GetUnlockedAt(string key) => _entries.TryGetValue(key, out var e) ? e.UnlockedAtMutable : null;

    public IEnumerable<(string Key, Achievement Achievement, bool IsUnlocked, DateTime? UnlockedAt)> Snapshot()
    {
        return _entries.Values.Select(e => (e.Key, e.Achievement, e.WasUnlockedMutable, e.UnlockedAtMutable));
    }

    public int EvaluateAll()
    {
        return _entries.Keys.ToArray().Sum(key => Evaluate(key) ? 1 : 0);
    }

    public bool Evaluate(string key)
    {
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
        OnUnlocked(e.Achievement, key, now);
        return true;
    }
}

