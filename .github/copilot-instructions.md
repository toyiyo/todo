# .github/copilot-instructions.yml

# Repository Description

description: |
  Toyiyo Todo Application is a modern project management tool built with ASP.NET Core and ASP.NET Boilerplate framework. Following Domain-Driven Design (DDD) principles, it implements a complete n-layer architecture with clear separation of concerns.

# Architecture Overview

## Domain Layer (Core Layer)
Contains domain objects, domain services, domain events:

```csharp
public class Job : FullAuditedEntity<Guid>, IMustHaveTenant 
{
    public enum Status { Open, InProgress, Done };
    public enum JobLevel { Task, SubTask, Epic, Bug };
    
    [Required]
    public Project Project { get; protected set; }
    public string Title { get; protected set; }
    public string Description { get; protected set; }
    public DateTime DueDate { get; protected set; }
    public User Owner { get; protected set; }
    public User Assignee { get; protected set; }
    public Status JobStatus { get; protected set; }
    public JobLevel Level { get; protected set; }

    // Domain logic methods
    public static Job Create(Project project, string title, string description, User user, int tenantId)
    {
        // Validation and business rules
        if (user == null) { throw new ArgumentNullException(nameof(user)); }
        if (tenantId <= 0) { throw new ArgumentNullException(nameof(tenantId)); }
        // ...
    }

    public static Job SetStatus(Job job, Status status, User user) 
    {
        // Business rules and validation
    }
}
```

## Domain Services
Handle complex domain operations:

```csharp
public class JobManager : DomainService, IJobManager
{
    private readonly IRepository<Job, Guid> _jobRepository;

    public async Task<Job> Create(Job inputJob)
    {
        // Domain logic
        return await _jobRepository.InsertAsync(inputJob);
    }
}
```

## Application Layer
Implements use cases, coordinates domain objects:

```csharp
public class JobAppService : todoAppServiceBase, IJobAppService
{
    private readonly IJobManager _jobManager;
    private readonly IProjectManager _projectManager;
    
    public async Task<JobDto> Create(JobCreateInputDto input)
    {
        var project = await _projectManager.Get(input.ProjectId);
        var job = Job.Create(
            project, 
            input.Title,
            input.Description,
            await GetCurrentUserAsync(),
            tenant.Id
        );
        
        await _jobManager.Create(job);
        return ObjectMapper.Map<JobDto>(job);
    }
}
```

## Infrastructure Layer
Handles data persistence and external concerns:

```csharp
public class JobRepository : EfCoreRepository<Job, Guid>
{
    public JobRepository(IDbContextProvider<AppDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<Job>> GetAllWithIncludes()
    {
        return await DbContext.Jobs
            .Include(j => j.Project)
            .Include(j => j.Assignee)
            .ToListAsync();
    }
}
```

## Presentation Layer
MVC Controllers and Views:

```csharp
public class JobsController : todoControllerBase
{
    private readonly IJobAppService _jobService;

    public async Task<IActionResult> Index(Guid projectId)
    {
        var jobs = await _jobService.GetAll(new GetAllJobsInput 
        { 
            ProjectId = projectId 
        });
        return View(jobs);
    }
}
```

# Key Features and Best Practices

- **Rich Domain Model**: Business rules are encapsulated in the domain layer
- **Protected Setters**: Forces use of domain methods for state changes
- **Domain Events**: For loose coupling between domain operations
- **Repository Pattern**: Abstracts data access behind interfaces
- **Unit of Work**: Ensures transactional consistency
- **Application Services**: Thin layer coordinating domain operations
- **DTOs**: Clean separation between layers
- **Validation**: Both domain and application level validation

# Coding Standards

1. Domain entities should protect their invariants:
   - Use protected setters
   - Validate in static factory methods
   - Throw domain exceptions for rule violations

2. Application services should:
   - Use DTOs for input/output
   - Not contain domain logic
   - Handle transaction boundaries
   - Map between DTOs and domain objects

3. Infrastructure should:
   - Implement interfaces defined in domain
   - Handle technical concerns
   - Not leak to upper layers

4. Controllers should:
   - Be thin
   - Only handle HTTP concerns
   - Delegate to application services

# Frontend Configuration

## Bundle Configuration
The application uses libman for client-side library management and a custom bundling system for optimizing frontend assets.

### Key Libraries
- Tribute.js: For @mentions functionality
- Marked: For markdown rendering
- SignalR: For real-time notifications
- JQuery UI: For UI interactions
- DataTables: For interactive tables

### Bundle Structure
```json
{
  "shared-layout.min.css": [
    "libs/font-awesome/css/all.min.css",
    "libs/tributejs/tribute.css",
    "libs/admin-lte/dist/css/adminlte.min.css",
    "libs/datatables/*.css",
    "css/style.css"
  ],
  "shared-layout.min.js": [
    "libs/jquery/jquery.js",
    "libs/jquery-ui/jquery-ui.min.js",
    "libs/bootstrap/dist/js/bootstrap.bundle.js",
    "libs/datatables/*.js",
    "libs/tributejs/tribute.min.js",
    "libs/marked/marked.min.js",
    "libs/signalr/*.js",
    "js/main.js"
  ]
}
```

### Library Management Rules
1. All third-party libraries should be managed through libman.json
2. CDN dependencies should be avoided - use local files for reliability
3. Bundle configurations should be maintained in bundleconfig.json
4. Development environment uses individual files, production uses minified bundles

### Script Loading Order
1. Core dependencies (jQuery, Bootstrap)
2. UI libraries (AdminLTE, DataTables)
3. Feature libraries (Tribute.js, Marked)
4. Application-specific code

### Notification System Integration
```javascript
abp.notifications.messageFormatters['toyiyo.todo.Notifications.NoteMentionNotificationData'] = 
  function (userNotification) {
    var data = userNotification.notification.data;
    return abp.localization.localize(
      'UserMentionedNotification',
      'todo',
      [data.senderUsername, data.jobTitle]
    );
};
```