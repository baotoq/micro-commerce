using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using MicroCommerce.Shared.MediatR.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroCommerce.Catalog.API.Infrastructure.Filters
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ValidationException exception:
                    new ValidationResult(exception.Errors).AddToModelState(context.ModelState, null);
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
                    break;
                case NotFoundException exception:
                    context.Result = new NotFoundObjectResult(exception.Message);
                    break;
            }
        }
    }
}
