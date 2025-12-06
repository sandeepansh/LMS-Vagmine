using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Managers;
using TMS.ViewModels.Academics;
using TMS.ViewModels.Masters;

namespace TMS.Web.Controllers
{
    public class StudentQuizController : BaseController
    {
        private readonly IMasterBaseManager<QuizMasterViewModel> _quizManager;
        private readonly IMasterBaseManager<StudentQuizAttemptViewModel> _attemptManager;

        public StudentQuizController(
            IMasterBaseManager<QuizMasterViewModel> quizManager,
            IMasterBaseManager<StudentQuizAttemptViewModel> attemptManager)
        {
            _quizManager = quizManager;
            _attemptManager = attemptManager;
        }

        [HttpGet]
        public async Task<IActionResult> Attempt(int courseId, int quadrantId)
        {
            // ✅ Fetch the quiz for given Course and Quadrant
            var quizList = await _quizManager.GetAsync(
                includes: new[] { "Questions", "Questions.Options", "Course", "CourseQuadrant" },
                predicate: q => q.CourseId == courseId && q.CourseQuadrantId == quadrantId
            );

            var quiz = quizList?.FirstOrDefault();
            if (quiz == null)
                return NotFound("No quiz found for the specified course and unit.");
            var attempts = await _attemptManager.GetAsync(null, x => x.IsActive);
            var existingAttempt = attempts
         .FirstOrDefault(x => x.StudentId == GetUserId() && x.QuizId == quiz.Id);

            if (existingAttempt != null)
            {
                SetApplicationResult(false, "Quiz already attempted.");
                return RedirectToAction("StudentDashboard", "Home");
            }
            // ✅ Map to ViewModel (optional, if your view expects it)
            var vm = new StudentQuizAttemptViewModel
            {
                StudentId = GetUserId(),
                QuizId = quiz.Id,
                CourseId = quiz.CourseId,
                CourseQuadrantId = quiz.CourseQuadrantId,
                Answers = quiz.Questions.Select(q => new StudentQuizAnswerViewModel
                {
                    QuizQuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType=q.QuestionType.ToString(),
                    Options = q.Options.Select(o => new QuizOptionViewModel
                    {
                        Id = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(StudentQuizAttemptViewModel attemptVm)
        {
            int studentId = GetUserId();

            // Construct attempt
            //var newAttempt = new StudentQuizAttemptViewModel
            //{
            //    StudentId = studentId,
            //    QuizId = attemptVm.QuizId,
            //    AttemptDate = DateTime.UtcNow,
            //    IsSubmitted = true,
            //    Answers = attemptVm.Answers
            //};




            await _attemptManager.AddUpdateStudentQuizAttemptAsync(attemptVm, GetUserId());

            SetApplicationResult(true, "Quiz submitted successfully.");
            return RedirectToAction("StudentDashboard", "Home");
        }

        [HttpGet]
        public IActionResult Result()
        {
            ViewBag.Score = TempData["Score"];
            return View();
        }
    }
}
