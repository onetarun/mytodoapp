using TodoApp.Api.Data;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Validators;

public sealed class CreateTaskRequestValidator(AppDbContext dbContext)
    : TaskWriteRequestValidator<CreateTaskRequest>(dbContext);
