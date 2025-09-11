namespace Achievements;

public class Achievement
{
    public string Name { get; }
    public string Description { get; }
    public ICriterionEvaluator Root { get; }

    public string IconPath { get; }
    
    public Achievement(string name, string description, ICriterionEvaluator root, string iconPath = "")
    {
        Name = name;
        Description = description;
        Root = root;
        IconPath = iconPath;
    }

    private bool _wasUnlocked;

    public bool IsUnlocked()
    {
        if (_wasUnlocked || !Root.Evaluate()) return _wasUnlocked;
        _wasUnlocked = true;
        OnUnlocked.Invoke(this);
        return _wasUnlocked;
    }

    public override string ToString() => $"{Name} - {Description}";

    public float GetTotalProgress()
    {
        return Root.GetProgress();
    }

    public event Action<Achievement> OnUnlocked =  delegate { };
    
}

public sealed class AchievementBuilder
{
    private readonly string _name;
    private readonly string _description;
    private readonly CompositeEvaluator _root;
    private readonly Stack<CompositeEvaluator> _stack = new();
    private string _iconPath;

    private CompositeEvaluator Current => _stack.Count > 0 ? _stack.Peek() : _root;

    private AchievementBuilder(string name, string description, string iconPath = "")
    {
        _name = name;
        _description = description;
        _root = new CompositeEvaluator("root", EvaluationMode.All);
        _iconPath = iconPath;
    }

    public static AchievementBuilder CreateNew(string name, string description) => new(name, description);

    public AchievementBuilder AllOf(string label, Action<AchievementBuilder> scope)
    {
        var group = new CompositeEvaluator(label, EvaluationMode.All);
        return CreateGroup(scope, group);
    }

    public AchievementBuilder AnyOf(string label, Action<AchievementBuilder> scope)
    {
        var group = new CompositeEvaluator(label, EvaluationMode.Any);
        return CreateGroup(scope, group);
    }

    private AchievementBuilder CreateGroup(Action<AchievementBuilder> scope, CompositeEvaluator group)
    {
        Current.With(group);
        _stack.Push(group);
        scope(this);
        _stack.Pop();
        return this;
    }

    public AchievementBuilder Criterion(string label, IUpdatableCriterion criterion)
    {
        Current.With(new UpdatableEvaluator(label, criterion));
        return this;
    }

    public AchievementBuilder Icon(string iconPath)
    {
        _iconPath = iconPath;
        return this;
    }

    public Achievement Build()
    {
        return new Achievement(_name, _description, _root, _iconPath);
    }
}