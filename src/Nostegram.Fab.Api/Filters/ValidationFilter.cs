using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nostegram.Fab.Api.Filters;

public class FluentValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            var validator = _serviceProvider.GetService(validatorType);

            if (validator is not IValidator fluentValidator)
                continue;

            var validationContext = new ValidationContext<object>(argument);

            var result = await fluentValidator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(result.ToDictionary())
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "One or more validation errors occurred.",
                        Detail = "One or more validation errors occurred.",
                        Type = "https://httpstatuses.com/400"
                    });

                return;
            }
        }

        await next();
    }
}