using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using TodoApp.Api.Controllers;
using TodoApp.Api.DTOs.Categories;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Controllers;

public sealed class CategoriesControllerTests
{
    private static readonly CategoryDto Work = new(7, "Work", 2);

    private readonly Mock<ICategoryService> _service = new();

    [Fact]
    public async Task GetAll_Returns200WithCategories()
    {
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([Work]);

        var result = await CreateController().GetAllAsync(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(new[] { Work });
    }

    [Fact]
    public async Task Create_Success_Returns201WithLocation()
    {
        var request = new CreateCategoryRequest("Work");
        _service.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CategoryDto>.Success(Work));

        var result = await CreateController().CreateAsync(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("/api/v1/categories/7");
        created.Value.Should().Be(Work);
    }

    [Fact]
    public async Task Create_Duplicate_Returns400()
    {
        var request = new CreateCategoryRequest("Work");
        _service.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CategoryDto>.ValidationFailed(CategoryService.NameField, CategoryService.DuplicateNameMessage));

        var result = await CreateController().CreateAsync(request, CancellationToken.None);

        var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>()
            .Which.Errors[CategoryService.NameField].Should().Contain(CategoryService.DuplicateNameMessage);
    }

    [Fact]
    public async Task Update_Success_Returns200()
    {
        var request = new UpdateCategoryRequest("Work");
        _service.Setup(s => s.UpdateAsync(7, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CategoryDto>.Success(Work));

        var result = await CreateController().UpdateAsync(7, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(Work);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var request = new UpdateCategoryRequest("Work");
        _service.Setup(s => s.UpdateAsync(7, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CategoryDto>.NotFound());

        var result = await CreateController().UpdateAsync(7, request, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(true, typeof(NoContentResult))]
    [InlineData(false, typeof(NotFoundResult))]
    public async Task Delete_Returns204OrNotFound(bool deleted, Type expected)
    {
        _service.Setup(s => s.DeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(deleted);

        var result = await CreateController().DeleteAsync(7, CancellationToken.None);

        result.Should().BeOfType(expected);
    }

    private CategoriesController CreateController()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        return new CategoriesController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
            },
        };
    }
}
