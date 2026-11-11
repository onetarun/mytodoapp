using FluentAssertions;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Services;

public sealed class TaskRulesTests
{
    private static readonly DateTime Now = new(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(false, -1, true)]
    [InlineData(true, -1, false)]
    [InlineData(false, 1, false)]
    [InlineData(false, 0, false)]
    public void IsOverdue_AndExpression_AgreeOnTheRule(bool isCompleted, int dueOffsetMinutes, bool expected)
    {
        var dueDate = Now.AddMinutes(dueOffsetMinutes);
        var task = new TodoTask { IsCompleted = isCompleted, DueDate = dueDate };

        TaskRules.IsOverdue(isCompleted, dueDate, Now).Should().Be(expected);
        TaskRules.IsOverdueExpression(Now).Compile()(task).Should().Be(expected);
    }

    [Fact]
    public void IsOverdue_WithoutDueDate_IsFalse()
    {
        TaskRules.IsOverdue(false, null, Now).Should().BeFalse();
        TaskRules.IsOverdueExpression(Now).Compile()(new TodoTask()).Should().BeFalse();
    }
}
