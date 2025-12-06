using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMS.Repository.Managers;
using TMS.ViewModels;
using TMS.ViewModels.Academics;
using TMS.ViewModels.Masters;

namespace TMS.Web.Controllers
{
    public class StudentCourseContentController : BaseController
    {
        private readonly IMasterManager<CourseMasterViewModel> _courseManager;
        private readonly IMasterBaseManager<CourseEnrollmentViewModel> _enrollmentManager;
        private readonly IMasterBaseManager<LectureMaterialViewModel> _materialManager;
        private readonly IMasterBaseManager<CourseQuadrantViewModel> _quadrantManager;
        private readonly IMasterBaseManager<StudentQuizAttemptViewModel> _attemptManager;
        public StudentCourseContentController(
            IMasterManager<CourseMasterViewModel> courseManager,
            IMasterBaseManager<CourseEnrollmentViewModel> enrollmentManager,
            IMasterBaseManager<LectureMaterialViewModel> materialManager,
            IMasterBaseManager<CourseQuadrantViewModel> quadrantManager,
            IMasterBaseManager<StudentQuizAttemptViewModel> attemptManager)
        {
            _courseManager = courseManager;
            _enrollmentManager = enrollmentManager;
            _materialManager = materialManager;
            _quadrantManager = quadrantManager;
            _attemptManager = attemptManager;
        }

        // =============================
        // GET: Index - Show enrolled courses & materials
        // =============================
        public async Task<IActionResult> Index()
        {
            int studentId = GetUserId();

            // Fetch enrolled courses with category info
            var enrollments = await _enrollmentManager.GetAsync(
                includes: new[] { "Course", "Course.CourseCategory", "Course.Semester" },
                predicate: e => e.StudentId == studentId && e.IsActive
            );

            var enrolledCourses = enrollments?
                .Select(e => e.Course)
                .Where(c => c != null)
                .ToList() ?? new List<CourseMasterViewModel>();

            if (!enrolledCourses.Any())
            {
                ViewBag.Message = "You are not enrolled in any courses.";
                return View(new StudentCourseCategoryViewModel());
            }

            // Pick first category as “main” (e.g., MBA)
            var firstCategory = enrolledCourses.First().CourseCategory;
            var quadrants = await _quadrantManager.GetAsync(null, q => q.IsActive);

            var viewModel = new StudentCourseCategoryViewModel
            {
                CategoryName = firstCategory?.Name ?? "No Category",
                CategoryDescription = firstCategory?.Description ?? "No description available.",
                Semesters = enrolledCourses
                    .GroupBy(c => c.Semester.Id)
                    .OrderBy(g => g.Key)
                    .Select(g => new SemesterCourseGroupViewModel
                    {
                        Semester = g.Key,
                        Courses = g.Select(c => new CourseSummaryViewModel
                        {
                            CourseId = c.Id,
                            CourseCode = c.CourseCode ?? "N/A",
                            CourseName = c.Name ?? "Untitled Course",
                            //CourseCredit = c.Credit,
                            CategoryId = c.CourseCategoryId
                        }).ToList()
                    }).ToList(),
                Quadrants = quadrants
                    .OrderBy(q => q.QuadrantNumber)
                    .Select(q => new CourseQuadrantViewModel
                    {
                        Id = q.Id,
                        QuadrantNumber = q.QuadrantNumber,
                        Name = q.Name
                    }).ToList()
            };
    
            return View(viewModel);
        }





        //[HttpGet]
        //public async Task<IActionResult> Details(int semesterId, int? courseId = null)
        //{
        //    // ✅ Fetch all active courses for the semester (and optionally for a specific course)
        //    var courses = await _courseManager.GetAsync(
        //        includes: new[] { "CourseCategory", "Semester" },
        //        predicate: c => c.SemesterId == semesterId && c.IsActive &&
        //                        (!courseId.HasValue || c.Id == courseId.Value)
        //    );

        //    if (courses == null || !courses.Any())
        //        return NotFound();

        //    // ✅ Fetch all lecture materials for these courses
        //    var courseIds = courses.Select(c => c.Id).ToList();

        //    // Fetch all materials for the semester, then filter locally
        //    var allMaterials = await _materialManager.GetAsync(
        //        includes: new[] { "CourseQuadrant" },
        //        predicate: m => m.IsActive
        //    );

        //    // Filter locally for matching course IDs
        //    var materials = allMaterials.Where(m => courseIds.Contains(m.CourseId)).ToList();


        //    // ✅ Group materials by course, then by unit (quadrant)
        //    var groupedCourses = courses.Select(c => new CourseMaterialGroupedViewModel
        //    {
        //        Course = c,
        //        GroupedMaterials = materials
        //            .Where(m => m.CourseId == c.Id)
        //            .GroupBy(m => m.CourseQuadrant?.Name ?? "Unassigned")
        //            .ToDictionary(g => g.Key, g => g.ToList())
        //    }).ToList();

        //    return View(groupedCourses);
        //}
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Details(int semesterId, int? courseId = null)
        {
            int studentId = GetUserId(); // ✅ Logged-in user ID

            // 1️⃣ Fetch all active courses for the semester (optionally filter by courseId)
            var courses = await _courseManager.GetAsync(
                includes: new[] { "CourseCategory", "Semester" },
                predicate: c => c.SemesterId == semesterId && c.IsActive &&
                                (!courseId.HasValue || c.Id == courseId.Value)
            );

            if (courses == null || !courses.Any())
                return NotFound();

            var courseIds = courses.Select(c => c.Id).ToList();

            // 2️⃣ Fetch all lecture materials for these courses
            var allMaterials = await _materialManager.GetAsync(
                includes: new[] { "CourseQuadrant" },
                predicate: m => m.IsActive
            );
            var materials = allMaterials.Where(m => courseIds.Contains(m.CourseId)).ToList();

            // 3️⃣ Fetch all attempts for logged-in student for these courses and quadrants
            var Attempts = await _attemptManager.GetAsync(null,t=>t.IsActive);
           
            var allAttempts = Attempts
                .Where(a => a.StudentId == studentId && courseIds.Contains(a.CourseId))
                .ToList();

            // 4️⃣ Group materials by course, then by quadrant, and check if quiz attempted
            var groupedCourses = courses.Select(c => new CourseMaterialGroupedViewModel
            {
                Course = c,
                GroupedMaterials = materials
                    .Where(m => m.CourseId == c.Id)
                    .GroupBy(m => new
                    {
                        QuadrantId = m.CourseQuadrantId,
                        QuadrantName = m.CourseQuadrant != null ? m.CourseQuadrant.Name : "Unassigned"
                    })
                    .Select(g => new QuadrantMaterialGroupViewModel
                    {
                        QuadrantId = g.Key.QuadrantId,
                        QuadrantName = g.Key.QuadrantName,
                        Materials = g.ToList(),
                        HasAttemptedQuiz = allAttempts.Any(a => a.CourseId == c.Id && a.CourseQuadrantId == g.Key.QuadrantId)
                    })
                    .ToList()
            }).ToList();

            return View(groupedCourses);
        }




    }




}
