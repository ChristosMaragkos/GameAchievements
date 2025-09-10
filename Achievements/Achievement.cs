namespace Achievements;

public class Achievement
{
    public string Name { get; }
    public string Description { get; }
    public ICriterionEvaluator Root { get; }
    
    public Achievement(string name, string description, ICriterionEvaluator root)
    {
        Name = name;
        Description = description;
        Root = root;
    }
    
    public bool IsUnlocked => Root.Evaluate();

    public override string ToString() => $"{Name} - {Description}";
    
    public float GetTotalProgress()
    {
        return Root.GetProgress();
    }
}

public sealed class AchievementBuilder
{
    private readonly string _name;
    private readonly string _description;
    private readonly CompositeEvaluator _root;
    private readonly Stack<CompositeEvaluator> _stack = new();

    private CompositeEvaluator Current => _stack.Count > 0 ? _stack.Peek() : _root;

    private AchievementBuilder(string name, string description)
    {
        _name = name;
        _description = description;
        _root = new CompositeEvaluator("root", EvaluationMode.All);
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

    public Achievement Build()
    {
        return new Achievement(_name, _description, _root);
    }
}