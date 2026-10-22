using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CountrySecure.API.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments)
            {
                var value = argument.Value;
                if (value == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    ValidationResult result = await validator.ValidateAsync(new ValidationContext<object>(value));

                    if (!result.IsValid)
                    {
                        var errors = result.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()
                            );

                        // RFC 7807 (igual que el resto de los errores) + `message` con el primer error para el front
                        var problem = new ValidationProblemDetails(errors)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "Hay datos inválidos en el formulario.",
                            Instance = context.HttpContext.Request.Path
                        };
                        problem.Extensions["message"] = errors.Values.SelectMany(m => m).First();

                        context.Result = new BadRequestObjectResult(problem);
                        return;
                    }
                }
            }

            await next();
        }
    }

}

