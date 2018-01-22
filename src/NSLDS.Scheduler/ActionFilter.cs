using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Scheduler
{
    public class ModelStateValidationFilterAttribute : ActionFilterAttribute
    {
        private static readonly string[] _methods = { "POST", "PUT" };
        private const string _revby = "RevBy";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_methods.Contains(context.HttpContext.Request.Method) && !context.ModelState.IsValid)
            {
                // we ignore the RevBy errors in the models
                var err = context.ModelState.Keys.Count(x => x.Contains(_revby));
                // are there other model validation errors?
                if (context.ModelState.ErrorCount > err)
                {
                    foreach (var item in context.ModelState)
                    {
                        if (item.Key.Contains(_revby))
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
