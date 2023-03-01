using System.Text;
using System.Text.Json;
using Innowise.Clinic.Auth.Exceptions.AccountBlockingService;
using Innowise.Clinic.Auth.Exceptions.UserManagement;
using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Middleware;

public class AuthenticationExceptionHandlingMiddleware : IMiddleware
{
    private const string DefaultUnhandledErrorMessage =
        "The error occured during this operation. Please, try again later.";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AccountBlockedException)
        {
            var publicException = new InvalidCredentialsProvidedException();
            context.Response.StatusCode = publicException.StatusCode;
            await WriteExceptionMessageToResponse(publicException.Message, context);
        }
        catch (AuthenticationException e)
        {
            context.Response.StatusCode = e.StatusCode;
            await WriteExceptionMessageToResponse(e.Message, context);
        }
        catch (SecurityTokenValidationException e)
        {
            context.Response.StatusCode = 401;
            await WriteExceptionMessageToResponse(e.Message, context);
        }
        catch (ApplicationException e)
        {
            context.Response.StatusCode = 400;
            await WriteExceptionMessageToResponse(e.Message, context);
        }
        catch (Exception e)
        {
            context.Response.StatusCode = 500;
            await WriteExceptionMessageToResponse(DefaultUnhandledErrorMessage, context);
        }
    }

    private async Task WriteExceptionMessageToResponse(string message, HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var jsonMessage = JsonSerializer.Serialize(message);
        await context.Response.WriteAsync(jsonMessage, Encoding.UTF8);
    }
}