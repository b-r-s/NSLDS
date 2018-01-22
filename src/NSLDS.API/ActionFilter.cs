using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace NSLDS.API
{
    public class ModelStateValidationFilterAttribute : ActionFilterAttribute
    {
        private static readonly string[] _methods = { "POST", "PUT" };
        private const string _revby = "RevBy";
        private const string _revon = "RevOn";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_methods.Contains(context.HttpContext.Request.Method) && !context.ModelState.IsValid)
            {
                // we ignore the RevBy/RevOn errors in the models
                var err = context.ModelState.Keys.Count(x => x.Contains(_revby));
                err += context.ModelState.Keys.Count(x => x.Contains(_revon));
                // are there other model validation errors?
                if (context.ModelState.ErrorCount > err)
                {
                    foreach (var item in context.ModelState)
                    {
                        if (item.Key.Contains(_revby) || item.Key.Contains(_revon))
                        {
                            item.Value.Errors.Clear();
                        }
                    }
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
            }
        }
    }
}
