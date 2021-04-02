using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroCommerce.Catalog.API.Infrastructure.Filters
{
    public class TranslateResultToActionResultAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Controller is not ControllerBase controller) return;

            if ((context.Result as ObjectResult)?.Value is not IResult result) return;

            if (result.IsFailure)
            {
                if (result is IResult<object> resultWithType)
                {
                    context.Result = controller.BadRequest(new { resultWithType.Error });
                }
                else
                {
                    context.Result = controller.BadRequest();
                }
            }

            if (result.IsSuccess)
            {
                if (result is IResult<object> resultWithType)
                {
                    context.Result = controller.Ok(resultWithType.Value);
                }
                else
                {
                    context.Result = controller.Ok();
                }
            }
        }
    }
}
