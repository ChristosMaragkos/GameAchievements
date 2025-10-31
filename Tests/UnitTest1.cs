using System.Text.Json;
using System.Text.Json.Serialization;
using Achievements.AchievementLogic;
using Achievements.Criteria;
using Xunit.Abstractions;

namespace Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region Setup

    public class UseToolCriterion : AbstractCriterion<UseToolConditions>
    {
        public void Trigger(int totalTimesUsed)
        {
            Trigger(conditions => conditions.RequirementsMet(totalTimesUsed));
        }

        public UseToolCriterion(params Achievement[] achievements) : base(achievements)
        {
            
        }
    }
    
    public record UseToolConditions([property: JsonPropertyName("required_times")]int RequiredTimes) : ICondition
    {
        public bool RequirementsMet(int totalTimes)
            => totalTimes >= RequiredTimes;
    }

    private static readonly UseToolConditions ConditionsInstance = new(10);
    
    private static Achievement _useToolAchievement = new(
        "TEST_USE_TOOL", "10_TIMES", 
        ConditionsInstance);

    private static UseToolCriterion _criterionInstance = new(_useToolAchievement);

    private static void Reset()
    {
        _timesUsedTool = 0;
        
        _useToolAchievement = new Achievement(
            "TEST_USE_TOOL", "10_TIMES", 
            new UseToolConditions(10));
        
        _criterionInstance = new UseToolCriterion(_useToolAchievement);
    }

    private static int _timesUsedTool;

    private static void UseTool()
    {
        _timesUsedTool++;
        _criterionInstance.Trigger(_timesUsedTool);
    }

    #endregion
    
    [Fact]
    public void Test1()
    {
        Reset();
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(_useToolAchievement, JsonSerializerOptions));
        while (_timesUsedTool < 9)
        {
            UseTool();
            Assert.NotEmpty(_useToolAchievement.Conditions);
        }
        
        UseTool();
        Assert.Empty(_useToolAchievement.Conditions);
    }
}