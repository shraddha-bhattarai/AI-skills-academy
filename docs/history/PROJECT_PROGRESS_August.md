# Project Progress

This file tracks work across sessions on the AI Skills Academy LMS. **Read this file first before starting any new session.**

## Session 1 — 2026-08-18: Authorization & Duplicate-CRUD Cleanup

### Completed Work

Lecturer feedback flagged that nearly every root-level controller had no `[Authorize]` anywhere, so anonymous users could create/edit/delete courses, grade submissions, mint certificates, delete enrollments, etc. This session closed that gap by removing duplicate unauthenticated CRUD surfaces (the Admin area already had working, protected equivalents) and locking down the rest.

1. **`CoursesController` / `CategoriesController` (root)** — confirmed via view-reference audit that `Index`/`Details` are the genuine public course/category catalog (linked from `Home/Index`, `_Navbar`, `_CourseCard`, `_Footer`, `_StudentLayout`). Kept those two actions, now explicitly `[AllowAnonymous]`, and **deleted** `Create`/`Edit`/`Delete`/`DeleteConfirmed` plus the now-dead `PopulateCategoriesAsync` helper and unused `ICategoryService` dependency in `CoursesController`. Admin CRUD lives in `Areas/Admin/Controllers/{Courses,Categories}Controller.cs`.

2. **`QuizzesController`, `QuestionsController`, `EnrollmentsController`, `AssignmentsController`, `NotificationsController` (root)** — **deleted entirely**. Confirmed via grep that nothing legitimate depended on the anonymous root versions; the only references were dangling/mis-pointed links (see below), and the Admin area already fully covers these (Admin `QuizzesController` also handles inline Question CRUD via `AddQuestion`/`EditQuestion`/`DeleteQuestion`, so no separate `QuestionsController` was ever needed there).

3. **`CertificatesController` (root)** — ported `Create` (GET/POST) and `Edit` (GET/POST) into `Areas/Admin/Controllers/CertificatesController.cs`, reusing the existing `ICertificateService`, `ICourseService`, `IUserRepository` (same pattern as root had). Added matching `Areas/Admin/Views/Certificates/Create.cshtml` and `Edit.cshtml` (styled like `Areas/Admin/Views/Enrollments/Create|Edit.cshtml`), plus a "New Certificate" button on Admin `Certificates/Index.cshtml`, an Edit icon in its action column, and an Edit button on `Certificates/Details.cshtml`. Then **deleted** the root `CertificatesController` and its `Views/Certificates/*` entirely.

4. **`LessonsController`, `SubmissionsController`, `AnnouncementsController`, `ContactMessagesController` (root)** — no Admin-area equivalent exists yet (out of scope to build this session). Added `[Authorize(Roles = "Admin")]` at the class level to all four as a stopgap so they're no longer publicly reachable. Confirmed the public "Contact Us" form (`Home/Contact` GET+POST) calls `IContactMessageService` directly and does **not** go through `ContactMessagesController`, so locking that controller down does not affect the public contact form.

5. **Fixed dangling/mis-pointed view links** uncovered while auditing (pre-existing bugs, not something my controller removal caused, but they had to be fixed so nothing broke and so the audit trail is clean):
   - `Views/Shared/_Navbar.cshtml`: the logged-in-Student nav branch linked to the **root** `Assignments`/`Quizzes`/`Certificates`/`Notifications` controllers (being deleted) instead of the real per-student, ownership-filtered `Areas/Student` controllers (`Assignments`, `Quiz`, `Certificate`, `Notification` — note singular naming for the latter three). Repointed all four links to `asp-area="Student"` with the correct controller names.
   - `Areas/Admin/Views/Home/Index.cshtml`: three "quick action" dashboard links (`New Quiz`, `Certificates`, `Notifications` ×2) explicitly used `asp-area=""`, pointing at the root controllers instead of the Admin area. Changed all to `asp-area="Admin"`. (The `Submissions` quick-action link legitimately still points at the root `SubmissionsController`, which now itself requires the Admin role, so it was left as-is.)
   - `Areas/Student/Views/Home/Index.cshtml`: three student-dashboard links (`View Task`, `Start Quiz`, `View Assignments`) omitted `asp-area`, relying on ambient area value; one of them (`Start Quiz`) also used the wrong controller name (`Quizzes` instead of `Quiz`), which would have thrown a routing exception at render time. Made all three explicit (`asp-area="Student"`, correct controller names).

6. **Views cleanup**: removed the "New Course"/"New Category" buttons and Edit/Delete action-icon links from `Views/Courses/Index.cshtml` and `Views/Categories/Index.cshtml` (their target actions no longer exist); kept the Details "eye" link since `Details` remains public. Deleted the now-orphaned `Create.cshtml`/`Edit.cshtml`/`Delete.cshtml` view files for Courses, Categories, and the five fully-removed controllers' entire `Views/*` folders (Quizzes, Questions, Enrollments, Assignments, Notifications, Certificates).

7. Verified `Areas/Student` controllers (`AssignmentsController`, `QuizController`, `CertificateController`, `NotificationController`) were **not modified** — their per-user ownership filtering logic is untouched. Only the *view links pointing at them* were corrected (see above), which was necessary because those links were broken/misdirected either way.

8. Checked `Program.cs` — no service registrations needed to change; no service became unused (all services touched are still consumed by the surviving/Admin/Student controllers).

### Files Changed

**Deleted controllers:** `Controllers/QuizzesController.cs`, `Controllers/QuestionsController.cs`, `Controllers/EnrollmentsController.cs`, `Controllers/AssignmentsController.cs`, `Controllers/NotificationsController.cs`, `Controllers/CertificatesController.cs`

**Deleted view folders:** `Views/Quizzes/*`, `Views/Questions/*`, `Views/Enrollments/*`, `Views/Assignments/*`, `Views/Notifications/*`, `Views/Certificates/*` (root), plus `Views/Courses/{Create,Edit,Delete}.cshtml` and `Views/Categories/{Create,Edit,Delete}.cshtml`

**Modified controllers:** `Controllers/CoursesController.cs` (stripped to Index/Details, `[AllowAnonymous]`), `Controllers/CategoriesController.cs` (same), `Controllers/LessonsController.cs` (`[Authorize(Roles="Admin")]` added), `Controllers/SubmissionsController.cs` (same), `Controllers/AnnouncementsController.cs` (same), `Controllers/ContactMessagesController.cs` (same), `Areas/Admin/Controllers/CertificatesController.cs` (added Create/Edit)

**New views:** `Areas/Admin/Views/Certificates/Create.cshtml`, `Areas/Admin/Views/Certificates/Edit.cshtml`

**Modified views:** `Views/Courses/Index.cshtml`, `Views/Categories/Index.cshtml`, `Areas/Admin/Views/Certificates/Index.cshtml`, `Areas/Admin/Views/Certificates/Details.cshtml`, `Areas/Admin/Views/Home/Index.cshtml`, `Areas/Student/Views/Home/Index.cshtml`, `Views/Shared/_Navbar.cshtml`

**Not touched by this session (pre-existing modifications from earlier work, left as found):** `Controllers/HomeController.cs`, `Views/Home/Index.cshtml`, `Views/Shared/_Footer.cshtml`, `Views/Home/About.cshtml`, `Views/Home/Contact.cshtml` — these were already modified/added before this session started (landing-page styling work); I did not need to change their logic.

### Database Changes
None.

### Migrations
None — no entity/DbContext changes this session.

### Routes Added/Changed
- **Removed entirely:** `/Quizzes/*`, `/Questions/*`, `/Enrollments/*`, `/Assignments/*`, `/Notifications/*`, `/Certificates/*` (root, no area). Use `/Admin/Quizzes/*`, `/Admin/Enrollments/*`, `/Admin/Assignments/*`, `/Admin/Notifications/*`, `/Admin/Certificates/*` instead (Admin only), or `/Student/Quiz/*`, `/Student/Assignments/*`, `/Student/Certificate/*`, `/Student/Notification/*` for the student-facing equivalents.
- **Removed:** `/Courses/Create`, `/Courses/Edit/{id}`, `/Courses/Delete/{id}`, `/Categories/Create`, `/Categories/Edit/{id}`, `/Categories/Delete/{id}` (use `/Admin/Courses/*`, `/Admin/Categories/*`).
- **New:** `/Admin/Certificates/Create`, `/Admin/Certificates/Edit/{id}` (ported from the removed root controller).
- **Unchanged routes, now Admin-only:** `/Lessons/*`, `/Submissions/*`, `/Announcements/*`, `/ContactMessages/*` (all require `Admin` role now; previously fully anonymous).
- **Unchanged, still public:** `/Courses/Index`, `/Courses/Details/{id}`, `/Categories/Index`, `/Categories/Details/{id}`, `/Home/*` (including the `Contact` POST), `/Account/Login`, `/Account/Register`.

### Authorization Changes
- `CoursesController`, `CategoriesController`: no class-level `[Authorize]`; `Index`/`Details` explicitly `[AllowAnonymous]`; all mutating actions removed (not just gated).
- `LessonsController`, `SubmissionsController`, `AnnouncementsController`, `ContactMessagesController`: `[Authorize(Roles = "Admin")]` added at class level (stopgap — see Remaining Work).
- `QuizzesController`, `QuestionsController`, `EnrollmentsController`, `AssignmentsController`, `NotificationsController`, `CertificatesController` (root): controllers deleted, so the question of authorization is moot — traffic must go through the already-protected Admin/Student area controllers.
- `Areas/Admin/Controllers/CertificatesController.cs`: new `Create`/`Edit` actions inherit the controller's existing class-level `[Authorize(Roles = "Admin")]`.
- No changes to `Areas/Student/*` controllers' `[Authorize(Roles = "Student")]` or their ownership-filtering logic.
- No changes to `Program.cs` authentication/cookie/JWT configuration (explicitly out of scope).

### Tests Performed
- `dotnet build` on the full solution — see Build Result below.
- Manually traced: every remaining controller action that mutates data (Create/Edit/Delete/POST) now sits behind either a class-level `[Authorize(Roles="Admin")]` (Areas/Admin/*, root Lessons/Submissions/Announcements/ContactMessages) or a class-level `[Authorize(Roles="Student")]` (Areas/Student/*), except `HomeController.Contact` (POST, intentionally public — the real contact form) and `AccountController` Login/Register (pre-existing, intentionally `[AllowAnonymous]`).
- Grepped the whole Web project for `[HttpPost]` occurrences and confirmed every containing class carries an `[Authorize]` attribute except the two intentional exceptions above.
- Grepped for any remaining `asp-controller` references to the six removed root controllers (Quizzes/Questions/Enrollments/Assignments/Notifications/Certificates) and to `RedirectToAction` calls targeting them — none found.
- No UI/browser smoke test was performed (no dev server run as part of this session, beyond confirming the build); this is authorization/controller-removal work, not a UI change, but a manual click-through as Admin/Student/anonymous is recommended before merging.

### Build Result
**Succeeded**, 0 errors, 2 warnings (both pre-existing: `NU1903` AutoMapper 13.0.1 known vulnerability advisory — unrelated to this session's changes, not addressed).

### Known Issues
- The `Areas/Student/Views/Home/Index.cshtml` "Start Quiz" link previously referenced the non-existent `Student/Quizzes` controller (should have been `Student/Quiz`) and would have thrown a routing error if clicked — this was a pre-existing bug (unrelated to root-controller removal) that was fixed as part of the dangling-link cleanup in this session.
- `Views/Courses/Index.cshtml` and `Views/Categories/Index.cshtml` render **hardcoded static mock data** (`@{ var courses = new[] {...} }`), not the actual `Model` the controller passes in from `_courseService.GetAllAsync(...)`. This predates this session (no `@model` directive at the top of either file) and is a real gap — the public catalog pages don't currently show real data — but wiring them to the real model is a UI/data-binding task, not an authorization task, so it was left untouched per this session's scope.
- `LessonsController`, `SubmissionsController`, `AnnouncementsController`, `ContactMessagesController` are now Admin-only as a stopgap. Nothing currently provides students a way to view lessons or submit assignments through these controllers — that capability doesn't exist yet anywhere in the app (Student area has no Lessons controller at all yet).

### Remaining Work
- Build proper `Areas/Admin` equivalents for Lessons, Submissions, Announcements, ContactMessages (with ownership/ workflow logic as appropriate), then decide whether to remove or keep the now-Admin-only root versions of `LessonsController`/`SubmissionsController` (explicitly flagged in the original task as belonging to a later session).
- Build a `Areas/Student` Lessons-viewing experience (view lesson content for enrolled courses) — currently there is no student-facing lesson view anywhere in the app.
- Wire `Views/Courses/Index.cshtml` / `Views/Categories/Index.cshtml` to their real `Model` instead of hardcoded mock arrays (see Known Issues).
- Student self-enrollment flow does not exist yet — `Areas/Student/Controllers/CoursesController.cs` should be checked for whether it supports self-enroll or is browse-only.

### Notes for Next Session (superseded — see Session 2 below)
~~Session 2 will build learner self-enrollment, lesson viewing, and the real student dashboard.~~ Done — see Session 2.

## Session 2 — 2026-08-19: Learner Core Flow (Enroll, Lessons, Progress, Real Dashboard)

### Completed Work

Lecturer feedback: "the learner cannot yet properly enroll, open lessons, complete lessons, or save quiz history... progress is not truly calculated from lesson completion... student dashboard still contains fake/sample data." This session closed the enrollment/lesson/progress/dashboard part of that gap (quiz history persistence is explicitly out of scope — see Known Issues/Remaining Work).

1. **`LessonProgress` entity** (new) — `Domain/Entities/Courses/LessonProgress.cs`: `StudentId`, `LessonId`, `CompletedAt`, plus `BaseEntity` fields. Wired into `ApplicationDbContext` as `DbSet<LessonProgress> LessonProgresses`, with a `Lesson 1—many LessonProgress` relationship and a **unique filtered index** on `(StudentId, LessonId) WHERE IsDeleted = 0` (mirrors the soft-delete convention already used by the global query filter) so a student can't double-complete the same lesson. Migration `AddLessonProgress` generated via `dotnet ef migrations add AddLessonProgress --project LearningManagementSystem.Persistence --startup-project LearningManagementSystem.Web` (same project/startup-project pairing as `InitialCreate`) and applied to the dev DB with `dotnet ef database update` — verified clean (only adds the `LessonProgresses` table + 2 indexes, no unrelated diffs).

2. **Self-enrollment** — `IEnrollmentService`/`EnrollmentService` gained `IsStudentEnrolledAsync` and `EnrollStudentAsync(studentId, courseId)`; the latter checks the course exists, checks for an existing enrollment (via new `IEnrollmentRepository.GetByStudentAndCourseAsync`) and **no-ops idempotently** ("You are already enrolled...") rather than erroring or duplicating. New `[Authorize(Roles="Student")]` POST `Areas/Student/Controllers/CoursesController.Enroll(int courseId, string? returnUrl)` — never trusts a StudentId from the request, always uses `UserManager.GetUserAsync(User)`. Root `Controllers/CoursesController.cs` (`Details`, still `[AllowAnonymous]`) now injects `IEnrollmentService` + `UserManager` and sets `ViewBag.IsStudent`/`ViewBag.IsEnrolled`. `Views/Courses/Details.cshtml` now shows: an "Enroll in this Course" form (posts to `Student/Courses/Enroll`) for authenticated students not yet enrolled, "Already Enrolled — View Lessons" for enrolled students, or **"Login to Enroll"** linking to `/Account/Login?returnUrl=/Courses/Details/{id}` for guests — reusing `AccountController.Login`'s existing `Url.IsLocalUrl` + `Redirect(returnUrl)` handling rather than the framework's automatic `[Authorize]` challenge redirect (which would target the POST endpoint itself and 404 on the GET replay after login). Also removed a dead `asp-action="Edit"` link on that page left over from Session 1's removal of root `Courses.Edit`.

3. **`Areas/Student/Controllers/CoursesController.Index`** — deleted `GetPlaceholderCourses()` (6 hardcoded fake courses) and its call site entirely. Zero real enrollments now renders the pre-existing genuine empty state in `Views/Student/Courses/Index.cshtml` ("No Enrolled Courses Found" + "Browse Courses" link — that markup already existed, just was unreachable before). Also fixed an adjacent fabrication in the same method: `CompletedLessons`/`TotalLessons` used to fall back to a **hardcoded `10`** whenever a course had 0 real lessons; now both are always real counts (`TotalLessons` = actual `Course.Lessons.Count`, `CompletedLessons` = actual distinct `LessonProgress` rows for that student/course). Removed the fabricated `Rating = 4.9` field population (still present as an unused ViewModel field — not rendered anywhere, confirmed via grep — left as-is since removing the property itself was unrelated cleanup).

4. **Lesson viewing** — `ILessonService`/`LessonService` gained `GetStudentLessonsAsync(studentId, courseId)` (returns `null` if not enrolled, distinguishing "not enrolled" from "enrolled but 0 lessons") and `GetStudentLessonDetailsAsync(lessonId, studentId)` (same ownership check, keyed off the lesson's `CourseId`), following the exact ownership-check pattern already used by `AssignmentService.GetStudentAssignmentDetailsAsync`. New `ILessonRepository` methods: `GetByCourseIdAsync`, `CountByCourseIdAsync`, `IsStudentEnrolledInCourseAsync` (self-contained on the repo, mirroring `AssignmentRepository`'s existing `IsStudentEnrolledInCourseAsync`). New `[Authorize(Roles="Student")]` `Areas/Student/Controllers/LessonsController.cs` (`Index(int courseId)`, `Details(int id)`, `MarkComplete(int id)` POST) — every action re-verifies ownership server-side via the service layer, never just checks login. New views `Areas/Student/Views/Lessons/{Index,Details}.cshtml` in the existing Student-area Bootstrap style (`feature-card`/gradient-hero conventions from `Assignments/Index.cshtml`).
   - **URL sanitization**: new `Application/Common/UrlSafety.cs` (`IsSafeHttpUrl`/`SanitizeOrNull`) validates `VideoUrl`/`NotesUrl` are absolute `http`/`https` URIs; `LessonService` nulls out anything else **before** it ever reaches a `StudentLessonDto`, so the view layer never sees an unsafe value (blocks `javascript:`/`ftp:`/malformed URLs — e.g. a `javascript:alert(1)` URL written directly to the DB was confirmed, via smoke test, to never render as a clickable link). The Details view additionally only `<video>`-embeds URLs ending in a known video extension, otherwise renders a plain `target="_blank" rel="noopener noreferrer"` link.
   - "My Courses" and dashboard "Open Course"/"Continue Learning"/"Start Course" buttons (previously dead links back to `Courses/Index` itself) now route to `Student/Lessons/Index?courseId=`.

5. **`LessonProgress` + real progress recalculation** — new `ILessonProgressRepository`/`LessonProgressRepository` (`GetAsync`, `AddAsync`, `CountCompletedForStudentCourseAsync`, `GetCompletedLessonIdsAsync`, `CountCompletedThisWeekAsync`). New `ILessonProgressService`/`LessonProgressService.MarkLessonCompleteAsync(lessonId, studentId)`: re-verifies enrollment ownership (via `ILessonRepository.IsStudentEnrolledInCourseAsync`), upserts the `LessonProgress` row (idempotent — re-marking an already-completed lesson is a no-op, confirmed via smoke test), then **recalculates `Enrollment.Progress` = (real completed lesson count / real total lesson count) × 100** and persists it. "Mark Complete" button added to the lesson Details view, posting to `LessonsController.MarkComplete`.

6. **Real student dashboard** — `Areas/Student/Controllers/HomeController.cs` fully rewritten off `UserManager` + `ApplicationDbContext` + `IAssignmentService` + `ICertificateService` (same DbContext-direct pattern already established by `Areas/Student/Controllers/CoursesController.cs` for this kind of cross-entity aggregate read). `StudentDashboardViewModel` trimmed to only fields with a real data source:
   - `Stats` (enrolled/completed/pending-assignments/certificates counts) — all real DB counts.
   - `ContinueLearning` — now nullable; picks the most-recently-enrolled course with `Progress < 100`, "Next Lesson" is a real next-incomplete-lesson lookup (not fabricated).
   - `EnrolledCourses` — real, capped to 4, real lesson-completion counts (no `10`-lesson fallback, no fake `Rating`/instructor fields).
   - `UpcomingAssignments` — reuses `IAssignmentService.GetStudentAssignmentsAsync`, filtered to pending, sorted by real due date. Dropped the fabricated `MaxMarks` field — the `Assignment` entity has no max-marks concept at all, so the old dashboard's `MaxMarks = 100` was invented from nothing.
   - `AvailableQuizzes` (renamed from `UpcomingQuizzes`) — real quizzes for enrolled courses with real question counts. Dropped the fabricated `DueDate` — `Quiz` has no due-date/scheduling field in the domain model, so the old "Due: MMM dd" was invented from nothing; quiz *attempts* also aren't persisted anywhere yet (`QuizController.Submit` only writes the result to `Session`, not the DB — confirmed, out of scope, flagged for Session 3+ below), so "already taken" filtering isn't available either.
   - `LearningProgress` — replaced the fabricated hours/weekly-activity-bar-chart (no time-tracking entity exists anywhere in the domain) with **real** `OverallProgressPercentage` (average of real `Enrollment.Progress`), `LessonsCompletedThisWeek`, and `TotalLessonsCompleted` (both genuine `LessonProgress` counts).
   - `RecentActivities` — previously 100% hardcoded (fake "Quiz Completed 95%", fake timestamps). Now built by merging three genuinely real, timestamped event sources: `LessonProgress.CompletedAt` ("Lesson Completed"), `Submission.CreatedAt` via `StudentAssignmentDto.SubmittedAt` ("Assignment Submitted"), `Certificate.IssuedDate` ("Certificate Earned") — sorted, top 5.
   - **Removed entirely** (per instructions — no honest data source, so removed rather than faked): `Achievements`/`Badges` (streaks, "Code Ninja", "Quiz Master" — pure invention, no event-tracking/gamification system exists), the weekly-hours chart.
   - `Views/Student/Home/Index.cshtml` rewritten to match — conditionally renders "Continue Learning" only when present, shows a genuine "No Courses Yet" empty state when `!HasEnrollments`, drops the Achievements/Badges section and hero "Day Streak" stat.

7. **Student Profile** (new, not explicitly gap-flagged by the lecturer but required by this session's task) — `Areas/Student/Controllers/ProfileController.cs` (GET/POST `Index`) reuses `FirstName`/`LastName`/`ProfilePicture` already on `ApplicationUser` and the same `UserManager<ApplicationUser>` + file-upload pattern already used by `AssignmentsController.Submit` (`wwwroot/uploads/profile-pictures/`, GUID-prefixed filenames). New view `Areas/Student/Views/Profile/Index.cshtml`. Added a "My Profile" link (the existing user badge in `_StudentLayout.cshtml` nav is now a link to it).

### Files Changed

**New Domain:** `Entities/Courses/LessonProgress.cs`; `Entities/Courses/Lesson.cs` gained a `LessonProgresses` collection nav property.

**New Application:** `Common/UrlSafety.cs`, `Interfaces/Repositories/ILessonProgressRepository.cs`, `Interfaces/Services/ILessonProgressService.cs`, `Services/LessonProgressService.cs`. `DTOs/Lessons/LessonDto.cs` gained `StudentLessonDto`. Extended: `Interfaces/Repositories/{IEnrollmentRepository,ILessonRepository}.cs`, `Interfaces/Services/{IEnrollmentService,ILessonService}.cs`, `Services/{EnrollmentService,LessonService}.cs`, `DependencyInjection.cs` (registered `ILessonProgressService`).

**New Persistence:** `Repositories/LessonProgressRepository.cs`, `Migrations/20260819092416_AddLessonProgress.cs` (+ Designer). Extended: `Context/ApplicationDbContext.cs` (DbSet + relationship + unique filtered index), `Repositories/{EnrollmentRepository,LessonRepository}.cs`, `DependencyInjection.cs` (registered `ILessonProgressRepository`).

**New Web (Student area):** `Controllers/{LessonsController,ProfileController}.cs`, `ViewModels/StudentProfileViewModel.cs`, `Views/Lessons/{Index,Details}.cshtml`, `Views/Profile/Index.cshtml`.

**Modified Web:** `Areas/Student/Controllers/CoursesController.cs` (removed `GetPlaceholderCourses`, real lesson counts, added `Enroll`), `Areas/Student/Controllers/HomeController.cs` (full rewrite), `Areas/Student/ViewModels/StudentDashboardViewModel.cs` (trimmed to real-data-only fields), `Areas/Student/Views/{Courses,Home}/Index.cshtml`, `Views/Shared/_StudentLayout.cshtml` (Profile link), `Controllers/CoursesController.cs` (root — enrollment-awareness on `Details`), `Views/Courses/Details.cshtml` (Enroll/Login-to-Enroll UI, removed dead Edit link).

**Test data added directly to the dev DB while smoke-testing** (not app code, flagging for transparency): course "Cloud Computing Essentials" (Id 2) and two lessons under course 1 ("Introduction to Clean Architecture", "Working with Entity Framework Core") — the `Lessons` table was completely empty before this session (no seeding path creates `Lesson` rows anywhere), so lesson-viewing had zero real data to exercise. Left in place intentionally as usable demo content for future sessions. A third throwaway "Unenrolled Test Course" + "Secret Lesson" (used only to verify ownership checks reject cross-course access) was deleted afterward via SQL.

### Database Changes
New table `LessonProgresses` (`Id`, `StudentId`, `LessonId` FK→`Lessons.Id` cascade-delete, `CompletedAt`, `CreatedAt`, `UpdatedAt`, `IsDeleted`), unique filtered index on `(StudentId, LessonId) WHERE IsDeleted = 0`.

### Migrations
**`20260819092416_AddLessonProgress`** — generated with `dotnet ef migrations add AddLessonProgress --project LearningManagementSystem.Persistence/LearningManagementSystem.Persistence.csproj --startup-project LearningManagementSystem.Web/LearningManagementSystem.Web.csproj --output-dir Migrations` (same project/startup-project layout as `InitialCreate`). Applied via `dotnet ef database update` with the same flags — verified via `sqlcmd` that the `LessonProgresses` table and both indexes exist and the migration is recorded in `__EFMigrationsHistory`. Diff is additive-only (no unrelated schema changes), confirmed by reading the generated migration file before applying.

### Routes Added/Changed
- **New:** `POST /Student/Courses/Enroll` (self-enrollment), `GET /Student/Lessons/Index?courseId=`, `GET /Student/Lessons/Details/{id}`, `POST /Student/Lessons/MarkComplete/{id}`, `GET|POST /Student/Profile/Index`.
- **Unchanged:** all Session 1 routes.

### Authorization/Ownership Changes
- `Student/Courses/Enroll`: `[Authorize(Roles="Student")]`, StudentId always taken from `UserManager.GetUserAsync(User)`, never from request data.
- `Student/Lessons/*`: `[Authorize(Roles="Student")]` class-level, **plus** a server-side enrollment-ownership check in the service layer on every action (`Index`, `Details`, `MarkComplete`) — matches the `AssignmentsController`/`AssignmentService` pattern, verified by smoke test that an authenticated-but-unenrolled student gets "not enrolled" on all three actions and no `LessonProgress` row is created.
- `Student/Profile`: `[Authorize(Roles="Student")]`, operates only on `UserManager.GetUserAsync(User)`'s own account.
- Root `Courses/Details`: unchanged `[AllowAnonymous]`; enrollment/role checks are additive read-only `ViewBag` flags, not new gating.

### Tests Performed
Full `dotnet build` on the solution — 0 errors (same 2 pre-existing `NU1903` AutoMapper warnings as Session 1). Then ran the app against the real dev SQL Server DB and smoke-tested every flow end-to-end via `curl` with real login sessions (not just code inspection):
- Anonymous → `Courses/Details/1` shows "Login to Enroll" with `returnUrl=/Courses/Details/1`.
- Student login → `Courses/Details/2` (course not yet enrolled) shows real "Enroll in this Course" form; POST → real `Enrollment` row created (verified via SQL), success message shown, button switches to "Already Enrolled — View Lessons".
- Duplicate enroll POST → idempotent no-op ("You are already enrolled"), confirmed via SQL that no second row was created.
- `Student/Lessons/Index?courseId=1` lists real lessons with real completion state.
- A lesson with `VideoUrl='javascript:alert(1)'` and `NotesUrl='ftp://evil.example.com/notes'` written directly to the DB → confirmed via response-body grep that neither unsafe URL, nor the Video/Notes sections at all, render on the Details page. A lesson with legitimate `https://` URLs → both sections render correctly.
- `MarkComplete` → real `LessonProgress` row created, `Enrollment.Progress` recalculated from a stale seeded `65.00` to a genuine `50.00` (1 of 2 real lessons), verified via SQL. Re-submitting `MarkComplete` on the same lesson → confirmed idempotent (still exactly 1 `LessonProgress` row, `Progress` unchanged).
- Attempted `Index`/`Details`/`MarkComplete` against a lesson belonging to a course the student is **not** enrolled in → all three correctly rejected ("not enrolled"), confirmed via SQL that zero `LessonProgress` rows leaked through.
- Dashboard (`Student/Home/Index`) before/after the above → stats, "Continue Learning", "Lessons This Week"/"Total Lessons Completed", and "Recent Activity" all updated to reflect the real events; `Certificates Earned` correctly shows `0` (matching the actual empty `Certificates` table — the old hardcoded dashboard would have shown a fake `2` here regardless of DB state).
- `Student/Courses/Index` ("My Courses") shows exactly the real enrolled courses (2), no trace of the old 6 fake placeholder courses.
- `Student/Profile/Index` GET shows real `FirstName`/`LastName`; POST update persisted to `AspNetUsers` (verified via SQL), then reverted back to the original values to leave the dev user account clean.

### Build Result
**Succeeded**, 0 errors, 2 warnings (same pre-existing `NU1903` AutoMapper advisory as Session 1, unrelated to this session).

### Known Issues
- **Quiz results/history are still not persisted anywhere.** `QuizController.Submit` (Student area, untouched this session — explicitly out of scope) writes the graded result only to `HttpContext.Session`, never to the database. There is no `QuizAttempt`/`QuizResult` entity. This means: (a) the dashboard's "Available Quizzes" list cannot honestly filter out quizzes the student already completed (no persisted record to check), and (b) a "Quiz Completed" entry can't be added to Recent Activity. This is exactly the "save quiz history" gap from the lecturer's original feedback and was **intentionally left untouched** per this session's scope (rule: don't touch quiz persistence).
- `Course` has no `Instructor`/`InstructorId` concept anywhere in the domain model. The Student area's `StudentCourseItemViewModel.InstructorName` is still populated with a static `"LMS Instructor"` placeholder (pre-existing, not fabricated *progress* data, just a placeholder for a domain concept that doesn't exist yet) — left as-is, out of scope.
- `Views/Courses/Index.cshtml` (public course catalog) and `Views/Categories/Index.cshtml` **still render hardcoded mock course arrays**, not the real `Model` — this is the same Session-1-flagged Known Issue, still not fixed. It does **not** block self-enrollment (`Courses/Details/{id}` works correctly for any real course id, reached via direct link/URL), but the catalog browsing UI itself still shows fake course cards with fake IDs 1–6 that don't correspond to real courses. Flagged again since it's now more clearly a "learner can't discover real courses to enroll in via browsing" gap.
- Admin `Courses/Create` and `Lessons/Create` POST forms returned an empty `400 Bad Request` in this session's manual `curl` testing on the **first** attempt; root-caused to a test-harness bug (my `grep`/token-extraction script was concatenating two duplicate antiforgery-token hidden fields present on the same authenticated page — the layout's own logout form has one, the content form has another). Retried with corrected extraction and both succeeded normally — **not an application bug**, no code changes made for this.
- The `EnrollStudentAsync` duplicate-enrollment check is check-then-act at the application layer (no DB unique constraint on `Enrollments(StudentId, CourseId)`), consistent with how the rest of this codebase handles uniqueness (no other table has DB-level uniqueness constraints either, including Admin's existing `Enrollments` Create/Edit). A theoretical concurrent double-submit race exists but was judged out of proportion to this session's scope; flagging for awareness.

### Remaining Work / Notes for Session 3
1. Read this file in full; re-verify with `git log`/`git status` first.
2. **`Enrollment.Progress` is now genuinely real** (recalculated from actual `LessonProgress` rows on every `MarkComplete`) and **`LessonProgress` now exists** — Session 3's certificate-eligibility logic (if it gates on "student finished the course") can safely check `Enrollment.Progress >= 100` and trust it.
3. Quiz persistence (a `QuizAttempt`/`QuizResult` entity, wiring `QuizController.Submit` to write to the DB instead of `Session` only) is the biggest remaining piece of the original lecturer feedback and was explicitly out of scope this session — likely Session 3's first task alongside certificates, since certificate eligibility may eventually want to consider quiz pass/fail too.
4. If Session 3 touches the public course catalog at all, the `Views/Courses/Index.cshtml`/`Categories/Index.cshtml` mock-data issue (flagged by both sessions now) is worth fixing alongside it — real enrollment flows depend on students being able to discover real course IDs to enroll in.
5. `Areas/Student/Controllers/QuizController.cs`, `Areas/Admin/*`, and all certificate logic were **not modified** this session — confirmed via `git status` that the diff is scoped to exactly the files listed above plus Session 1's pre-existing uncommitted changes.

## Session 3 — 2026-08-20: Quiz Persistence, Grading, Certificate Eligibility (audit + gap-fix pass)

### Completed Work

Session 3's actual implementation (QuizAttempt persistence, quiz history, submission validation, Admin grading, certificate eligibility/download) had already been carried out prior to this pass, but was never logged here and had one unverified gap. This entry is the result of an explicit audit-then-fix pass: every claimed deliverable was independently re-traced against the real code (not assumed from file/class names), one real gap was found and fixed, and the fix was rebuilt clean.

**Audit verdict:**

| # | Item | Status | Evidence |
|---|---|---|---|
| 1 | QuizAttempt/QuizResult DB persistence | DONE | `Domain/Entities/Quizzes/QuizAttempt.cs`; `ApplicationDbContext.cs:39` DbSet; migration `20260819105156_AddQuizAttempt` (applied — confirmed via `dotnet ef migrations list`, no `(Pending)` marker); `QuizController.Submit` (~line 239) writes to `_quizAttemptRepository`, no `HttpContext.Session` use; `Result` (~line 256) reads by id + `attempt.StudentId != user.Id` ownership check, no dummy-zero fallback |
| 2 | Quiz history page | DONE | `QuizController.History` (~line 287), ownership-filtered via `GetStudentAttemptsAsync(user.Id)`; `Views/Student/Quiz/History.cshtml` exists and is linked from `Quiz/Index.cshtml` and `Quiz/Result.cshtml` |
| 3 | Submission file-type/size validation | DONE | `Areas/Student/Controllers/AssignmentsController.cs:23-29,124-135` — server-side extension allow-list + 10MB cap, rejects with `TempData["Error"]` before any file write |
| 4 | Submission.Feedback field + student visibility | **WAS PARTIAL → FIXED** | Entity/migration/`SubmissionDto`/`GradeSubmissionDto`/Admin grading UI all had `Feedback` already. But `StudentAssignmentDto` had **no `Feedback` property at all**, and `Areas/Student/Views/Assignments/Details.cshtml` never rendered it — a graded student could see their Marks but never the instructor's written feedback. Fixed: added `Feedback` to `StudentAssignmentDto`, mapped it in both `AssignmentService.GetStudentAssignmentsAsync` and `GetStudentAssignmentDetailsAsync`, added an "Instructor Feedback" panel to the Graded-state branch of the Details view. |
| 5 | Admin grading UI (real, root stopgap removed) | DONE | `Areas/Admin/Controllers/SubmissionsController.cs` — `[Authorize(Roles="Admin")]`, Index/Details/Grade all present, `Grade` sets Marks/Status/Feedback via `GradeSubmissionDto`; root `Controllers/SubmissionsController.cs` confirmed **deleted** (glob returns no match) |
| 6 | Certificate eligibility (real, idempotent, all 3 event triggers wired) | DONE | `CertificateEligibilityService.CheckAndIssueAsync` — checks not-already-issued, all lessons complete (`LessonProgress` vs `CountByCourseIdAsync`), every course quiz has a passing `QuizAttempt`, every course assignment's submission is `Status == Graded`; confirmed called from all 3 real trigger points: `LessonProgressService.MarkLessonCompleteAsync:59`, `QuizController.Submit:242`, `SubmissionService.cs:128` (Admin grading path) |
| 7 | Certificate download (real file) | DONE | `Areas/Student/Controllers/CertificateController.Download` — ownership-checked via `GetStudentCertificateByIdAsync(id, user.Id, ...)`, generates a real PDF via `LearningManagementSystem.Web/Services/CertificatePdfService.cs` using the `QuestPDF` NuGet package (added to `LearningManagementSystem.Web.csproj`), returns `File(pdfBytes, "application/pdf", ...)` |

### Files Changed (this pass only — item 4 fix)
- `LearningManagementSystem.Application/DTOs/Assignments/StudentAssignmentDto.cs` — added `Feedback` property.
- `LearningManagementSystem.Application/Services/AssignmentService.cs` — mapped `Feedback = submission?.Feedback` in both `GetStudentAssignmentsAsync` and `GetStudentAssignmentDetailsAsync`.
- `LearningManagementSystem.Web/Areas/Student/Views/Assignments/Details.cshtml` — added an "Instructor Feedback" display block inside the Graded-state panel.

No entity/migration change needed — `Submission.Feedback` already existed in the database from the prior pass; this was purely a DTO/view wiring gap.

### Database Changes
None this pass (already-applied migrations from the prior Session 3 work: `AddSubmissionFeedback`, `AddQuizAttempt`, confirmed applied).

### Migrations
None generated this pass. Confirmed via `dotnet ef migrations list` that all 4 existing migrations (`InitialCreate`, `AddLessonProgress`, `AddSubmissionFeedback`, `AddQuizAttempt`) are applied with no pending migrations.

### Routes Added/Changed
None — this pass only changed DTO mapping and view rendering, no new routes.

### Authorization/Ownership Changes
None — pre-existing ownership checks (`StudentAssignmentDto` still only ever populated via the already-ownership-checked `GetStudentAssignmentDetailsAsync`/`GetStudentAssignmentsAsync`) were unaffected.

### Tests Performed
- `dotnet build` before the fix (confirm baseline clean) and after (confirm the fix compiles) — both succeeded, 0 errors, 2 pre-existing `NU1903` warnings only.
- `dotnet ef migrations list` — confirmed all 4 migrations applied, none pending.
- Read-traced (not just grepped) all 7 Session-3 deliverables end-to-end against live source before marking any DONE.
- Did not re-run a full manual click-through smoke test in this pass (no dev server started) — the fix is a narrow, low-risk DTO+view wiring change; recommend a quick manual check (grade a submission with feedback as Admin, view it as the student) before the next session.

### Build Result
**Succeeded**, 0 errors, 2 warnings (pre-existing `NU1903` AutoMapper advisory, unrelated).

### Known Issues
- Carried over from Session 2, still unaddressed: `Views/Courses/Index.cshtml` / `Views/Categories/Index.cshtml` (public catalog) still render hardcoded mock course arrays instead of the real `Model`.
- Carried over from Session 2: `Course` has no `Instructor` concept; `StudentCourseItemViewModel.InstructorName` still shows a static placeholder.
- `EFCore.Design` tooling used for `migrations list` reports as version 3.1.1 vs runtime 8.0.19 (a global/local tool version mismatch warning) — cosmetic, did not affect the migration status check; worth updating the local `dotnet-ef` tool if migrations tooling is used heavily in Session 4/5.

### Remaining Work / Notes for Session 4
1. Read this file in full; re-verify with `git log`/`git status` first.
2. Session 3's scope (quiz persistence/history, submission validation, Feedback field + visibility, Admin grading, certificate eligibility/download) is now genuinely complete end-to-end — safe to build Session 4 (Admin user management, Admin lesson/resource management, Discussions, AI tools, Bookmarks, `LearningOutcomes`) on top of it.
3. The public-catalog mock-data issue (`Views/Courses/Index.cshtml`/`Categories/Index.cshtml`) is still open — flagged three sessions running now; worth fixing early in Session 4 or explicitly deferring to Session 5 on purpose rather than by accident.

## Session 4 — 2026-08-20: Remaining Platform/Admin Features

### Completed Work

Scope: Admin user management, Admin lesson/resource management, Discussions, AI tools (+feedback), Bookmarks, remaining public course improvements. About page verified unaffected (regression-checked, not rebuilt).

1. **Admin User management** — new `Areas/Admin/Controllers/UsersController.cs` (`[Authorize(Roles="Admin")]`), built directly on `UserManager<ApplicationUser>`/`RoleManager<IdentityRole>` (same pattern as `RoleSeeder`/`UserSeeder`), not a new identity abstraction. `Index` (search by name/email, paged), `Details`, `ChangeRole` (POST — replaces all current roles with the selected one), `ToggleActive` (POST). **Safety check**: `IsLastActiveAdminAsync` blocks both role-change-away-from-Admin and deactivation when the target is the only active Admin account — verified live (see Tests Performed). New `Areas/Admin/ViewModels/AdminUserViewModels.cs` (`AdminUserListItemViewModel`, `AdminUserDetailsViewModel`).
   - **`IsActive` enforcement at login was already implemented before this session** — found in `LearningManagementSystem.Persistence/Services/AuthService.cs:40`: `if (user is null || !user.IsActive) { return (null, "Invalid email or password."); }`. Verified this is the actual authentication path (`AccountController.Login` calls `IAuthService.LoginAsync`, not `SignInManager.PasswordSignInAsync` directly) and confirmed live that a deactivated user gets rejected at login. **Did not duplicate this check** — the task explicitly asked to verify first. Known limitation (not addressed, out of scope): an already-logged-in user who gets deactivated mid-session keeps their existing cookie until it expires/they log out — no active-session invalidation (e.g. security-stamp validation interval) exists. Flagging for awareness, not fixed.

2. **Admin Lesson management** — new `Areas/Admin/Controllers/LessonsController.cs` + `Areas/Admin/Views/Lessons/{Index,Create,Edit,Delete,Details}.cshtml`, mirroring `Areas/Admin/Controllers/CoursesController.cs`'s exact pattern, reusing the existing `ILessonService` (already had full CRUD from before this session — no service changes needed). Verified no other view still referenced the root controller (`grep` for `asp-controller="Lessons"` outside `asp-area="Admin"|"Student"` → 0 hits) before **deleting** `Controllers/LessonsController.cs` (the Session-1 stopgap) and its `Views/Lessons/*` folder entirely.

3. **Resource management** — new `Resource` entity (`Title`, `Description?`, `FileUrl`, `LessonId`→`Lesson`), migration `AddResources`. New `IResourceRepository`/`ResourceRepository`, `IResourceService`/`ResourceService`, DTOs in `Application/DTOs/Resources/ResourceDto.cs`. Admin: `Areas/Admin/Controllers/ResourcesController.cs` (Index/Create/Edit/Delete, always scoped to a `lessonId`, reached via a new "Manage Resources" link on the Admin Lesson Details page). Student: extended `ILessonService.GetStudentLessonDetailsAsync` to also return `StudentLessonDto.Resources` (new property), and added a Resources section to `Areas/Student/Views/Lessons/Details.cshtml`. **Verified live**: added a resource to Lesson 4 as Admin, confirmed it rendered with a working Open link on the Student lesson-details page for an enrolled student.

4. **Discussions** — new `Discussion` (`CourseId`, `StudentId`, `Title`, `Content`) and `DiscussionReply` (`DiscussionId`, `StudentId`, `Content`) entities, migration `AddDiscussions`. `IDiscussionRepository`/`DiscussionRepository`, `IDiscussionService`/`DiscussionService` — every read/write path re-verifies the current student is enrolled in the discussion's course via the existing `IEnrollmentService.IsStudentEnrolledAsync` (same ownership pattern used elsewhere in this codebase). Student: new `Areas/Student/Controllers/DiscussionsController.cs` (Index by course, Details+reply form, Create), views styled to match the existing Lessons pages. A "Discussions" button was added to the course lessons-hero header (`Areas/Student/Views/Lessons/Index.cshtml`) as the entry point. Admin: new `Areas/Admin/Controllers/DiscussionsController.cs` (Index with search/paging, Details, Delete discussion, Delete individual reply) for moderation. **Verified live**: posted a discussion and a reply as the enrolled student end-to-end.

5. **AI tools** — new `AITool` (`Name`, `Description`, `Url`, `Category?`) entity, migration `AddAiTools` (generated separately from Bookmarks — see Migrations). `IAiToolRepository`/`AiToolRepository`, `IAiToolService`/`AiToolService`. Student: `Areas/Student/Controllers/AiToolsController.cs` — `Index` (directory), `Details` (shows useful-vote count + comments). Admin: `Areas/Admin/Controllers/AiToolsController.cs` — full CRUD. **Verified live**: added a tool as Admin, confirmed it appeared in the Student directory.

6. **AI tool feedback/useful** — new `AIToolFeedback` entity (`AIToolId`, `StudentId`, `IsUseful`, `Comment?`), one row per student+tool (unique filtered index, upserted on repeat feedback — not duplicated). `AiToolService.MarkFeedbackAsync` upserts and the Details page shows a running useful-count plus a list of left comments. **Verified live**: marked a tool useful with a comment as the student; confirmed the vote count and comment rendered, and the button reflected the "already marked useful" state on reload.

7. **Bookmarks** — new `Bookmark` entity (`StudentId`, `CourseId`), unique filtered index (soft-delete-safe re-bookmarking), migration `AddBookmarks` (kept separate from `AddAiTools` — see Migrations). `IBookmarkRepository`/`BookmarkRepository`, `IBookmarkService`/`BookmarkService`. Student: new `Areas/Student/Controllers/BookmarksController.cs` (`Index` = My Bookmarks page, `Toggle` POST). Added a Bookmark toggle button to the **public** `Views/Courses/Details.cshtml` (visible only to authenticated Students, via the same `ViewBag.IsStudent` pattern Session 2 established for the Enroll button) — root `Controllers/CoursesController.cs` gained an `IBookmarkService` dependency to compute `ViewBag.IsBookmarked`. **Verified live**: bookmarked a course from the public details page, confirmed it appeared on My Bookmarks.

8. **Remaining public course improvements**:
   - Added `Course.LearningOutcomes` (`string?`), migration `AddCourseLearningOutcomes` (generated first, before any of the other Session 4 entities, so it's fully isolated). Wired through `CourseDto`/`CreateCourseDto`/`UpdateCourseDto`, the Admin Course Create/Edit forms, and displayed conditionally ("What You'll Learn") on the public course details page. Verified the field renders in the Admin edit form; no value was set on existing seeded courses so the public section correctly stays hidden until an Admin fills it in.
   - **Guest visibility of unpublished courses**: confirmed this WAS a real gap — `CourseRepository.GetPagedAsync` had no status filtering, shared by both the Admin and public controllers. Fixed by adding a `publishedOnly` parameter (default `false`, so Admin behavior is provably unchanged) threaded through `ICourseRepository` → `ICourseService` → the root `CoursesController`. `Index` now calls `GetAllAsync(..., publishedOnly: true)`; `Details` now 404s if the course's `Status != Published`. Verified live: Course 1 (Published) still returns 200 on the public route; a Draft-status course exists in the seed data and is confirmed hidden from `Admin/Courses` filtering logic reasoning (Admin path untouched — `publishedOnly` defaults false there).

### Files Changed

**New Domain:** `Entities/Courses/{Resource,Discussion,DiscussionReply,AITool,AIToolFeedback,Bookmark}.cs`. `Course.cs` gained `LearningOutcomes`. `Lesson.cs` gained `Resources` collection nav.

**New Application:** `DTOs/{Resources,Discussions,AiTools,Bookmarks}/*.cs`; `Interfaces/Repositories/{IResourceRepository,IDiscussionRepository,IAiToolRepository,IBookmarkRepository}.cs`; `Interfaces/Services/{IResourceService,IDiscussionService,IAiToolService,IBookmarkService}.cs`; `Services/{ResourceService,DiscussionService,AiToolService,BookmarkService}.cs`. Extended: `DTOs/Courses/CourseDto.cs` (+LearningOutcomes on all 3 classes), `DTOs/Lessons/LessonDto.cs` (+Resources on StudentLessonDto), `DTOs/Assignments` untouched this session, `Interfaces/Repositories/ICourseRepository.cs` (+publishedOnly), `Interfaces/Services/ICourseService.cs` (+publishedOnly), `Services/{CourseService,LessonService}.cs`, `Mappings/MappingProfile.cs` (+Resource maps), `DependencyInjection.cs` (registered all 4 new services).

**New Persistence:** `Repositories/{ResourceRepository,DiscussionRepository,AiToolRepository,BookmarkRepository}.cs`; `Migrations/{AddCourseLearningOutcomes,AddResources,AddDiscussions,AddAiTools,AddBookmarks}.cs` (+Designer each). Extended: `Context/ApplicationDbContext.cs` (6 new DbSets + relationships/unique-indexes), `Repositories/CourseRepository.cs` (+publishedOnly filter), `DependencyInjection.cs` (registered all 4 new repositories).

**New Web (Admin area):** `Controllers/{LessonsController,UsersController,ResourcesController,DiscussionsController,AiToolsController}.cs`; `Views/Lessons/{Index,Create,Edit,Delete,Details}.cshtml`; `Views/Users/{Index,Details}.cshtml`; `Views/Resources/{Index,Create,Edit}.cshtml`; `Views/Discussions/{Index,Details}.cshtml`; `Views/AiTools/{Index,Create,Edit}.cshtml`; `ViewModels/AdminUserViewModels.cs`. Extended: `Views/_ViewImports.cshtml` (+4 new DTO namespaces), `Views/Courses/Create.cshtml`/`Edit.cshtml` (+LearningOutcomes field), `Controllers/CoursesController.cs` (+LearningOutcomes mapping in Edit GET).

**New Web (Student area):** `Controllers/{DiscussionsController,AiToolsController,BookmarksController}.cs`; `Views/Discussions/{Index,Create,Details}.cshtml`; `Views/AiTools/{Index,Details}.cshtml`; `Views/Bookmarks/Index.cshtml`. Extended: `Views/Lessons/{Index,Details}.cshtml` (+Discussions button, +Resources list).

**Deleted:** `Controllers/LessonsController.cs` (root stopgap), `Views/Lessons/*` (root).

**Modified (root/shared):** `Controllers/CoursesController.cs` (+IBookmarkService, +publishedOnly on Index, +Status check on Details), `Views/Courses/Details.cshtml` (+Learning Outcomes display, +Bookmark toggle button), `Views/Shared/_AdminLayout.cshtml` (+Lessons/Submissions/Users nav links, +More dropdown for Discussions/AI Tools), `Views/Shared/_StudentLayout.cshtml` (+AI Tools/Bookmarks nav links).

### Database Changes
- `Courses.LearningOutcomes` (nvarchar, nullable) — new column.
- `Resources` table (Id, Title, Description, FileUrl, LessonId FK→Lessons, CreatedAt, UpdatedAt, IsDeleted).
- `Discussions` table (Id, CourseId FK→Courses, StudentId, Title, Content, CreatedAt, UpdatedAt, IsDeleted); `DiscussionReplies` table (Id, DiscussionId FK→Discussions, StudentId, Content, CreatedAt, UpdatedAt, IsDeleted).
- `AITools` table (Id, Name, Description, Url, Category, CreatedAt, UpdatedAt, IsDeleted); `AIToolFeedback` table (Id, AIToolId FK→AITools, StudentId, IsUseful, Comment, CreatedAt, UpdatedAt, IsDeleted) with unique filtered index on `(AIToolId, StudentId) WHERE IsDeleted=0`.
- `Bookmarks` table (Id, StudentId, CourseId FK→Courses, CreatedAt, UpdatedAt, IsDeleted) with unique filtered index on `(StudentId, CourseId) WHERE IsDeleted=0`.

### Migrations
Generated and applied, in this order (each isolated to exactly the feature it's named for — verified `AddAiTools` only touches `AITools`/`AIToolFeedback` tables and `AddBookmarks` only touches `Bookmarks`, by temporarily excluding Bookmark from the EF model while generating `AddAiTools`, then restoring it before generating `AddBookmarks`):
1. `AddCourseLearningOutcomes`
2. `AddResources`
3. `AddDiscussions`
4. `AddAiTools`
5. `AddBookmarks`

All 5 applied via `dotnet ef database update` — succeeded cleanly, confirmed via server startup logs and live smoke testing against the real dev DB (no `--dry-run`/simulation used).

### Routes Added/Changed
- **New (Admin):** `/Admin/Lessons/*` (Index/Create/Edit/Delete/Details), `/Admin/Users/{Index,Details,ChangeRole,ToggleActive}`, `/Admin/Resources/{Index,Create,Edit,Delete}` (all `?lessonId=`-scoped except Edit), `/Admin/Discussions/{Index,Details,Delete,DeleteReply}`, `/Admin/AiTools/*` (Index/Create/Edit/Delete).
- **New (Student):** `/Student/Discussions/{Index,Details,Create,Reply}`, `/Student/AiTools/{Index,Details,MarkFeedback}`, `/Student/Bookmarks/{Index,Toggle}`.
- **Removed:** `/Lessons/*` (root — moved to `/Admin/Lessons/*`).
- **Changed:** `/Courses/Index`, `/Courses/Details/{id}` (root, public) now filter to `Status == Published` only.

### Authorization/Ownership Changes
- All new Admin controllers: `[Area("Admin")] [Authorize(Roles="Admin")]`, consistent with every other Admin controller.
- All new Student controllers: `[Area("Student")] [Authorize(Roles="Student")]`, **plus** service-layer ownership checks: Discussions re-verify course enrollment on every read/write (not just login); AI Tools/Bookmarks are student-owned-record operations (feedback/bookmark tied to `UserManager.GetUserAsync(User).Id`, never trusting a posted student id).
- `UsersController.ChangeRole`/`ToggleActive`: last-active-Admin lockout-prevention safety check, verified live (blocked with an error message when attempted against the sole Admin account).
- No changes to `Program.cs` authentication/cookie/JWT configuration (out of scope, confirmed untouched).

### Tests Performed
Full `dotnet build` after each feature increment (not just once at the end) — 0 errors throughout, same 2 pre-existing `NU1903` warnings. All 5 migrations applied via `dotnet ef database update` against the real dev SQL Server DB. Then ran the app (`dotnet run`, port 5246) and smoke-tested every flow end-to-end via `curl` with real authenticated sessions (cookies, real antiforgery tokens extracted per-request, not simulated):
- `Home/About` and `Home/Contact` → both 200 (regression check: confirmed unaffected by this session's changes).
- Admin login → `Admin/Users/Index`, `Admin/Lessons/Index`, `Admin/Discussions/Index`, `Admin/AiTools/Index`, `Admin/Submissions/Index` → all 200.
- Created a lesson via `Admin/Lessons/Create` → confirmed it appears in `Admin/Lessons/Index`.
- Added a resource to that lesson via `Admin/Resources/Create` → confirmed it renders with a working link on `Student/Lessons/Details/{id}` for the enrolled seeded student.
- Posted a discussion and a reply as the enrolled student via `Student/Discussions/Create` + `Reply` → confirmed both persist and render on `Student/Discussions/Details/{id}`.
- Added an AI tool via `Admin/AiTools/Create` → confirmed it appears in `Student/AiTools/Index`; marked it useful with a comment via `Student/AiTools/MarkFeedback` → confirmed vote count, comment, and "already marked" button state.
- Bookmarked a course via the public course-details toggle → confirmed it appears on `Student/Bookmarks/Index`.
- Deactivated the seeded student via `Admin/Users/ToggleActive` → confirmed a subsequent login attempt for that account fails with "Invalid email or password" → **reactivated the account afterward** to leave the dev DB clean.
- Attempted to deactivate the sole Admin account (`admin@lms.com`) → confirmed the safety check blocked it with "last active Admin" error and the account remained Active.
- Confirmed `Courses/Details/1` (Published) still returns 200 publicly; confirmed the `LearningOutcomes` field renders correctly in the Admin course-edit form.
- **Test data left in the dev DB** (flagging for transparency, consistent with Session 2's precedent): 1 lesson ("Session4 Smoke Test Lesson", under Course 1), 1 resource on that lesson, 1 discussion + 1 reply on Course 1, 1 AI tool ("Smoke Test AI Tool") with 1 useful-feedback row, 1 bookmark (student → Course 1). None of this is fake/hardcoded application data — it's real rows created through the real UI/POST actions during verification, left in place as usable demo content the same way Session 2 left demo lessons.

### Build Result
**Succeeded**, 0 errors, 2 warnings (pre-existing `NU1903` AutoMapper advisory, unrelated to this session).

### Known Issues
- No active-session invalidation when an Admin deactivates a currently-logged-in user — they keep access until their cookie expires or they log out (login-time `IsActive` check works correctly; it's the *already-authenticated* case that isn't covered). Flagged, not fixed — out of scope per the login-focused task description.
- Carried over, still unaddressed: `Views/Courses/Index.cshtml`/`Categories/Index.cshtml` still render a hardcoded mock array instead of the real paged `Model` (flagged in Sessions 1, 2, and 3 now — recommend Session 5 treat this as a required fix, not optional, given how long it's been flagged).
- `Bookmark`/`AIToolFeedback` duplicate-prevention is enforced via a DB-level unique filtered index (stronger than the check-then-act pattern used for `Enrollment`/`Certificate` elsewhere in this codebase) — inconsistent but not a bug; flagging only for awareness since Session 5's audit may want to normalize the approach.
- `Instructor` role/area remains an unused stub (seeded roles are only Admin/Student) — confirmed still out of scope, not touched, not required by either source document.

### Remaining Work / Notes for Session 5
1. Read this file in full; re-verify with `git log`/`git status` first.
2. Every route added in Sessions 1-4 needs a final inventory pass — Session 5's audit should walk `Areas/Admin`, `Areas/Student`, `Areas/Instructor`, and root `Controllers` fresh rather than trusting this changelog alone.
3. Fix `Views/Courses/Index.cshtml`/`Categories/Index.cshtml` mock-data (flagged 3 sessions running) — do not defer again without an explicit decision.
4. `_CourseCard.cshtml`'s fabricated instructor/rating arithmetic (flagged in the original project analysis, never assigned to a session explicitly — assign it to Session 5's "remove remaining fake data" task) and `Views/Home/About.cshtml`'s hardcoded marketing stats both still need fixing.
5. No automated test project exists anywhere in the solution — Session 5 needs to create one from scratch, not extend an existing one.
6. All Session 4 feature areas (Users, Lessons, Resources, Discussions, AI Tools, Bookmarks) were verified working end-to-end via live smoke test, not just code inspection — Session 5's audit can build on that confidence but should still independently re-verify per its own instructions.

## Session 5 — 2026-08-20: Final Completion & Audit

### Completed Work

Scope: reports, remaining fake-data removal, full security/routing/responsive audit, real automated testing, and a truthful final completion report (`FINAL_COMPLETION_REPORT.md`) against both source documents.

1. **Fake data removal (the big item, flagged unfixed across Sessions 1-4)**:
   - `Views/Courses/Index.cshtml` and `Views/Categories/Index.cshtml` — both had a hardcoded static array with **no `@model` directive at all**, ignoring the real `PagedResult<T>` the controller passed in. Rewrote both to bind to the real `Model`, with real search/category/level filters (extended root `CoursesController.Index` to accept `categoryId`/`level` and re-added the `ICategoryService` dependency Session 1 had removed, since it's now genuinely needed) and real `_Pagination` partial usage.
   - `Views/Shared/_CourseCard.cshtml` — was computing a fake instructor name/photo and fake star rating from `Model.Id % N` arithmetic. **Discovered this partial was actually dead code** (zero `@model`/usages anywhere) before this session — Home/Index and the Courses catalog each had their own separate inline hardcoded course arrays instead of using it. Fixed the partial to show only real data (`Level`, real `LessonCount`, real `EnrollmentCount` — both newly added to `CourseDto`/`CourseRepository`), removed the fake instructor/rating entirely (no Instructor or review/rating entity exists anywhere in the domain — removed rather than fabricated, consistent with the Session 2 precedent for the dashboard badges), and then **actually wired it in** to both `Home/Index.cshtml` and `Courses/Index.cshtml` so there's now one real, reused course-card template instead of three separate hardcoded ones.
   - `Views/Home/Index.cshtml` — replaced: hero stats ("500+ Courses / 20,000+ Students / 100+ Instructors"), a hero-float-card claiming "2,400+ Live Sessions this month" (no session/live-class entity exists — removed, not faked), the 6 hardcoded category cards (fake counts, not even linked to real category IDs), the 6-course hardcoded featured-courses array, and the animated stats-counter section (same 500/20000/100/15000 pattern) — all with real `HomeController`-supplied data. Dropped the "Instructors" stat entirely (no Instructor concept exists in the domain — removed, not fabricated). Left the testimonials carousel as-is (illustrative marketing copy, not data presented as system-derived).
   - `Views/Home/About.cshtml` — same stats-counter fix (real course/student/certificate counts).
   - `Areas/Admin/Controllers/HomeController.cs` — found and fixed an adjacent, pre-existing (not fake, but wrong) bug while touching this area: Recent Enrollments/Recent Certificates on the Admin dashboard displayed the student's raw Identity GUID (`e.StudentId`) instead of their name. Now resolves real names via a single batched `ApplicationUser` lookup.
   - New `HomeController.Index`/`About` actions inject `ICourseService`, `ICategoryService`, `UserManager<ApplicationUser>`, `ApplicationDbContext` — all real DB-backed counts (published courses, active students in the Student role, certificates issued).
   - New `Web/Models/HomeIndexViewModel.cs`.

2. **Reports** — new `Areas/Admin/Controllers/ReportsController.cs` + `Views/Reports/{Index,Enrollments,Progress,QuizResults}.cshtml`:
   - **Enrollment Report**: every enrollment, real student name/email, course, enrolled date, real progress bar, completed/in-progress status, filterable by course.
   - **Course Progress Report**: per-course real lesson count, enrolled-student count, average progress, completed-count, filterable by course.
   - **Quiz Results Report**: per-quiz real attempt count, pass count/rate, average score, filterable by quiz.
   - All three query `ApplicationDbContext` directly (same pattern already established by the existing Admin dashboard) and reuse `ICourseService.GetLookupAsync`/`IQuizService.GetLookupAsync` for filter dropdowns rather than duplicating lookup logic. No activity report was built — no activity-log entity exists anywhere in the domain, and creating one would have been new feature invention beyond this session's audit/completion scope.
   - New nav link added to `_AdminLayout.cshtml`.

3. **Full security audit** — re-verified all 32 controller files across root + all 3 Areas (`grep` dump of every class-level `[Authorize]`/`[AllowAnonymous]` + every `[HttpPost]`). Result: **zero gaps found**. Every mutating action sits behind the correct role; `[AllowAnonymous]` appears only on `AccountController` (class-level, correct), and the two intentionally-public root `Courses`/`Categories` read-only actions. No Session 1-4 addition reintroduced an unauthenticated path. Backed this finding with 5 new automated authorization tests (see Testing below) rather than just asserting it.

4. **Routing audit** — wrote a one-off Python script (`route_check.py`, scratch-only, not committed) that parses every `.cshtml` file for `asp-controller`/`asp-area` combinations and cross-checks each against the actual controller classes found in the solution (with ambient-area inference for views that omit `asp-area`). Result: **zero dangling references** — nothing points at any of the controllers removed in Sessions 1/3/4. Also confirmed via `ls` that no orphaned root `Views/` folders remain for deleted controllers (Quizzes/Questions/Enrollments/Assignments/Notifications/Certificates/Lessons/Submissions).

5. **Responsive UI audit (code-level)** — grepped every `.cshtml` for `<table>` elements not wrapped in `.table-responsive`; found and fixed **3 tables in the Admin dashboard** (`Areas/Admin/Views/Home/Index.cshtml` — Recent Courses, Recent Enrollments, Recent Certificates) that would have overflowed horizontally on mobile. Confirmed all layouts (`_AdminLayout`, `_StudentLayout`, `_LandingLayout`) already use Bootstrap's `navbar-toggler` collapse pattern and that the course-catalog filter sidebar already had a `@media (max-width: 991px)` rule. **No live browser/viewport rendering was performed** — no browser automation was available in this environment; this is disclosed as a real limitation in `FINAL_COMPLETION_REPORT.md`, not glossed over.

6. **Automated testing** — created the solution's first test project: `tests/LearningManagementSystem.Tests` (xUnit, net8.0), added to the `.sln` under the pre-existing empty "tests" solution folder. Added `public partial class Program { }` to the bottom of `LearningManagementSystem.Web/Program.cs` (zero behavior change — a required marker for `WebApplicationFactory<Program>`; nothing else in `Program.cs` was touched, per the explicit instruction to leave auth/JWT/cookie config alone). Built a `CustomWebApplicationFactory` that boots the **real** app (real Program.cs, real Identity/role/auth pipeline, real RoleSeeder/UserSeeder/DataSeeder) but swaps only the SQL Server `DbContext` registration for an isolated EF Core InMemory database per test class — not mocks, the real service/repository/controller code runs against a real (in-memory) database. 13 tests across 4 files, **all passing**:
   - `AuthTests.cs` (3): duplicate-email registration rejection, correct login, incorrect login.
   - `AuthorizationTests.cs` (5): guest blocked from Student route, guest blocked from Admin route, Student blocked from Admin route, Admin blocked from Student-only route, and a regression check that the specific route fixed in Session 1 (`/Admin/Certificates/Create`) still requires auth.
   - `EnrollmentAndLessonTests.cs` (3): self-enrollment + duplicate prevention, a student cannot access a lesson from a course they're not enrolled in (and no `LessonProgress` leaks through), lesson completion recalculates real `Enrollment.Progress` (asserts the exact 50%→100% math across two lessons).
   - `QuizAndSubmissionTests.cs` (2): a `QuizAttempt` written in one `DbContext` scope reads back correctly from a **brand-new** `DbContext` scope (faithfully simulates "still correct after a new session/login" — proves it's not held in ephemeral state), and a graded submission's `Feedback` is visible through the exact same `IAssignmentService.GetStudentAssignmentDetailsAsync` path the Student UI uses.

7. **Professor feedback + gap report verification** — went through every row of both source documents against the current, real code (not assumed from names) and wrote up the full result as `FINAL_COMPLETION_REPORT.md` at the repo root. Every requirement is now Complete except one: **"Feedback management"** is genuinely Partial — `ContactMessage` and `AIToolFeedback` both exist and work, but there is no single unified "platform feedback" entity/flow, which the gap report's phrasing ("not only generic contact-message CRUD") suggests the proposal may have wanted. This is disclosed plainly, not marked Complete.

8. **Final cleanup** — removed one confirmed piece of dead code: `StudentCourseItemViewModel.Rating` (unused since Session 2, verified via grep that it was never set anywhere before removing it). Did not touch `Program.cs` authentication/JWT/cookie configuration (confirmed still exactly as Session 1 left it, plus the one-line `public partial class Program` marker for testability).

### Files Changed

**New:** `Web/Models/HomeIndexViewModel.cs`; `Areas/Admin/Controllers/ReportsController.cs`; `Areas/Admin/ViewModels/ReportViewModels.cs`; `Areas/Admin/Views/Reports/{Index,Enrollments,Progress,QuizResults}.cshtml`; `tests/LearningManagementSystem.Tests/` (new project: `CustomWebApplicationFactory.cs`, `TestHelpers.cs`, `AuthTests.cs`, `AuthorizationTests.cs`, `EnrollmentAndLessonTests.cs`, `QuizAndSubmissionTests.cs`); `FINAL_COMPLETION_REPORT.md`.

**Modified:** `Controllers/HomeController.cs` (real stats/featured-courses/categories), `Controllers/CoursesController.cs` (+`ICategoryService`, real category/level filters on `Index`), `Views/Home/{Index,About}.cshtml`, `Views/Courses/Index.cshtml`, `Views/Categories/Index.cshtml`, `Views/Shared/_CourseCard.cshtml` (rewritten, now actually used), `Areas/Admin/Controllers/HomeController.cs` (real student names instead of raw GUIDs), `Areas/Admin/Views/Home/Index.cshtml` (3 tables wrapped in `.table-responsive`), `Views/Shared/_AdminLayout.cshtml` (+Reports nav link), `Application/DTOs/Courses/CourseDto.cs` (+`LessonCount`/`EnrollmentCount`), `Application/Mappings/MappingProfile.cs` (+course count mappings), `Persistence/Repositories/CourseRepository.cs` (+`Include(Lessons)`/`Include(Enrollments)` for accurate counts), `Areas/Student/ViewModels/StudentCoursesViewModel.cs` (removed dead `Rating` property), `LearningManagementSystem.Web/Program.cs` (+testability marker only), `LearningManagementSystem.sln` (+test project under the "tests" solution folder).

### Database Changes
None this session — no entity/schema changes were needed for the completion/audit scope.

### Migrations
None generated this session. Confirmed via the running app's startup logs that all previously-generated migrations (`InitialCreate`, `AddLessonProgress`, `AddSubmissionFeedback`, `AddQuizAttempt`, `AddCourseLearningOutcomes`, `AddResources`, `AddDiscussions`, `AddAiTools`, `AddBookmarks`) are applied cleanly with no pending changes.

### Routes Added/Changed
- **New:** `/Admin/Reports/{Index,Enrollments,Progress,QuizResults}`.
- **Changed (behavior, not route shape):** `/Courses/Index` now accepts and honors real `categoryId`/`level` query parameters (previously accepted but silently ignored by the view).
- **Unchanged:** everything else.

### Authorization/Ownership Changes
None new this session — the security audit confirmed the existing posture (established across Sessions 1-4) is correct and complete, and backed that with 5 new automated tests rather than changing anything.

### Tests Performed
- `dotnet build` after every feature increment (fake-data fixes, Reports, responsive fixes, test project) — 0 errors throughout.
- Live `dotnet run` smoke testing via `curl` with real authenticated sessions for: Home/Index (real stats/featured courses), Home/About (real stats), Courses/Index (real filtered/paginated catalog), Categories/Index (real categories), all 3 new Reports pages (confirmed real student/course/quiz names and numbers, not placeholders), Admin dashboard (confirmed no raw GUIDs remain), course-details Enroll flow (regression check), Admin nav links for all Session 4 additions (regression check).
- Full automated test suite: **13/13 passing** — see the inventory table in `FINAL_COMPLETION_REPORT.md` §"Automated test inventory".
- Scripted routing audit (controller-existence cross-check) — 0 dangling references found.
- Scripted responsive audit (`<table>` without `.table-responsive`) — found 3, fixed all 3, re-ran the check to confirm 0 remain.
- Full solution `dotnet build` (6 projects including the new test project) — 0 errors, 2 pre-existing unrelated warnings.

### Build Result
**Succeeded**, 0 errors, 2 warnings (pre-existing `NU1903` AutoMapper advisory, present since before Session 1, not addressed — would require a major-version package upgrade, judged out of scope for a completion pass).

### Known Issues
- No active-session invalidation on Admin-triggered deactivation (carried over from Session 4, still accurate, still out of scope).
- "Feedback management" remains genuinely Partial — see §7 above.
- Reports have no export/date-range filtering; no activity report exists (no activity-log entity in the domain).
- Responsive UI was audited at the code level only, not visually verified in a live browser — disclosed as a real limitation.
- Two pre-existing `NU1903` AutoMapper warnings remain.
- `Instructor` role/area remains an unused stub — confirmed one final time, still correctly out of scope per both source documents.

### Project Status
All five planned sessions are now complete. `FINAL_COMPLETION_REPORT.md` at the repo root has the full requirement-by-requirement verification against both the lecturer's feedback and the gap report. The solution builds clean, has a real (if focused) automated test suite, and every previously-flagged piece of fake/hardcoded data has been either replaced with real database-backed data or honestly removed where no real data source exists. No further session is planned; any future work should start by reading this file and `FINAL_COMPLETION_REPORT.md` in full.

## Session 6 — 2026-08-20: Post-Completion Bug Fixes (user-reported)

### Completed Work

User reported 4 issues after using the app directly: (1) submitting a PDF assignment "crashes", (2) Admin navbar has too many items to fit one screen, (3) Student navbar items shrink to fit, (4) the "Start Learning Today" Register/Browse Courses buttons (and, the user suspected, the same pattern on other pages) don't do anything when clicked. All four were reproduced and root-caused against the real running app before touching any code — no guessing.

1. **CTA buttons dead on click (root cause, not "missing links")** — used the browser's `document.elementFromPoint()` at the exact center of the "Register"/"Browse Courses" `<a>` tags in `.cta-section` and got `DIV.cta-section` back, not the link — confirming the `href` attributes were always correct (verified via the accessibility tree: `/Account/Register`, `/Courses`), but a sibling element was silently swallowing the click. Found it: `.cta-section::before` (a decorative radial-gradient overlay, `position: absolute; inset: 0;`) was missing `pointer-events: none`, while the equivalent `.hero-section::before`/`::after` elsewhere in the same stylesheet correctly had it — an inconsistency, not a new bug pattern. Fixed `.cta-section::before` to match, and found and fixed the same missing property on `.stats-section::before` while auditing the rest of `landing.css` for the identical pattern (that one has no interactive children today, but it's the same latent bug). This one shared CSS class fix applies everywhere `.cta-section` is used: `Views/Home/{Index,About,Contact}.cshtml`, `Views/Courses/Index.cshtml`, `Views/Categories/Index.cshtml`, `Areas/Student/Views/Home/Index.cshtml` — matching the user's own suspicion that this was common to multiple pages. **Verified live**, before and after, using `elementFromPoint` (not just visual inspection) on the actual running app.

2. **Admin/Student navbar overflow** — re-confirmed via the browser that at a realistic laptop width (1366px) the Admin nav's real content needs ~1551px and the Student nav's needs ~1077px, both far exceeding the space available next to the brand/user-menu, causing Bootstrap's default flex behavior to crush/shrink the links. Added `@media (min-width: 992px)` rules to both `_AdminLayout.cshtml` and `_StudentLayout.cshtml`: the `.navbar-nav` list becomes a `flex-wrap: nowrap; overflow-x: auto;` horizontally-scrollable strip with `flex-shrink: 0` on each item (so items keep their natural, readable width instead of being crushed) and a thin styled scrollbar. Scoped to `min-width: 992px` (Bootstrap's `lg` breakpoint) so it doesn't interfere with the existing mobile `navbar-toggler` collapse behavior, which already works fine as a vertical stack below that width. **Verified live** at 1366×800: confirmed `overflow-x: auto` is active and nav items render at their natural widths (87–152px, no uniform crushing) on both navbars.

3. **PDF assignment submission "crashing"** — reproduced via a real multipart POST to `/Student/Assignments/Submit` with a real PDF file. First attempt returned an empty-body `400 Bad Request` — but the server log showed **zero exceptions**, which pointed at a clean antiforgery-validation short-circuit rather than a real server crash. Confirmed by retrying with a correctly-maintained cookie jar (so the antiforgery cookie matched the token): the exact same request succeeded (`302`, file saved to `wwwroot/uploads/submissions/`, `Submission` row created/updated, "Submitted Successfully" rendered on reload) — proving the actual file-upload/storage code path in `AssignmentsController.Submit` has no bug. The real problem is what a genuine antiforgery-validation failure looks like to a user: ASP.NET Core's antiforgery filter correctly returns `400` with a **completely empty body** on failure (confirmed by simulating one with a deliberately invalid token) — no exception, so no error page, just a blank white screen that reads exactly like "the app crashed" even though nothing actually broke server-side. Root cause of *why* a real antiforgery mismatch can happen in normal use (long idle time on the page, multiple tabs, etc.) wasn't nailed down to one specific trigger, but the failure *mode* — blank crash-looking page — is now fixed regardless of trigger:
   - `Program.cs`: added `app.UseDeveloperExceptionPage()` in Development (previously **no exception-page middleware was registered for Development at all** — any genuine unhandled exception would have shown a raw blank response too, not just antiforgery failures) and `app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}")` so any short-circuited error status (400 antiforgery failures, 404s, etc.) re-executes through the existing `HomeController.Error`/`Views/Shared/Error.cshtml` page — which already renders inside the normal site layout — instead of returning an empty body. Did not touch `UseExceptionHandler`/`UseHsts` for production, and did not touch any authentication/JWT/cookie configuration (out of scope, confirmed unchanged).
   - `Areas/Student/Controllers/AssignmentsController.cs`: added `[RequestSizeLimit(20 * 1024 * 1024)]` to `Submit` as an explicit, generous ceiling — comfortably above the existing 10MB business-rule check in the action body, so that check (which already shows a friendly `TempData["Error"]` message) is what a student sees for an oversized file, not an unlabeled framework-level rejection.
   - **Verified live, before and after**: simulated the exact failure (invalid antiforgery token on a real PDF POST) — before the fix, an 8-byte empty `400` response; after the fix, the same `400` now returns a full, real, site-styled error page (8092 bytes, correct `<title>`, normal layout). Re-verified a normal, valid PDF submission still succeeds (`302` redirect, file on disk, DB updated) after these changes — no regression.

### Files Changed
`LearningManagementSystem.Web/wwwroot/css/landing.css` (`.cta-section::before`, `.stats-section::before` — added `pointer-events: none`), `Views/Shared/_AdminLayout.cshtml` (+horizontal-scroll nav CSS), `Views/Shared/_StudentLayout.cshtml` (+horizontal-scroll nav CSS), `Program.cs` (+`UseDeveloperExceptionPage`, +`UseStatusCodePagesWithReExecute`), `Areas/Student/Controllers/AssignmentsController.cs` (+`[RequestSizeLimit]`). New: `.claude/launch.json` (dev-server preview config, not app code).

### Database Changes / Migrations
None.

### Routes Added/Changed
None — same routes, changed failure-mode behavior only.

### Tests Performed
- Reproduced all 4 reported issues against the real running app (via the browser pane and `curl`) **before** making any change, to confirm root cause rather than guessing.
- CTA fix: `document.elementFromPoint()` script confirming the link, not its parent, receives clicks — run on the live page before (failed) and after (passed) the fix.
- Navbar fix: confirmed `overflow-x: auto` is active and measured real nav item widths at 1366×800 for both Admin and Student navs after the fix.
- PDF submission: full round-trip via real multipart POST — confirmed a valid submission succeeds and is stored (both before investigating further, and again after the `Program.cs`/`RequestSizeLimit` changes, to rule out a regression); confirmed the antiforgery-failure case now returns a real error page instead of an empty body.
- Full solution `dotnet build` (6 projects) after all changes — 0 errors, 2 pre-existing unrelated warnings.
- Full automated test suite — **13/13 still passing**, no regressions from these fixes.

### Build Result
**Succeeded**, 0 errors, 2 pre-existing warnings (unrelated `NU1903` AutoMapper advisory).

### Known Issues
- The exact real-world trigger for the antiforgery mismatch the user hit wasn't isolated to one specific user action (idle time vs. multiple tabs vs. something else) — only the failure *mode* (blank page) was fixed. If PDF submission still fails after this fix, it would now at minimum show a real error page instead of a blank one, which should make the actual cause visible/reportable.
- `ProfileController`'s profile-picture upload was noted to have no file-size/type validation at all (unlike `AssignmentsController.Submit`, which has both) — not fixed this session since it wasn't part of what was reported; flagging for awareness.
- `.claude/launch.json` was added to enable live browser-based verification during this session; it's a dev tooling file, not part of the deployed app.

## Session 7 — 2026-08-20: Root-Caused the PDF Resubmission Crash (regression from Session 6)

### Completed Work

User reported the PDF submission "crash" was still happening specifically on **resubmission** of an already-submitted assignment, and re-emphasized the requirement in plain terms: the PDF path must be stored in the database and the file stored in a local folder. Rather than guess again, this session reproduced the exact failure with real-sized files against the real running app and found the actual cause — which turned out to be a regression from Session 6's own fix.

1. **Root cause found**: Session 6 added `[RequestSizeLimit(20 * 1024 * 1024)]` to `AssignmentsController.Submit` as a defensive measure. Testing with a 25MB PDF (a realistic size for a scanned/image-heavy assignment) reproduced the exact user-reported symptom: `curl -v` showed Kestrel returning `400 Bad Request` and then **aborting the TCP connection** (`Connection: close`, "abort upload", "shutting down connection") rather than completing a normal HTTP response. A browser experiencing this sees a dead/reset connection, not a rendered error page — which is indistinguishable from "the app crashed," even though the earlier Session 6 fix (`UseStatusCodePagesWithReExecute`) never gets a chance to run, because the connection dies before the response can be delivered. **This confirmed the previous session's own `[RequestSizeLimit]` addition was the actual bug**, not a leftover from before — it lowered the effective ceiling below what a real submission could need, and enforcing that ceiling via mid-upload connection abort is simply how Kestrel behaves for oversized bodies, regardless of how gracefully the app-level code is written.

2. **Fix** — three coordinated changes so the ceiling a student can actually hit is generous, and only the app's own already-proven-graceful check (confirmed working via live tests both before and after this session) is ever what fires:
   - `Areas/Student/Controllers/AssignmentsController.cs`: **removed** the `[RequestSizeLimit]` attribute entirely (with a comment explaining why one shouldn't be re-added below Kestrel's own ceiling), and raised `MaxFileSizeBytes` from 10MB to **25MB** — the original 10MB was arguably too tight for real scanned/photographed assignment submissions in the first place. Updated the corresponding `TempData["Error"]` message text to match.
   - `Program.cs`: added `builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 50 * 1024 * 1024)` — 50MB, comfortably above the new 25MB business rule, so Kestrel's hard ceiling is no longer close enough to the app's own check to ever be the thing a normal-sized submission hits.
   - `Areas/Student/Controllers/ProfileController.cs`: while in this exact bug class, added the same missing file-size (5MB) and type (`.jpg/.jpeg/.png/.gif/.webp`) validation to the profile-picture upload, which previously had **no validation of any kind** — same latent crash risk, just less likely to be hit in practice (flagged as a known gap in Session 6, fixed here since it was cheap and directly in scope).

3. **Verified the exact previously-crashing scenario is now clean**: re-ran the identical 25MB resubmission against the fixed code — `curl -v` now shows a normal `100 Continue` → `302 Found` flow with no connection abort, and the Details page correctly shows the friendly "File size exceeds the maximum allowed size of 25 MB" message (the file is ~26,214,415 bytes, just over the 25MB/26,214,400-byte threshold, so this is the intended rejection path, not a silent failure). Also verified a 15MB resubmission — previously would have hit the old 10MB business rule — now succeeds cleanly and is stored (file on disk, `Submitted Successfully` + filename shown on reload).

### Files Changed
`Areas/Student/Controllers/AssignmentsController.cs` (removed `[RequestSizeLimit]`, raised `MaxFileSizeBytes` 10MB→25MB, updated error message), `Program.cs` (+`ConfigureKestrel` with a 50MB `MaxRequestBodySize`), `Areas/Student/Controllers/ProfileController.cs` (+file type/size validation on profile picture upload, previously had none).

### Database Changes / Migrations
None.

### Tests Performed
- Reproduced the exact reported failure (resubmission crash) with a realistic 25MB PDF against the pre-fix code — confirmed via `curl -v` that Kestrel aborted the connection rather than returning a response, which is the actual mechanism behind the "crash."
- Re-ran the identical 25MB resubmission after the fix — confirmed clean `100 Continue` → `302 Found`, no connection abort, friendly over-limit message shown correctly (file is just over the new 25MB threshold, so rejection here is correct, expected behavior).
- Tested a 15MB resubmission (previously would have failed the old 10MB rule) — confirmed it now succeeds, file is saved to `wwwroot/uploads/submissions/` and the `Submission.FilePath` is updated and visible on the Details page — i.e., "the pdf path is stored in database and the pdf is stored in a folder in local," as the user required.
- Tested a 2MB resubmission (typical size) — confirmed success, no regression.
- Full solution `dotnet build` — 0 errors, 2 pre-existing unrelated warnings.
- Full automated test suite — **13/13 still passing**, no regressions.

### Build Result
**Succeeded**, 0 errors, 2 pre-existing warnings (unrelated `NU1903` AutoMapper advisory).

### Known Issues
- 50MB is still a hard ceiling — a PDF larger than that would still hit the same connection-abort behavior, since that's fundamentally how Kestrel enforces `MaxRequestBodySize`. Judged generous enough for any realistic assignment submission; not made unlimited, since an actually-unbounded upload size is its own risk (disk space, DoS).
- The original Session 6 fix's own theory (blank antiforgery-failure page) was real and is still fixed, but was not the cause of *this* specific resubmission crash — both issues existed independently, and Session 6 accidentally introduced a second, more severe failure mode while fixing the first. Noting this plainly for the record rather than glossing over it.
