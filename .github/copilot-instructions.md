# .github/copilot-instructions.yml

# Repository Description

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
    private readonly IProjectAppService _projectAppService;
    
    public async Task<JobDto> Create(JobCreateInputDto input)
    {
        // Don't create projects directly - use ProjectAppService
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test project" 
        });
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

### Notification System Architecture

The application uses ASP.NET Boilerplate's notification system with the following components:

1. **Notification Definition**
```csharp
public class TodoNotificationProvider : NotificationProvider
{
    public override void SetNotifications(INotificationDefinitionContext context)
    {
        context.Manager.Add(
            new NotificationDefinition(
                "Note.Mention",
                displayName: new LocalizableString("NoteMentionNotificationDefinition", "todo"),
                permissionDependency: null
            )
        );
    }
}
```

2. **Notification Data Types**
```csharp
[Serializable]
public class NoteMentionNotificationData : NotificationData
{
    public string Message { get; set; }
    public string JobTitle { get; set; }
    public string SenderUsername { get; set; }

    public NoteMentionNotificationData(string message, string jobTitle, string senderUsername)
    {
        Message = message;
        JobTitle = jobTitle;
        SenderUsername = senderUsername;
    }
}
```

3. **Publishing Notifications**
```csharp
public async Task NotifyMention(string message, string jobTitle, UserIdentifier targetUserId)
{
    await _notificationPublisher.PublishAsync(
        "Note.Mention",
        new NoteMentionNotificationData(message, jobTitle, AbpSession.GetUserName()),
        userIds: new[] { targetUserId }
    );
}
```

4. **Client-Side Integration**
```javascript
// Register notification formatter
abp.notifications.messageFormatters['toyiyo.todo.Notifications.NotificationData.NoteMentionNotificationData'] = 
    function (userNotification) {
        var data = userNotification.notification.data;
        var message = data.properties ? data.properties.Message : null;
        return message || 'You have been mentioned in a note';
    };

// Listen for notifications
abp.event.on('abp.notifications.received', function (userNotification) {
    abp.notifications.showUiNotifyForUserNotification(userNotification);
});
```

### Key Notification Rules

1. **Notification Types**:
   - Define clear notification names (e.g., "Note.Mention")
   - Use proper notification data classes
   - Register notification definitions in the module initializer

2. **Client Integration**:
   - Register formatters for each notification type
   - Use consistent naming across backend and frontend
   - Handle notification data structure correctly

3. **Data Structure**:
```javascript
// Example notification object structure
{
    "userId": 2,
    "state": 0,
    "notification": {
        "notificationName": "Note.Mention",
        "data": {
            "properties": {
                "Message": "@username mentioned you",
                "JobTitle": "Task title",
                "SenderUsername": "sender"
            },
            "type": "toyiyo.todo.Notifications.NotificationData.NoteMentionNotificationData"
        },
        "severity": 0,
        "creationTime": "2024-02-21T15:06:58.917627Z"
    }
}
```

# Testing Guidelines

## Test Project Setup
1. Create a new Class Library project under `aspnet-core/test` directory.
2. Add the following NuGet packages:
   - `Abp.TestBase`: Provides base classes for testing ABP based projects.
   - `Abp.EntityFrameworkCore`: For Entity Framework Core integration.
   - `Effort.EFCore`: For creating an in-memory database.
   - `xunit`: The testing framework.
   - `xunit.runner.visualstudio`: To run tests in Visual Studio.
   - `Shouldly`: For easy-to-read assertions.

## Base Test Class
Create a base test class to initialize the ABP system and set up an in-memory database:
```csharp
public abstract class TodoTestBase : AbpIntegratedTestBase<TodoTestModule>
{
    protected TodoTestBase()
    {
        UsingDbContext(context => new TodoInitialDataBuilder().Build(context));
    }

    protected override void PreInitialize()
    {
        LocalIocManager.IocContainer.Register(
            Component.For<DbConnection>()
                .UsingFactoryMethod(Effort.DbConnectionFactory.CreateTransient)
                .LifestyleSingleton()
        );

        base.PreInitialize();
    }

    public void UsingDbContext(Action<TodoDbContext> action)
    {
        using (var context = LocalIocManager.Resolve<TodoDbContext>())
        {
            context.DisableAllFilters();
            action(context);
            context.SaveChanges();
        }
    }

    public T UsingDbContext<T>(Func<TodoDbContext, T> func)
    {
        T result;

        using (var context = LocalIocManager.Resolve<TodoDbContext>())
        {
            context.DisableAllFilters();
            result = func(context);
            context.SaveChanges();
        }

        return result;
    }
}
```

## Test Module
Create a test module to define dependencies:
```csharp
[DependsOn(
    typeof(TodoDataModule),
    typeof(TodoApplicationModule),
    typeof(AbpTestBaseModule)
)]
public class TodoTestModule : AbpModule
{
}
```

## Initial Data Builder
Create an initial data builder to seed the database:
```csharp
public class TodoInitialDataBuilder
{
    public void Build(TodoDbContext context)
    {
        context.Users.AddOrUpdate(
            u => u.UserName,
            new User { UserName = "admin" },
            new User { UserName = "user1" }
        );
        context.SaveChanges();

        context.Projects.AddOrUpdate(
            p => p.Name,
            new Project { Name = "Project 1" },
            new Project { Name = "Project 2" }
        );
        context.SaveChanges();
    }
}
```

## Example Test
Create a test class to test application services:
```csharp
public class JobAppService_Tests : TodoTestBase
{
    private readonly IJobAppService _jobAppService;

    public JobAppService_Tests()
    {
        _jobAppService = LocalIocManager.Resolve<IJobAppService>();
    }

    [Fact]
    public void Should_Create_New_Jobs()
    {
        var initialJobCount = UsingDbContext(context => context.Jobs.Count());
        var project = UsingDbContext(context => context.Projects.First());

        _jobAppService.Create(new JobCreateInputDto
        {
            ProjectId = project.Id,
            Title = "New Job",
            Description = "Job Description"
        });

        UsingDbContext(context =>
        {
            context.Jobs.Count().ShouldBe(initialJobCount + 1);
            context.Jobs.FirstOrDefault(j => j.Title == "New Job").ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task Should_Do_Something()
    {
        // Arrange
        var currentUser = await GetCurrentUserAsync();
        var currentTenant = await GetCurrentTenantAsync();
        
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test project" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job", 
            Description = "test job" 
        });

        // Act
        var result = await _myService.DoSomething(job.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public async Task Should_Assign_Job_To_User()
    {
        // Arrange
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test project" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job" 
        });

        var assigneeUser = await GetUserByUserNameAsync("username");

        // Act
        await _jobAppService.AssignJob(new AssignJobInputDto
        {
            JobId = job.Id,
            UserId = assigneeUser.Id
        });

        // Assert
        var updatedJob = await _jobAppService.Get(job.Id);
        updatedJob.AssigneeId.ShouldBe(assigneeUser.Id);
    }

    [Fact]
    public async Task Should_Change_Job_Status()
    {
        // Arrange
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job" 
        });

        // Act
        await _jobAppService.SetJobStatus(new JobSetStatusInputDto
        {
            Id = job.Id,
            JobStatus = Status.Done
        });

        // Assert
        var updatedJob = await _jobAppService.Get(job.Id);
        updatedJob.Status.ShouldBe(Status.Done);
    }
}
```

## Running Tests
1. Open Visual Studio Test Explorer by selecting `TEST\Windows\Test Explorer`.
2. Click 'Run All' to execute all tests in the solution.

## Best Practices
- Use `UsingDbContext` methods to interact with the database.
- Follow the Arrange-Act-Assert pattern in test methods.
- Use `Shouldly` for assertions to improve readability.
- Ensure tests are isolated and do not depend on each other
- Never use new Job() or new Project() directly
- Always use application services to create entities
- Use GetCurrentUserAsync() and GetCurrentTenantAsync()
- Login as tenant admin in constructor
- Clean up test data in Dispose if needed
- Use meaningful test data names (e.g., "test project", "test job")
- Include both positive and negative test cases
- Test authorization rules
- Use Shouldly for assertions
- Follow Arrange-Act-Assert pattern
- Common Pitfalls to Avoid

- Don't create entities with new keyword
- Don't manipulate protected setters
- Don't bypass application services
- Don't create users manually
- Don't skip tenant context
- Don't use hardcoded IDs