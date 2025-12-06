using TMS.Common;
using TMS.Repository.Managers;
using TMS.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TMS.Web
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidateFormAccessAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly int _formId;

        public ValidateFormAccessAttribute(FormDefination Form)
        {
            _formId = (int)Form;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userPermissions = context.HttpContext?.Session.GetObjectFromJson<UserPermissions>("UserPermissions");
            if (userPermissions == null)
            {
                var accountManager = context.HttpContext?.RequestServices.GetService(typeof(IAccountManager)) as IAccountManager;
                var userId = (user as System.Security.Claims.ClaimsPrincipal)?.Claims?.Where(t => t.Type == "userId")?.Select(t => t.Value)?.FirstOrDefault();
                if (int.TryParse(userId, out int userIdInt))
                    userPermissions = accountManager?.GetUserPermissions(userIdInt).GetAwaiter().GetResult();
                if (userPermissions == null)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                    return;
                }
                context.HttpContext?.Session.SetObjectAsJson("UserPermissions", userPermissions);
            }

            var permission = userPermissions.Forms.Where(t => t.Id == _formId).FirstOrDefault();
            if (permission == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
            if (!permission.View)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
            if (context.HttpContext != null)
            {
                context.HttpContext.Items["ViewPermission"] = permission.View ? 1 : 0;
                context.HttpContext.Items["AddPermission"] = permission.Add ? 1 : 0;
                context.HttpContext.Items["EditPermission"] = permission.Edit ? 1 : 0;
            }
            return;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateFormAccessActionAttribute : ValidateFormAccessAttribute
    {
        public ValidateFormAccessActionAttribute(FormDefination Form) : base(Form)
        {

        }
    }
}
