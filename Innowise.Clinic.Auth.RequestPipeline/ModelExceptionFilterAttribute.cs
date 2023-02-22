using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Innowise.Clinic.Auth.Middleware;

public class ModelExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        if (exception is AuthenticationModelException modelException)
        {
            foreach (var error in modelException.Errors)
                context.ModelState.TryAddModelError(error.Code, error.Description);

            context.Result = new BadRequestObjectResult(context.ModelState);
            context.ExceptionHandled = true;
        }
    }
}