using System;
using System.Collections.Generic;
// using YourRegistrarLibrary; // Uncomment and adjust as needed

public class AchievementTracker
{
    private readonly Dictionary<string, Action> achievementBindings = new();

    // Example: Bind an achievement to a registry value
    public void BindAchievement(string registryKey, Action onAchieved)
    {
        achievementBindings[registryKey] = onAchieved;
        // RegistrarLibrary.Bind(registryKey, () => TriggerAchievement(registryKey));
        // Replace above with actual registrar binding logic
    }

    // Call this when a registry value changes
    public void TriggerAchievement(string registryKey)
    {
        if (achievementBindings.TryGetValue(registryKey, out var action))
        {
            action?.Invoke();
        }
    }

    // ...other tracking logic...
}

