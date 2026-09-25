using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Questions;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;
    private readonly IQuizRepository _quizRepository;
    private readonly IMapper _mapper;

    public QuestionService(IQuestionRepository repository, IQuizRepository quizRepository, IMapper mapper)
    {
        _repository = repository;
        _quizRepository = quizRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<QuestionDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<QuestionDto>
        {
            Items = _mapper.Map<IReadOnlyList<QuestionDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<QuestionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _repository.GetByIdAsync(id, cancellationToken);
        return question is null ? null : _mapper.Map<QuestionDto>(question);
    }

    public async Task CreateAsync(CreateQuestionDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _quizRepository.ExistsAsync(dto.QuizId, cancellationToken))
        {
            throw new InvalidOperationException("The selected quiz does not exist.");
        }

        ValidateCorrectAnswer(dto.CorrectAnswer);

        var question = _mapper.Map<Question>(dto);
        question.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(question, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateQuestionDto dto, CancellationToken cancellationToken = default)
    {
        var question = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Question with ID {dto.Id} was not found.");

        if (!await _quizRepository.ExistsAsync(dto.QuizId, cancellationToken))
        {
            throw new InvalidOperationException("The selected quiz does not exist.");
        }

        ValidateCorrectAnswer(dto.CorrectAnswer);

        _mapper.Map(dto, question);
        question.UpdatedAt = DateTime.UtcNow;

        _repository.Update(question);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Question with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateCorrectAnswer(string correctAnswer)
    {
        var normalized = correctAnswer.Trim().ToUpperInvariant();
        if (normalized is not ("A" or "B" or "C" or "D"))
        {
            throw new InvalidOperationException("Correct answer must be A, B, C, or D.");
        }
    }
}
