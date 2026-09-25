using LearningManagementSystem.Application.DTOs.Questions;
using LearningManagementSystem.Application.DTOs.Quizzes;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class QuizzesController : Controller
{
    private readonly IQuizService _quizService;
    private readonly IQuestionService _questionService;
    private readonly ICourseService _courseService;
    private readonly IQuizRepository _quizRepository;
    private const int PageSize = 10;

    public QuizzesController(
        IQuizService quizService,
        IQuestionService questionService,
        ICourseService courseService,
        IQuizRepository quizRepository)
    {
        _quizService = quizService;
        _questionService = questionService;
        _courseService = courseService;
        _quizRepository = quizRepository;
    }

    // ─── Quiz CRUD ───────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(
        string? search,
        int? courseId,
        string? sortBy,
        bool sortDescending = false,
        int page = 1)
    {
        var result = await _quizService.GetAllAsync(search, courseId, sortBy, sortDescending, page, PageSize);

        var courses = await _courseService.GetLookupAsync();
        ViewBag.CourseList = courses;
        ViewBag.Search = search;
        ViewBag.CourseId = courseId;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;

        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        // Use GetQuizWithQuestionsAsync so the Questions collection is populated
        var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id, cancellationToken);
        if (quiz is null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCoursesAsync();
        return View(new CreateQuizDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuizDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }

        try
        {
            await _quizService.CreateAsync(dto);
            TempData["Success"] = "Quiz created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _quizService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateCoursesAsync(item.CourseId);
        return View(new UpdateQuizDto
        {
            Id = item.Id,
            Title = item.Title,
            TotalMarks = item.TotalMarks,
            CourseId = item.CourseId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateQuizDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }

        try
        {
            await _quizService.UpdateAsync(dto);
            TempData["Success"] = "Quiz updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id, cancellationToken);
        if (quiz is null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _quizService.DeleteAsync(id);
            TempData["Success"] = "Quiz deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    // ─── Question Management ──────────────────────────────────────────────────

    public IActionResult AddQuestion(int quizId)
    {
        return View(new CreateQuestionDto { QuizId = quizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(CreateQuestionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _questionService.CreateAsync(dto);
            TempData["Success"] = "Question added successfully.";
            return RedirectToAction(nameof(Details), new { id = dto.QuizId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> EditQuestion(int id)
    {
        var item = await _questionService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(new UpdateQuestionDto
        {
            Id = item.Id,
            QuestionText = item.QuestionText,
            OptionA = item.OptionA,
            OptionB = item.OptionB,
            OptionC = item.OptionC,
            OptionD = item.OptionD,
            CorrectAnswer = item.CorrectAnswer,
            QuizId = item.QuizId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int id, UpdateQuestionDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _questionService.UpdateAsync(dto);
            TempData["Success"] = "Question updated successfully.";
            return RedirectToAction(nameof(Details), new { id = dto.QuizId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var item = await _questionService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost, ActionName("DeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionConfirmed(int id, int quizId)
    {
        try
        {
            await _questionService.DeleteAsync(id);
            TempData["Success"] = "Question deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = quizId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task PopulateCoursesAsync(int? selectedId = null)
    {
        var courses = await _courseService.GetLookupAsync();
        ViewBag.CourseId = courses.ToSelectList(selectedId);
    }
}
