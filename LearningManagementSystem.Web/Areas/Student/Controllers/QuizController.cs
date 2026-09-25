using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Student.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly ICertificateEligibilityService _certificateEligibilityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizController(
            IQuizRepository quizRepository,
            IAssignmentRepository assignmentRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ICertificateEligibilityService certificateEligibilityService,
            UserManager<ApplicationUser> userManager)
        {
            _quizRepository = quizRepository;
            _assignmentRepository = assignmentRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _certificateEligibilityService = certificateEligibilityService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var enrolledCourseIds = await _assignmentRepository.GetStudentEnrolledCourseIdsAsync(user.Id, cancellationToken);
            var quizzes = await _quizRepository.GetQuizzesByCourseIdsAsync(enrolledCourseIds, cancellationToken);

            var quizList = quizzes.ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                quizList = quizList.Where(q =>
                    q.Title.ToLower().Contains(term) ||
                    (q.Course != null && q.Course.Title.ToLower().Contains(term))
                ).ToList();
            }

            var viewModels = quizList.Select(q => new StudentQuizItemViewModel
            {
                QuizId = q.Id,
                Title = q.Title,
                CourseTitle = q.Course?.Title ?? "General Course",
                CourseId = q.CourseId,
                TotalMarks = q.TotalMarks,
                QuestionCount = q.Questions?.Count ?? 0,
                EstimatedDurationMinutes = Math.Max(10, (q.Questions?.Count ?? 0) * 3)
            }).ToList();

            ViewBag.Search = search ?? string.Empty;

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            bool isEnrolled = await _quizRepository.IsStudentEnrolledInQuizCourseAsync(user.Id, id, cancellationToken);
            if (!isEnrolled)
            {
                TempData["Error"] = "Quiz not found or you are not enrolled in the associated course.";
                return RedirectToAction(nameof(Index));
            }

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id, cancellationToken);
            if (quiz == null)
            {
                TempData["Error"] = "Quiz details could not be loaded.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new StudentQuizDetailsViewModel
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                CourseTitle = quiz.Course?.Title ?? "General Course",
                CourseId = quiz.CourseId,
                TotalMarks = quiz.TotalMarks,
                QuestionCount = quiz.Questions.Count,
                EstimatedDurationMinutes = Math.Max(10, quiz.Questions.Count * 3),
                PassingMarks = (int)Math.Ceiling(quiz.TotalMarks * 0.5)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Take(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            bool isEnrolled = await _quizRepository.IsStudentEnrolledInQuizCourseAsync(user.Id, id, cancellationToken);
            if (!isEnrolled)
            {
                TempData["Error"] = "You are not authorized to take this quiz.";
                return RedirectToAction(nameof(Index));
            }

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id, cancellationToken);
            if (quiz == null || !quiz.Questions.Any())
            {
                TempData["Error"] = "This quiz has no active questions to answer.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new TakeQuizViewModel
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                CourseTitle = quiz.Course?.Title ?? "General Course",
                TotalMarks = quiz.TotalMarks,
                DurationMinutes = Math.Max(10, quiz.Questions.Count * 3),
                Questions = quiz.Questions.Select(q => new QuizQuestionItemViewModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, IFormCollection form, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            bool isEnrolled = await _quizRepository.IsStudentEnrolledInQuizCourseAsync(user.Id, id, cancellationToken);
            if (!isEnrolled)
            {
                TempData["Error"] = "Unauthorized quiz submission attempt.";
                return RedirectToAction(nameof(Index));
            }

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id, cancellationToken);
            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found.";
                return RedirectToAction(nameof(Index));
            }

            int correctCount = 0;
            int incorrectCount = 0;
            int unansweredCount = 0;
            double calculatedMarks = 0;

            double marksPerQuestion = quiz.Questions.Count > 0
                ? (double)quiz.TotalMarks / quiz.Questions.Count
                : 0;

            foreach (var question in quiz.Questions)
            {
                string? selectedOption = form[$"question_{question.Id}"].ToString();

                if (string.IsNullOrWhiteSpace(selectedOption))
                {
                    unansweredCount++;
                }
                else if (IsCorrectAnswer(question, selectedOption))
                {
                    correctCount++;
                    calculatedMarks += marksPerQuestion;
                }
                else
                {
                    incorrectCount++;
                }
            }

            int finalScore = (int)Math.Round(calculatedMarks);
            double percentage = quiz.TotalMarks > 0 ? Math.Round(((double)finalScore / quiz.TotalMarks) * 100, 1) : 0;
            bool isPassed = percentage >= 50.0;

            var answers = quiz.Questions.ToDictionary(
                q => q.Id.ToString(),
                q => form[$"question_{q.Id}"].ToString());

            var attempt = new QuizAttempt
            {
                StudentId = user.Id,
                QuizId = quiz.Id,
                Score = finalScore,
                TotalMarks = quiz.TotalMarks,
                Percentage = percentage,
                IsPassed = isPassed,
                CorrectCount = correctCount,
                IncorrectCount = incorrectCount,
                UnansweredCount = unansweredCount,
                TotalQuestions = quiz.Questions.Count,
                AttemptedAt = DateTime.UtcNow,
                AnswersJson = JsonSerializer.Serialize(answers),
                CreatedAt = DateTime.UtcNow
            };

            // Database write is the source of truth for the result — no Session storage.
            await _quizAttemptRepository.AddAsync(attempt, cancellationToken);
            await _quizAttemptRepository.SaveChangesAsync(cancellationToken);

            await _certificateEligibilityService.CheckAndIssueAsync(user.Id, quiz.CourseId, cancellationToken);

            return RedirectToAction(nameof(Result), new { id = attempt.Id });
        }

        public async Task<IActionResult> Result(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // id identifies a persisted QuizAttempt, ownership-checked against the current student.
            var attempt = await _quizAttemptRepository.GetByIdAsync(id, cancellationToken);
            if (attempt == null || attempt.StudentId != user.Id)
            {
                TempData["Error"] = "Quiz result not found.";
                return RedirectToAction(nameof(Index));
            }

            var studentName = !string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName)
                ? $"{user.FirstName} {user.LastName}".Trim()
                : (user.UserName ?? "Student");

            var resultModel = new QuizResultViewModel
            {
                QuizId = attempt.QuizId,
                QuizTitle = attempt.Quiz?.Title ?? "Quiz",
                CourseTitle = attempt.Quiz?.Course?.Title ?? "General Course",
                StudentName = studentName,
                Score = attempt.Score,
                TotalMarks = attempt.TotalMarks,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed,
                CorrectCount = attempt.CorrectCount,
                IncorrectCount = attempt.IncorrectCount,
                UnansweredCount = attempt.UnansweredCount,
                TotalQuestions = attempt.TotalQuestions,
                SubmittedAt = attempt.AttemptedAt
            };

            return View(resultModel);
        }

        public async Task<IActionResult> History(CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var attempts = await _quizAttemptRepository.GetStudentAttemptsAsync(user.Id, cancellationToken);

            var viewModels = attempts.Select(a => new QuizHistoryItemViewModel
            {
                AttemptId = a.Id,
                QuizId = a.QuizId,
                QuizTitle = a.Quiz?.Title ?? "Quiz",
                CourseTitle = a.Quiz?.Course?.Title ?? "General Course",
                Score = a.Score,
                TotalMarks = a.TotalMarks,
                Percentage = a.Percentage,
                IsPassed = a.IsPassed,
                AttemptedAt = a.AttemptedAt
            }).ToList();

            return View(viewModels);
        }

        private static bool IsCorrectAnswer(Question q, string? studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer)) return false;

            var ans = studentAnswer.Trim();
            var correct = (q.CorrectAnswer ?? "").Trim();

            if (string.Equals(ans, correct, StringComparison.OrdinalIgnoreCase)) return true;

            // Handle choice identifiers (A, B, C, D) or Option names (OptionA, OptionB...)
            if ((correct.Equals("OptionA", StringComparison.OrdinalIgnoreCase) || correct.Equals("A", StringComparison.OrdinalIgnoreCase)) &&
                (ans.Equals("OptionA", StringComparison.OrdinalIgnoreCase) || ans.Equals("A", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionA, StringComparison.OrdinalIgnoreCase))) return true;

            if ((correct.Equals("OptionB", StringComparison.OrdinalIgnoreCase) || correct.Equals("B", StringComparison.OrdinalIgnoreCase)) &&
                (ans.Equals("OptionB", StringComparison.OrdinalIgnoreCase) || ans.Equals("B", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionB, StringComparison.OrdinalIgnoreCase))) return true;

            if ((correct.Equals("OptionC", StringComparison.OrdinalIgnoreCase) || correct.Equals("C", StringComparison.OrdinalIgnoreCase)) &&
                (ans.Equals("OptionC", StringComparison.OrdinalIgnoreCase) || ans.Equals("C", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionC, StringComparison.OrdinalIgnoreCase))) return true;

            if ((correct.Equals("OptionD", StringComparison.OrdinalIgnoreCase) || correct.Equals("D", StringComparison.OrdinalIgnoreCase)) &&
                (ans.Equals("OptionD", StringComparison.OrdinalIgnoreCase) || ans.Equals("D", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionD, StringComparison.OrdinalIgnoreCase))) return true;

            // Direct text match against option text
            if (string.Equals(correct, q.OptionA, StringComparison.OrdinalIgnoreCase) && (ans.Equals("OptionA", StringComparison.OrdinalIgnoreCase) || ans.Equals("A", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionA, StringComparison.OrdinalIgnoreCase))) return true;
            if (string.Equals(correct, q.OptionB, StringComparison.OrdinalIgnoreCase) && (ans.Equals("OptionB", StringComparison.OrdinalIgnoreCase) || ans.Equals("B", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionB, StringComparison.OrdinalIgnoreCase))) return true;
            if (string.Equals(correct, q.OptionC, StringComparison.OrdinalIgnoreCase) && (ans.Equals("OptionC", StringComparison.OrdinalIgnoreCase) || ans.Equals("C", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionC, StringComparison.OrdinalIgnoreCase))) return true;
            if (string.Equals(correct, q.OptionD, StringComparison.OrdinalIgnoreCase) && (ans.Equals("OptionD", StringComparison.OrdinalIgnoreCase) || ans.Equals("D", StringComparison.OrdinalIgnoreCase) || ans.Equals(q.OptionD, StringComparison.OrdinalIgnoreCase))) return true;

            return false;
        }
    }
}
