# AI Skills Academy

AI Skills Academy is a course website for learning AI basics. Learners can register, join a course, read lessons, take quizzes, submit projects and see their progress. Administrators can manage courses and review learner work.

This copy uses **ASP.NET Core 8**. It includes a default **SQLite database that is created on first run**. You do not need SQL Server Express or a separate database download for the default setup.

## Start on a Windows laptop

1. Unzip the whole `AI_Skills_Academy_Portable.zip` folder. Keep all project folders together.
2. Install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) if it is not installed.
3. Double-click **`RUN_WINDOWS.cmd`**. The first run restores packages, creates the database and adds the sample AI course.
4. Open **http://localhost:5246** in your browser. Leave the command window open while using the site.

You can also use Visual Studio with the ASP.NET workload:

1. Open **`LearningManagementSystem.sln`** from this newly unzipped folder.
2. In Solution Explorer, expand **src**, right-click **`LearningManagementSystem.Web`**, and select **Set as Startup Project**. The green Run button must show **LearningManagementSystem.Web** (or its **http/https** launch profile), not `LearningManagementSystem.Domain`.
3. Select **http** and press **F5**. Open **http://localhost:5246**. If you select **https**, the address is **https://localhost:7276** and Visual Studio might prompt you to trust the local development certificate.

If Visual Studio says “A project with an Output Type of Class Library cannot be started directly,” repeat step 2. This happens when the Domain library is selected as the startup project. If a browser tab still shows an older purple header with no academy name, close the old running app and reopen this extracted solution before pressing F5; then refresh the page.

## Start on macOS or Linux

Install the .NET 8 SDK, unzip the project, then run these commands in a terminal inside the project folder:

```bash
chmod +x RUN_MAC_LINUX.sh
./RUN_MAC_LINUX.sh
```

Open **http://localhost:5246**. A first run needs an internet connection to restore NuGet packages.

## Demo sign-in

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@lms.com` | `Admin@123` |
| Learner | `student@lms.com` | `Student@123` |

These are classroom demo accounts. Each laptop gets its own database and accounts. Learners can also register new accounts on the site.

## Where the data goes

The app creates `academy.db` in the current user's application data directory, normally `%LOCALAPPDATA%\AISkillsAcademy\academy.db` on Windows or `~/.local/share/AISkillsAcademy/academy.db` on Linux. It keeps the file when you move or unzip a new copy of the source. Uploads are kept by the running app under `LearningManagementSystem.Web/wwwroot/uploads/`.

The default setup **does not sync data between laptops**. If the whole group must see the same accounts and courses, one group member needs to host a reachable SQL Server database. Set `Database__Provider=SqlServer` and `ConnectionStrings__SqlServer` to that server's connection string before starting the app. Existing SQL Server migrations are included. Do not put a shared database password into `appsettings.json` or the ZIP.

## Check the project

From a terminal in this folder:

```bash
dotnet build LearningManagementSystem.sln
dotnet test LearningManagementSystem.sln
dotnet list LearningManagementSystem.sln package --vulnerable --include-transitive
```

The test suite includes a SQLite test that creates a fresh database file, seeds an AI course, reopens it and checks that the data remains.
Visual Studio's package warning is separate from the startup-project error. The project pins a patched native SQLite library for a known transitive SQLite warning; the audit command lists any other warnings that still apply after restoring packages on your laptop.

## Notes for the group

- The included course is a short example. Add more AI lessons, quizzes and projects from the Admin area for the assignment demo.
- The old progress reports describe earlier versions of the project. Use this README for the current setup.
- The proposal predates the teacher-approved change to ASP.NET Core. This package keeps the approved language and framework.
- A `.sln` file by itself cannot run the project. All source folders in this ZIP are needed.
