using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using TMS.Repository.Managers;
using TMS.ViewModels.Masters;
using TMS.Web.Models;

namespace TMS.Web.Areas.Admin.Controllers
{
    [ValidateFormAccess(Common.FormDefination.CourseMaster)]
    [Area("Admin")]
    public class CourseController : MastersAjaxController<CourseMasterViewModel>
    {
        private readonly IMasterManager<CourseCategoryMasterViewModel> _courseCategoryManager;
        private readonly IMasterManager<SemesterMasterViewModel> _semesterManager;

        public CourseController(IMasterManager<CourseMasterViewModel> manager, IMasterManager<CourseCategoryMasterViewModel> courseCategoryManager
            ,IMasterManager<SemesterMasterViewModel> semesterManager)
            : base(manager, "Course", "Course", new[] { "CourseCategory" }, new[] { "CourseCategory"})
        {
            _courseCategoryManager = courseCategoryManager;
            _semesterManager = semesterManager;
            
        }

        protected override Expression<Func<CourseMasterViewModel, bool>>? GetFilter(DataTableRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Search?.Value))
                return null;
            Expression<Func<CourseMasterViewModel, bool>> predicate = t => (t.IsActive ? "Active" : "Inactive").Contains(request.Search.Value)
            || (!string.IsNullOrWhiteSpace(t.CourseCode) && t.CourseCode.Contains(request.Search.Value))
            || (!string.IsNullOrWhiteSpace(t.CourseCategory.Name) && t.CourseCategory.Name.Contains(request.Search.Value))
            || (!string.IsNullOrWhiteSpace(t.Name) && t.Name.Contains(request.Search.Value));
       
            return predicate;
        }
        protected override List<DataTableColumnsOrder>? GetOrderColumns(DataTableRequest request)
        {
            var columns = new[] { "", "CourseCode", "CourseCategory.Name", "Name", };
            return GetOrderedColumns(columns, request);
        }
        protected async override Task SetDropdownViewBag(CourseMasterViewModel model)
        {
            var semlist = await _semesterManager.GetAsync(null, t => t.IsActive || t.Id == model.SemesterId);
            ViewBag.SemesterId = semlist.GetSelectList(model.SemesterId, dataTextField: "NameStatus");
            var list = await _courseCategoryManager.GetAsync(null,
              t => t.IsActive || t.Id == model.CourseCategoryId);
            ViewBag.CourseCategoryId = list.GetSelectList(model.CourseCategoryId, dataTextField: "NameStatus");

        }
        protected override async Task<AppResultViewModel> IsValidModel(CourseMasterViewModel model)
        {
            var result = await base.IsValidModel(model);
            if (!result.Status)
            {
                return result;
            }
            var resultIdCheck = await _manager.CheckExpression(t => (model.Id == 0 || t.Id != model.Id)
                    && t.CourseCode == model.CourseCode);
            if (resultIdCheck)
            {
                return GetResultModelFail("Course Code Id already in use");
            }
            return GetResultModelSuccess();
        }
    }
}
