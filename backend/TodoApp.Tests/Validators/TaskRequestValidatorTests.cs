using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Validators;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Validators;

public sealed class TaskRequestValidatorTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task TitleOnly_Passes()
    {
        await using var context = CreateContext(UserA);

        var result = await new CreateTaskRequestValidator(context).ValidateAsync(new CreateTaskRequest("Buy milk"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task EmptyTitle_IsRejected(string? title)
    {
        await using var context = CreateContext(UserA);

        var create = await new CreateTaskRequestValidator(context).ValidateAsync(new CreateTaskRequest(title!));
        var update = await new UpdateTaskRequestValidator(context).ValidateAsync(new UpdateTaskRequest(title!));

        create.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskRequest.Title));
        update.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTaskRequest.Title));
    }

    [Fact]
    public async Task TitleOver200_OrDescriptionOver2000_IsRejected()
    {
        await using var context = CreateContext(UserA);
        var validator = new CreateTaskRequestValidator(context);

        (await validator.ValidateAsync(new CreateTaskRequest(new string('a', 200)))).IsValid.Should().BeTrue();
        (await validator.ValidateAsync(new CreateTaskRequest(new string('a', 201))))
            .Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskRequest.Title));
        (await validator.ValidateAsync(new CreateTaskRequest("Ok", new string('d', 2001))))
            .Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskRequest.Description));
    }

    [Fact]
    public async Task UndefinedPriority_IsRejected()
    {
        await using var context = CreateContext(UserA);

        var result = await new CreateTaskRequestValidator(context)
            .ValidateAsync(new CreateTaskRequest("Ok", Priority: (Priority)7));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskRequest.Priority));
    }

    [Fact]
    public async Task OwnCategory_Passes_ButAnotherUsersOrDeletedCategory_IsRejected()
    {
        var ownId = await SeedCategoryAsync(UserA, "Mine", isDeleted: false);
        var deletedId = await SeedCategoryAsync(UserA, "Gone", isDeleted: true);
        var otherId = await SeedCategoryAsync(UserB, "Theirs", isDeleted: false);
        await using var context = CreateContext(UserA);
        var create = new CreateTaskRequestValidator(context);
        var update = new UpdateTaskRequestValidator(context);

        (await create.ValidateAsync(new CreateTaskRequest("Ok", CategoryId: ownId))).IsValid.Should().BeTrue();
        (await update.ValidateAsync(new UpdateTaskRequest("Ok", CategoryId: ownId))).IsValid.Should().BeTrue();

        foreach (var categoryId in new[] { otherId, deletedId, 999 })
        {
            var createResult = await create.ValidateAsync(new CreateTaskRequest("Ok", CategoryId: categoryId));
            var updateResult = await update.ValidateAsync(new UpdateTaskRequest("Ok", CategoryId: categoryId));

            createResult.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateTaskRequest.CategoryId))
                .Which.ErrorMessage.Should().Be(CreateTaskRequestValidator.CategoryNotFoundMessage);
            updateResult.IsValid.Should().BeFalse();
        }
    }

    private async Task<int> SeedCategoryAsync(string userId, string name, bool isDeleted)
    {
        await using var context = CreateContext(userId);
        var category = new Category { UserId = userId, Name = name, IsDeleted = isDeleted };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category.Id;
    }

    private AppDbContext CreateContext(string userId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AppDbContext(options, new FakeCurrentUserService(userId));
    }
}
