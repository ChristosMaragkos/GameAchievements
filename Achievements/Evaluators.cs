namespace Achievements;

public enum EvaluationMode { All, Any }

public interface ICriterionEvaluator
{
    string Label { get; }
    bool Evaluate();
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
}

public sealed class CompositeEvaluator : ICriterionEvaluator
{
    private readonly List<ICriterionEvaluator> _children = [];
    private readonly EvaluationMode _mode;
    public string Label { get; }

    public CompositeEvaluator(string label, EvaluationMode mode)
    {
        Label = label;
        _mode = mode;
    }

    public CompositeEvaluator With(ICriterionEvaluator evaluator)
    {
        _children.Add(evaluator);
        return this;
    }

    public bool Evaluate()
    {
        if (_children.Count == 0) return _mode == EvaluationMode.All;
        return _mode == EvaluationMode.All
            ? _children.All(c => c.Evaluate())
            : _children.Any(c => c.Evaluate());
    }
}
