using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await SeedSampleQuizDataAsync(context);
    }

    public static async Task SeedSampleQuizDataAsync(ApplicationDbContext context)
    {
        // Seed a demonstration course once. Never overwrite any group member's records.
        if (await context.Courses.AnyAsync()) return;

        var category = new Category
        {
            Name = "AI Foundations",
            Description = "Start with the key ideas behind artificial intelligence."
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var course = new Course
        {
            Title = "Getting Started with AI",
            Description = "Learn what AI can do, how to write a useful prompt, and how to check the answers you get. Built for first-time learners.",
            LearningOutcomes = "Explain AI in simple terms; write a clear prompt; check an AI answer for mistakes and bias.",
            Level = CourseLevel.Beginner,
            Price = 0m,
            Status = CourseStatus.Published,
            CategoryId = category.Id
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        context.Lessons.AddRange(
            new Lesson
            {
                CourseId = course.Id,
                Title = "What is artificial intelligence?",
                Content = "AI is software that finds patterns in data and uses them to make predictions or generate responses. You may see it in search, translation and chat tools. AI can make mistakes, so check important facts before using its output."
            },
            new Lesson
            {
                CourseId = course.Id,
                Title = "Write a better prompt",
                Content = "Tell the tool what task you want, who the answer is for, and what format you need. For example: Explain machine learning to a beginner in five short points. Review the answer and ask a follow-up question if it is unclear."
            },
            new Lesson
            {
                CourseId = course.Id,
                Title = "Check the result",
                Content = "AI responses can sound sure even when they are wrong. Compare key claims with a reliable source. Avoid pasting private information into a tool you do not trust. Think about bias and use your own judgement."
            });

        var quiz = new Quiz
        {
            CourseId = course.Id,
            Title = "AI Foundations Check",
            TotalMarks = 3
        };
        context.Quizzes.Add(quiz);
        context.Assignments.Add(new Assignment
        {
            CourseId = course.Id,
            Title = "Check an AI response",
            Description = "Ask an AI tool to explain a simple topic. Check two claims against reliable sources. Submit a short PDF that shows your prompt, the claims and what you found.",
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = AssignmentStatus.Published
        });
        await context.SaveChangesAsync();

        context.Questions.AddRange(
            new Question
            {
                QuizId = quiz.Id,
                QuestionText = "What should you do with an important fact in an AI answer?",
                OptionA = "Trust it without checking",
                OptionB = "Check it against a reliable source",
                OptionC = "Share it at once",
                OptionD = "Remove the source",
                CorrectAnswer = "OptionB"
            },
            new Question
            {
                QuizId = quiz.Id,
                QuestionText = "Which prompt gives the clearest task?",
                OptionA = "AI",
                OptionB = "Help",
                OptionC = "Explain machine learning to a beginner in five short points",
                OptionD = "Write something",
                CorrectAnswer = "OptionC"
            },
            new Question
            {
                QuizId = quiz.Id,
                QuestionText = "What information should you avoid sending to an untrusted AI tool?",
                OptionA = "Private account details",
                OptionB = "A general topic",
                OptionC = "A public title",
                OptionD = "A harmless example",
                CorrectAnswer = "OptionA"
            });
        await context.SaveChangesAsync();
    }
}
