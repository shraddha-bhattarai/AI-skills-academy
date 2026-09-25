# AI Skills Academy current changes

This source package is based on the group's August project archive. The proposal is included unchanged in `Proposal_Reference.docx`; the teacher approved the switch to ASP.NET Core.

## Setup

- SQLite is now the default database. The app creates its file and schema on first run and seeds an introductory AI course, roles and demo accounts.
- SQL Server remains an option when configured through environment variables. Its original migrations are preserved.
- The Windows and Mac/Linux launchers start in the web project folder, so static assets and uploads use the right paths.

## Site

- The home page has clear AI learning content, a CSS illustration, real course data and working routes.
- The site name is AI Skills Academy across public pages, learner and admin layouts, and certificates.
- The About page explains the project as proposed. Fake testimonials, teacher profiles, contact numbers, office details and newsletter signup were removed.
- The category detail page links to a filtered course list. Public category editing was removed.
- Student course cards no longer show an invented instructor. Courses without images use a built-in graphic.

## Review and limits

- Source files, JSON, XML and JavaScript received static checks in this environment. The .NET SDK is not available here, so the updated solution could not be compiled or started here.
- Run `dotnet build LearningManagementSystem.sln` and `dotnet test LearningManagementSystem.sln` on a laptop with the .NET 8 SDK. The SQLite file test checks database creation and data after reopening.
- Each default database is private to its laptop. Shared records require a hosted SQL Server and a connection string.
- The August reports in `history/` record earlier tests of the earlier source. They do not prove the changes in this package have passed a new build.
