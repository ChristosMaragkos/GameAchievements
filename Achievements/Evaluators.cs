namespace Achievements;

public enum EvaluationMode { All, Any }

public interface ICriterionEvaluator
{
    string Label { get; }
    bool Evaluate();
    
    float GetProgress();
}

public sealed class UpdatableEvaluator : ICriterionEvaluator
{
    private readonly IUpdatableCriterion _criterion;
    public string Label { get; }
    public UpdatableEvaluator(string label, IUpdatableCriterion criterion)
    {
        Label = label;
        _criterion = criterion;
    }
    public bool Evaluate() => _criterion.IsSatisfied;
    
    public float GetProgress() => _criterion.GetProgress();
}

public sealed class CompositeEvaluator : ICriterionEvaluator
{
    public readonly List<ICriterionEvaluator> Children = [];
    private readonly EvaluationMode _mode;
    public string Label { get; }

    public CompositeEvaluator(string label, EvaluationMode mode)
    {
        Label = label;
        _mode = mode;
    }

    public CompositeEvaluator With(ICriterionEvaluator evaluator)
    {
        Children.Add(evaluator);
        return this;
    }
    
    public float GetProgress()
    {
        if (Children.Count == 0) return _mode == EvaluationMode.All ? 1f : 0f;
        return _mode == EvaluationMode.All
            ? Children.Average(c => c.GetProgress())
            : Children.Max(c => c.GetProgress());
    }

    public bool Evaluate()
    {
        if (Children.Count == 0) return _mode == EvaluationMode.All;
        return _mode == EvaluationMode.All
            ? Children.All(c => c.Evaluate())
            : Children.Any(c => c.Evaluate());
    }
}
