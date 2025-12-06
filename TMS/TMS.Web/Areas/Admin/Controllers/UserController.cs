using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using TMS;
using TMS.Repository.Managers;
using TMS.ViewModels;
using TMS.ViewModels.Masters;
using TMS.Web.Models;

namespace TMS.Web.Areas.Admin.Controllers
{
    [ValidateFormAccess(Common.FormDefination.User)]
    [Area("Admin")]
    public class UserController : MastersAjaxController<UserViewModel>
    {
        private readonly IAccountManager _accountManager;
        private readonly IMasterManager<EmploymentTypeMasterViewModel> _employmentManager;
        private readonly IMasterManager<LocationMasterViewModel> _locationManager;
        private readonly IMasterManager<DepartmentMasterViewModel> _departmentManager;
        private readonly IMasterManager<DivisionMasterViewModel> _divisionManager;
        private readonly IMasterManager<DesignationMasterViewModel> _designationManager;

        public UserController(IMasterManager<UserViewModel> manager, IAccountManager accountManager
            , IMasterManager<EmploymentTypeMasterViewModel> employmentManager, IMasterManager<LocationMasterViewModel> locationManager
            , IMasterManager<DepartmentMasterViewModel> departmentManager, IMasterManager<DivisionMasterViewModel> divisionManager
            , IMasterManager<DesignationMasterViewModel> designationManager
            )
            : base(manager, "User", "User", new[] { "Role" }, new[] { "UserRoles" })
        {
            _accountManager = accountManager;
            _employmentManager = employmentManager;
            _locationManager = locationManager;
            _departmentManager = departmentManager;
            _divisionManager = divisionManager;
            _designationManager = designationManager;
        } 



        protected override Expression<Func<UserViewModel, bool>>? GetFilter(DataTableRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Search?.Value))
                return null;
            Expression<Func<UserViewModel, bool>> predicate = t => (t.IsActive ? "Active" : "Inactive").Contains(request.Search.Value)
            || (!string.IsNullOrWhiteSpace(t.Role.Name) && t.Role.Name.Contains(request.Search.Value))
            || (!string.IsNullOrWhiteSpace(t.Name) && t.Name.Contains(request.Search.Value))
            || (!string.IsNullOrWhiteSpace(t.Email) && t.Email.Contains(request.Search.Value));
            return predicate;
        }
        protected override List<DataTableColumnsOrder>? GetOrderColumns(DataTableRequest request)
        {
            var columns = new[] { "", "Role.Name", "Name", "Email", "IsActive", "UpdatedByUser.Name,CreatedByUser.Name", "UpdatedOn,CreatedOn" };
            return GetOrderedColumns(columns, request);
        }
        public async Task<IActionResult> GetRolesOptions(int companyId)
        {
            var list = await _accountManager.GetRolesByCompanyId(companyId, 0);
            List<SelectListItem> selectList = new SelectList(list, "Id", "Name").ToList();
            return PartialView("~/Views/Shared/_OptionsPartial.cshtml", selectList);
        }
        public async Task<IActionResult> GetDepartments(int id)
        {
            var departments = await _departmentManager.GetAsync(null, t => t.LocationId == id);
            return GetDropdownList(departments.GetSelectList(dataTextField: "NameStatus"), true);
        }
        public async Task<IActionResult> GetDivision(int id)
        {
            var departments = await _divisionManager.GetAsync(null, t => t.DepartmentId == id);
            return GetDropdownList(departments.GetSelectList(dataTextField: "NameStatus"), true);
        }
        public async Task<IActionResult> GetDesignation(int id)
        {
            var departments = await _designationManager.GetAsync(null, t => t.DivisionId == id);
            return GetDropdownList(departments.GetSelectList(dataTextField: "NameStatus"), true);
        }

        protected override void RemoveFromModelState()
        {
            base.RemoveFromModelState();
            ModelState.Remove("Role");
            //ModelState.Remove("Password");
            ModelState.Remove("UserId");
        }

        protected override void SetEditProperties(ref UserViewModel model)
        {
            if (model.Id == 0)
                model.Password = model.Password?.Encrypt(false);
            if (model.UserRoles != null && !model.UserRoles.Any())
                model.UserRoles = null;
        }

        protected override async Task<AppResultViewModel> IsValidModel(UserViewModel model)
        {
            if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Password))
            {
                return GetResultModelFail("Password is required");
            }
            var resultEmail = await _manager.CheckExpression(t => (model.Id == 0 || t.Id != model.Id)
                    && t.Email == model.Email);
            if (resultEmail)
            {
                return GetResultModelFail("Email already in use");
            }

            return GetResultModelSuccess();
        }
        protected override async Task SetDropdownViewBag(UserViewModel model)
        {

            var roleList = await _accountManager.GetRolesByCompanyId(0, 0);
            ViewBag.RoleId = roleList.GetSelectList();
         
          
            
        }
        [HttpPost]
        public override async Task<IActionResult> Item(UserViewModel model)
        {
            RemoveFromModelState();

            ViewData["Title"] = _masterType;
            if (!ModelState.IsValid)
            {
                return Json(GetFirstModelError(ModelState));
            }
            var validationResult = await IsValidModel(model);
            if (!validationResult.Status)
            {
                return Json(validationResult);
            }
            SetEditProperties(ref model);
            model.Password = EncriptorUtility.Encrypt(model.Password);
            var result = await _manager.AddUpdateAsync(model, this.GetUserId());
            var r = await _accountManager.UpdatePassword(model);
            if (result)
            {
                return Json(GetResultModelSuccess($"{_masterType} {(model.Id == 0 ? "added" : "updated")} successfully"));
            }
            return Json(GetResultModelFail("Something went worng, please contact support"));
        }
    }
}
