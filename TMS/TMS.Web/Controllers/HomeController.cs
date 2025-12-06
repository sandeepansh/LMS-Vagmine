using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using TMS.Common;
using TMS.Repository.Managers;
using TMS.Utilities;
using TMS.ViewModels;
using TMS.ViewModels.Account;
using TMS.ViewModels.Masters;
using TMS.Web.Models;
using TMS.Web.Models.ViewModels;

namespace TMS.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly StorageSettings _storageSettings;
        private readonly IMasterManager<BadgeMasterViewModel> _badgeManager;
        private readonly IMasterManager<CertificateMasterViewModel> _certificateManager;
        private readonly IMasterManager<CompanyMasterViewModel> _companyManager;
        private readonly IMasterManager<CourseCategoryMasterViewModel> _courseCategoryManager;
        private readonly IMasterManager<CourseMasterViewModel> _courseManager;
        private readonly IMasterManager<DepartmentMasterViewModel> _departmentManager;
        private readonly IMasterManager<DesignationMasterViewModel> _designationManager;
        private readonly IMasterManager<DivisionMasterViewModel> _divisionManager;
        private readonly IMasterManager<EmploymentTypeMasterViewModel> _employmentTypeManager;
        private readonly IMasterManager<LocationMasterViewModel> _locationManager;
        private readonly IMasterBaseManager<PhotoGalleryMasterViewModel> _photoGalleryManager;
        private readonly IMasterManager<SignatureMasterViewModel> _signatureManager;
        private readonly IMasterManager<VenueMasterViewModel> _venueManager;

        private readonly IMasterBaseManager<CourseEnrollmentViewModel> _enrollmentManager;
        private readonly IMasterBaseManager<LectureMaterialViewModel> _lectureMaterialManager;
        private readonly IMasterManager<UserViewModel> _userManager; 


        public HomeController(
            IMasterManager<BadgeMasterViewModel> badgeManager
            , IMasterManager<CertificateMasterViewModel> certificateManager
            , IMasterManager<CompanyMasterViewModel> companyManager
            , IMasterManager<CourseCategoryMasterViewModel> courseCategoryManager
            , IMasterManager<CourseMasterViewModel> courseManager
            , IMasterManager<DepartmentMasterViewModel> departmentManager
            , IMasterManager<DesignationMasterViewModel> designationManager
            , IMasterManager<DivisionMasterViewModel> divisionManager
            , IMasterManager<EmploymentTypeMasterViewModel> employmentTypeManager
            , IMasterManager<LocationMasterViewModel> locationManager
            , IMasterBaseManager<PhotoGalleryMasterViewModel> photoGalleryManager
            , IMasterManager<SignatureMasterViewModel> signatureManager
            , IMasterManager<VenueMasterViewModel> venueManager
            , IMasterBaseManager<LectureMaterialViewModel> lectureMaterialManager
            , IMasterManager<UserViewModel> userManager
           , IMasterBaseManager<CourseEnrollmentViewModel> enrollmentManager



            //ILogger<HomeController> logger,
            , IOptions<StorageSettings> options)
        {
            //_logger = logger;
            _storageSettings = options.Value;
            _badgeManager = badgeManager;
            _certificateManager = certificateManager;
            _companyManager = companyManager;
            _courseCategoryManager = courseCategoryManager;
            _courseManager = courseManager;
            _departmentManager = departmentManager;
            _designationManager = designationManager;
            _divisionManager = divisionManager;
            _employmentTypeManager = employmentTypeManager;
            _locationManager = locationManager;
            _photoGalleryManager = photoGalleryManager;
            _signatureManager = signatureManager;
            _venueManager = venueManager;
            _userManager = userManager;
            _lectureMaterialManager = lectureMaterialManager;
            _enrollmentManager = enrollmentManager;

        }

        public IActionResult Index()
        {
            var userRole=GetUserRole();
            if(userRole.ToLower()=="student")
                return RedirectToAction("StudentDashboard", "Home");

            return View();
        }

        public async Task<IActionResult> DashboardUser()
        {
            var courses = await _courseManager.GetAsync();
            var materials = await _lectureMaterialManager.GetAsync();
            var users = await _userManager.GetAsync(new[] { "Role" });
            var enrollments = await _enrollmentManager.GetAsync(
                includes: new[] { "Course", "Course.CourseCategory" },
                predicate: e => e.IsActive
            );

            var students = users?.Where(u => u.Role?.Name?.ToLower() == "student").ToList();

            // Group enrollments by Course Category
            var courseCategoryStats = enrollments?
                .Where(e => e.Course != null && e.Course.CourseCategory != null)
                .GroupBy(e => e.Course.CourseCategory.Name)
                .Select(g => new CourseCategoryStatViewModel
                {
                    CategoryName = g.Key,
                    TotalCourses = g.Select(e => e.CourseId).Distinct().Count(),
                    TotalStudents = g.Select(e => e.StudentId).Distinct().Count()
                })
                .OrderByDescending(g => g.TotalStudents)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalCourses = courses?.Count() ?? 0,
                TotalMaterials = materials?.Count() ?? 0,
                TotalStudents = students?.Count() ?? 0,
                CourseCategoryStats = courseCategoryStats ?? new List<CourseCategoryStatViewModel>()
            };

            return PartialView("_UserDashboardPartial", viewModel);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var userId = GetUserId(); // Assuming BaseController has this helper
            var userName = _userManager.GetAsync(predicate: u => u.Id == userId)
                .Result
                .FirstOrDefault()?
                .Name ?? "Student";

            // Fetch course-related data for this student
            var enrollments = await _enrollmentManager.GetAsync(
                includes: new[] { "Course", "Course.CourseCategory" },
                predicate: e => e.IsActive && e.StudentId == userId
            );

            var totalCourses = enrollments?.Select(e => e.CourseId).Distinct().Count() ?? 0;
            var totalExams = 0; // Replace with real count later if you have exams table
            var totalResults = 0; // Replace with real data later
            var passingRate = 0; // Replace with actual logic

            var todayExams = new List<string>(); // Add logic to fetch today’s exams
            var todayVirtualClasses = new List<string>(); // Add logic to fetch today’s virtual sessions

            var model = new StudentDashboardViewModel
            {
                StudentName = userName,
                TotalCourses = totalCourses,
                TotalExams = totalExams,
                TotalResults = totalResults,
                PassingRate = passingRate,
                TodayExams = todayExams,
                TodayVirtualClasses = todayVirtualClasses
            };

            return View(model);
        }


        public async Task<IActionResult> GetAllMasters()
        {
            //var userPermissions = HttpContext?.Session.GetObjectFromJson<UserPermissions>("UserPermissions");
            //List<FormViewModel> list = new();
            //if (userPermissions != null && userPermissions.Forms != null && userPermissions.Forms.Any())
            //{
            //    foreach (FormViewModel form in userPermissions.Forms)
            //    {
            //        if (form.View)
            //        {
            //            switch ((FormDefination)form.Id)
            //            {
                        
            //                case FormDefination.CourseCategoryMaster:
            //                    {
            //                        form.RecordCountActive = await _courseCategoryManager.CountAsync(t => t.IsActive);
            //                        form.RecordCountInactive = await _courseCategoryManager.CountAsync(t => !t.IsActive);
            //                        list.Add(form);
            //                        break;
            //                    }
            //                case FormDefination.CourseMaster:
            //                    {
            //                        form.RecordCountActive = await _courseManager.CountAsync(t => t.IsActive);
            //                        form.RecordCountInactive = await _courseManager.CountAsync(t => !t.IsActive);
            //                        list.Add(form);
            //                        break;
            //                    }
                        
                        


            //                //case FormDefination.RankingMasternew:
            //                //    {
            //                //        form.RecordCountActive = await _rankingManager.CountAsync(t => t.IsActive);
            //                //        form.RecordCountInactive = await _rankingManager.CountAsync(t => !t.IsActive);
            //                //        list.Add(form);
            //                //        break;
            //                //    }

            //                default: break;
            //            }

            //            // list.Add(form);
            //        }
            //    }
            //}
            return PartialView("_MasterListPartial", null);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadFile(string p, string f, string i)
        {
            byte[] file;
            if (!string.IsNullOrWhiteSpace(i) && i.ToUpper() == "Y")
                file = AzureOperations.GetFileAzureProduct(p, _storageSettings);
            else
                file = AzureOperations.GetFileAzure(p, _storageSettings);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(f, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(file, contentType, f);
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult STF(string p, string f, string i)
        {
            string filePath = p.Decrypt();
            string fileName = f.Decrypt();
            string fileType = i.Decrypt();//Product container or not values Y/N
            if (string.IsNullOrWhiteSpace(filePath))
                return NotFound();
            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            byte[] file;
            if (!string.IsNullOrWhiteSpace(fileType) && fileType.ToUpper() == "Y")
                file = AzureOperations.GetFileAzureProduct(filePath, _storageSettings);
            else
                file = AzureOperations.GetFileAzure(filePath, _storageSettings);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(file, contentType, fileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult UnauthorizedAccess()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ItemNotFound()
        {
            return View();
        }
    }
}