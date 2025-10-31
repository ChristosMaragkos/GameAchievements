using System.Text.Json.Serialization;
using Achievements.AchievementLogic;
using Achievements.Criteria;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tests;

public class UnitTest1
{


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
            ConditionsInstance);
        
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
    public void AchievementUnlocks_OnlyAfterRequiredUses()
    {
        Reset();
        while (_timesUsedTool < 9)
        {
            UseTool();
            Assert.NotEmpty(_useToolAchievement.Conditions);
        }
        
        UseTool();
        Assert.Empty(_useToolAchievement.Conditions);
    }

    [Fact]
    public void Achievement_FiresEvent_WhenUnlocked()
    {
        Reset();
        var wasEventFired = false;
        _useToolAchievement.OnUnlocked += _ => wasEventFired = true;
        
        while (_timesUsedTool < 10)
        {
            UseTool();
        }
        
        Assert.True(wasEventFired);
    }
}