namespace PrintHub.API.Filters;

/// <summary>
/// Validates request model state and returns consistent error responses
/// </summary>
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new ValidationError
                {
                    Field = e.Key,
                    Message = err.ErrorMessage
                }))
                .ToList();

            context.Result = new BadRequestObjectResult(new ValidationErrorResponse
            {
                Error = "Validation Failed",
                Message = "One or more validation errors occurred",
                Errors = errors
            });
        }
    }
}

public class ValidationErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}