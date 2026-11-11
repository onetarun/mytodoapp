using TodoApp.Api.Data;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Validators;

public sealed class UpdateTaskRequestValidator(AppDbContext dbContext)
    : TaskWriteRequestValidator<UpdateTaskRequest>(dbContext);
