# Simplified Multi-Domain Architecture Plan for Project Management Application

## Executive Summary

This document outlines a new software architecture design that prioritizes **complexity mitigation** and **extensibility** for a multi-domain project management application. The architecture supports multiple business domains (Projects, Tasks, Users, etc.) while following simplicity-first principles, maintaining clear separation of concerns, and providing multiple extension points for future growth.

## Core Architecture Philosophy

### Guiding Principles

1. **Simplicity First**: Start with the minimal viable architecture and grow organically
2. **Clear Boundaries**: Well-defined layers with explicit interfaces
3. **Progressive Complexity**: Add complexity only when justified by real requirements
4. **Extension Ready**: Built-in hooks for future enhancements without over-engineering
5. **Developer Experience**: Minimize boilerplate, maximize productivity

### Complexity Mitigation Strategies

- **Convention over Configuration**: Leverage .NET 9 conventions
- **Composition over Inheritance**: Favor flexible composition patterns
- **Interface Segregation**: Small, focused contracts
- **Dependency Inversion**: Abstract dependencies for testability and flexibility

## Recommended Architecture: Layered Clean Architecture with Component-First Design

### Project Organization Strategy: Single Project with Domain Boundaries

**Decision**: Start with a **single .NET project approach** using domain folder organization, with a clear evolution path to separate projects when justified by specific requirements.

**Rationale**: 
- Prioritizes simplicity and development velocity for small teams
- Maintains clear domain boundaries through namespace organization
- Provides excellent performance for mobile/desktop applications
- Allows evolution to separate projects when benefits justify complexity

### Multi-Domain Layer Structure (Single Project)

```
📁 ProjectManagement.Solution/
├── 🎯 Core/
│   ├── ProjectManagement.Domain/
│   │   ├── Projects/                 # Project bounded context
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   └── Events/
│   │   ├── Tasks/                    # Task bounded context
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   └── Events/
│   │   ├── Users/                    # User bounded context
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   └── Services/
│   │   ├── Shared/                   # Cross-domain value objects
│   │   │   ├── ValueObjects/
│   │   │   └── Interfaces/
│   │   └── Common/                   # Domain events, base classes
│   │       ├── Events/
│   │       ├── Interfaces/
│   │       └── Base/
│   └── ProjectManagement.Application/
│       ├── Projects/                 # Project use cases
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── Services/
│       ├── Tasks/                    # Task use cases
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── Services/
│       ├── Users/                    # User use cases
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── Services/
│       ├── Coordination/             # Cross-domain coordination
│       │   └── Services/
│       └── Shared/                   # Cross-cutting concerns
│           ├── DTOs/
│           ├── Interfaces/
│           └── Services/
├── 🔧 Infrastructure/
│   ├── ProjectManagement.Persistence/
│   │   ├── Projects/                 # Project data access
│   │   ├── Tasks/                    # Task data access
│   │   ├── Users/                    # User data access
│   │   ├── Common/                   # Shared data access
│   │   └── Migrations/
│   └── ProjectManagement.External/   # Third-party integrations
├── 🎨 Presentation/
│   ├── ProjectManagement.UI.Shared/  # Shared Blazor components
│   │   ├── Projects/                 # Project-specific components
│   │   ├── Tasks/                    # Task-specific components
│   │   ├── Users/                    # User-specific components
│   │   ├── Common/                   # Shared UI components
│   │   └── Layouts/                  # Layout components
│   ├── ProjectManagement.MAUI/       # MAUI Blazor Hybrid app
│   └── ProjectManagement.Web/        # Web Blazor app (future)
└── 🧪 Tests/
    ├── ProjectManagement.Domain.Tests/
    │   ├── Projects/
    │   ├── Tasks/
    │   └── Users/
    ├── ProjectManagement.Application.Tests/
    │   ├── Projects/
    │   ├── Tasks/
    │   ├── Users/
    │   └── Coordination/
    └── ProjectManagement.UI.Tests/
        ├── Projects/
        ├── Tasks/
        └── Common/
```

## Layer Details

### 1. Domain Layer (ProjectManagement.Domain)

**Purpose**: Core business logic and entities organized by bounded context

#### Projects Domain
```csharp
// Project aggregate root
public class Project
{
    public ProjectId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    
    // Domain relationships (by ID to maintain boundaries)
    private readonly List<TaskId> _taskIds = new();
    public IReadOnlyList<TaskId> TaskIds => _taskIds.AsReadOnly();
    
    // Business logic encapsulated in entity
    public void AddTask(TaskId taskId)
    {
        if (!_taskIds.Contains(taskId))
        {
            _taskIds.Add(taskId);
            RaiseDomainEvent(new TaskAddedToProjectEvent(Id, taskId));
        }
    }
    
    public void CompleteProject()
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Project already completed");
            
        Status = ProjectStatus.Completed;
        RaiseDomainEvent(new ProjectCompletedEvent(Id, DateTime.UtcNow));
    }
}

// Project value objects
public record ProjectId(Guid Value);
public record ProjectStatus(string Name, int Order);
```

#### Tasks Domain
```csharp
// Task aggregate root
public class Task
{
    public TaskId Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public Priority Priority { get; private set; }
    public ProjectId? ProjectId { get; private set; }
    public UserId AssigneeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }

    // Business logic encapsulated in entity
    public void MarkComplete() 
    { 
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Task already completed");
            
        Status = TaskStatus.Completed;
        RaiseDomainEvent(new TaskCompletedEvent(Id, DateTime.UtcNow));
    }
    
    public void AssignToProject(ProjectId projectId)
    {
        var oldProjectId = ProjectId;
        ProjectId = projectId;
        RaiseDomainEvent(new TaskProjectChangedEvent(Id, oldProjectId, projectId));
    }
    
    public void UpdatePriority(Priority newPriority) 
    { 
        Priority = newPriority;
        RaiseDomainEvent(new TaskPriorityChangedEvent(Id, newPriority));
    }
}

// Task value objects
public record TaskId(Guid Value);
public record TaskStatus(string Name, int Order);
public record Priority(string Name, int Level, string Color);
```

#### Users Domain
```csharp
// User aggregate root
public class User
{
    public UserId Id { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    
    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Display name cannot be empty");
            
        DisplayName = newDisplayName;
        RaiseDomainEvent(new UserDisplayNameChangedEvent(Id, newDisplayName));
    }
    
    public void Deactivate()
    {
        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id));
    }
}

// User value objects
public record UserId(Guid Value);
public record UserRole(string Name, IReadOnlyList<Permission> Permissions);
public record Permission(string Name, string Description);
```

#### Shared Domain Components
```csharp
// Shared value objects
public record AuditInfo(UserId CreatedBy, DateTime CreatedAt, UserId? ModifiedBy, DateTime? ModifiedAt);

// Domain events base
public abstract record DomainEvent(DateTime OccurredAt) : IDomainEvent;

// Cross-domain events
public record TaskAddedToProjectEvent(ProjectId ProjectId, TaskId TaskId) : DomainEvent(DateTime.UtcNow);
public record TaskCompletedEvent(TaskId TaskId, DateTime CompletedAt) : DomainEvent(DateTime.UtcNow);
public record ProjectCompletedEvent(ProjectId ProjectId, DateTime CompletedAt) : DomainEvent(DateTime.UtcNow);
public record TaskProjectChangedEvent(TaskId TaskId, ProjectId? OldProjectId, ProjectId? NewProjectId) : DomainEvent(DateTime.UtcNow);
```

#### Domain Services
```csharp
// Project domain service
public interface IProjectDomainService
{
    bool CanCompleteProject(Project project, IEnumerable<Task> projectTasks);
    Task<ProjectSummary> CalculateProjectProgress(Project project, IEnumerable<Task> projectTasks);
}

// Task domain service
public interface ITaskDomainService
{
    bool CanCompleteTask(Task task);
    Task<RecurringTask> CreateRecurringTask(Task template, RecurrencePattern pattern);
    Priority CalculateAutoPriority(Task task, Project? project);
}
```

**Dependencies**: None (pure business logic)

**Complexity Mitigation**:
- Rich domain models with encapsulated behavior organized by bounded context
- Domain relationships maintained by ID references to preserve boundaries
- Domain events for cross-domain coordination
- No external dependencies
- Immutable value objects where appropriate

#### Namespace Organization Strategy
```csharp
// Clear namespace separation enforces domain boundaries
namespace ProjectManagement.Domain.Projects
{
    public class Project { /* ... */ }
    public record ProjectId(Guid Value);
    public interface IProjectDomainService { /* ... */ }
}

namespace ProjectManagement.Domain.Tasks
{
    public class Task { /* ... */ }
    public record TaskId(Guid Value);
    public interface ITaskDomainService { /* ... */ }
}

namespace ProjectManagement.Domain.Shared
{
    // Only shared types that truly belong to multiple domains
    public record AuditInfo(UserId CreatedBy, DateTime CreatedAt);
    public abstract record DomainEvent(DateTime OccurredAt);
}

namespace ProjectManagement.Domain.Common
{
    // Base classes and truly common infrastructure
    public abstract class Entity<TId> { /* ... */ }
    public interface IDomainEvent { /* ... */ }
}
```

### 2. Application Layer (ProjectManagement.Application)

**Purpose**: Orchestrate domain objects and define use cases, organized by domain with cross-domain coordination

#### Domain-Specific Application Services

**Projects Application Services**:
```csharp
// Project use cases
public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto> GetProjectByIdAsync(ProjectId id);
    Task<ProjectId> CreateProjectAsync(CreateProjectRequest request);
    Task UpdateProjectAsync(ProjectId id, UpdateProjectRequest request);
    Task DeleteProjectAsync(ProjectId id);
    Task CompleteProjectAsync(ProjectId id);
}

// Project DTOs
public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    Guid OwnerId,
    string OwnerDisplayName,
    DateTime CreatedAt,
    DateTime? DueDate,
    int TaskCount,
    int CompletedTaskCount
);

public record CreateProjectRequest(string Name, string Description, Guid OwnerId, DateTime? DueDate);
public record UpdateProjectRequest(string Name, string Description, DateTime? DueDate);
```

**Tasks Application Services**:
```csharp
// Task use cases
public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(ProjectId projectId);
    Task<TaskDto> GetTaskByIdAsync(TaskId id);
    Task<TaskId> CreateTaskAsync(CreateTaskRequest request);
    Task UpdateTaskAsync(TaskId id, UpdateTaskRequest request);
    Task DeleteTaskAsync(TaskId id);
    Task CompleteTaskAsync(TaskId id);
    Task AssignTaskToProjectAsync(TaskId taskId, ProjectId projectId);
}

// Task DTOs
public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? ProjectId,
    string? ProjectName,
    Guid AssigneeId,
    string AssigneeDisplayName,
    DateTime CreatedAt,
    DateTime? DueDate
);

public record CreateTaskRequest(string Title, string Description, Guid AssigneeId, Guid? ProjectId, string Priority, DateTime? DueDate);
public record UpdateTaskRequest(string Title, string Description, string Priority, DateTime? DueDate);
```

**Users Application Services**:
```csharp
// User use cases
public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(UserId id);
    Task<UserId> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(UserId id, UpdateUserRequest request);
    Task DeactivateUserAsync(UserId id);
}

// User DTOs
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    DateTime CreatedAt,
    bool IsActive
);

public record CreateUserRequest(string Email, string DisplayName, string Role);
public record UpdateUserRequest(string DisplayName);
```

#### Cross-Domain Coordination Services

```csharp
// Coordination service for complex cross-domain operations
public class ProjectTaskCoordinationService
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    private readonly IUserService _userService;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public async Task<ProjectWithTasksDto> GetProjectWithTasksAsync(ProjectId projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
        var owner = await _userService.GetUserByIdAsync(new UserId(project.OwnerId));
        
        return new ProjectWithTasksDto(
            project,
            tasks.ToList(),
            owner,
            CalculateProgress(tasks)
        );
    }
    
    public async Task<TaskId> CreateTaskInProjectAsync(ProjectId projectId, CreateTaskInProjectRequest request)
    {
        // Validate project exists and user has permission
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
            throw new ProjectNotFoundException(projectId);
            
        // Create task with project assignment
        var taskRequest = new CreateTaskRequest(
            request.Title,
            request.Description,
            request.AssigneeId,
            projectId.Value,
            request.Priority,
            request.DueDate
        );
        
        var taskId = await _taskService.CreateTaskAsync(taskRequest);
        
        // Publish cross-domain event
        await _eventPublisher.PublishAsync(new TaskAddedToProjectEvent(projectId, new TaskId(taskId)));
        
        return taskId;
    }
    
    public async Task CompleteProjectWithTasksAsync(ProjectId projectId)
    {
        // Get all project tasks
        var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
        
        // Complete all incomplete tasks
        foreach (var task in tasks.Where(t => t.Status != "Completed"))
        {
            await _taskService.CompleteTaskAsync(new TaskId(task.Id));
        }
        
        // Complete the project
        await _projectService.CompleteProjectAsync(projectId);
    }
    
    private ProjectProgress CalculateProgress(IEnumerable<TaskDto> tasks)
    {
        var taskList = tasks.ToList();
        var totalTasks = taskList.Count;
        var completedTasks = taskList.Count(t => t.Status == "Completed");
        
        return new ProjectProgress(
            totalTasks,
            completedTasks,
            totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0
        );
    }
}

// Cross-domain DTOs
public record ProjectWithTasksDto(
    ProjectDto Project,
    IReadOnlyList<TaskDto> Tasks,
    UserDto Owner,
    ProjectProgress Progress
);

public record ProjectProgress(int TotalTasks, int CompletedTasks, double PercentComplete);
public record CreateTaskInProjectRequest(string Title, string Description, Guid AssigneeId, string Priority, DateTime? DueDate);
```

#### Domain Event Handling

```csharp
// Event handlers for cross-domain coordination
public class ProjectTaskEventHandler : IDomainEventHandler<TaskCompletedEvent>
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    
    public async Task HandleAsync(TaskCompletedEvent domainEvent)
    {
        var task = await _taskService.GetTaskByIdAsync(domainEvent.TaskId);
        
        if (task.ProjectId.HasValue)
        {
            var projectId = new ProjectId(task.ProjectId.Value);
            var projectTasks = await _taskService.GetTasksByProjectIdAsync(projectId);
            
            // Auto-complete project if all tasks are done
            if (projectTasks.All(t => t.Status == "Completed"))
            {
                await _projectService.CompleteProjectAsync(projectId);
            }
        }
    }
}

public class TaskProjectEventHandler : IDomainEventHandler<TaskProjectChangedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    
    public async Task HandleAsync(TaskProjectChangedEvent domainEvent)
    {
        // Notify relevant users about task project assignment changes
        // This keeps domains decoupled while enabling business workflows
    }
}
```

**Dependencies**: Domain layer only

**Complexity Mitigation**:
- Domain-specific service interfaces with clear boundaries
- Coordination services handle cross-domain complexity
- DTOs prevent domain model leakage
- Event handlers enable loose coupling between domains
- Simple service implementations with minimal ceremony

### 3. Infrastructure Layer

#### Persistence (TaskManagement.Persistence)

**Purpose**: Data access implementation

```csharp
// Simple repository pattern
public interface ITaskRepository
{
    Task<IEnumerable<Task>> GetAllAsync();
    Task<Task?> GetByIdAsync(TaskId id);
    Task<TaskId> AddAsync(Task task);
    Task UpdateAsync(Task task);
    Task DeleteAsync(TaskId id);
}

// EF Core implementation
public class EfTaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;
    
    // Simple CRUD operations
    // No complex queries initially
}

// DbContext with conventions
public class TaskDbContext : DbContext
{
    public DbSet<Task> Tasks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simple configurations
        // Leverage EF conventions
    }
}
```

**Complexity Mitigation**:
- Repository pattern for data access abstraction
- EF Core with conventions over configuration
- No complex query objects initially

### 4. Presentation Layer

#### Shared Components (ProjectManagement.UI.Shared)

**Purpose**: Reusable Blazor components across platforms, organized by domain

#### Project Components
```csharp
// ProjectList.razor - Domain-specific component
@inject IProjectService ProjectService
@inject ProjectTaskCoordinationService CoordinationService

<div class="project-list">
    @if (IsLoading)
    {
        <LoadingSpinner />
    }
    else
    {
        @foreach (var project in Projects)
        {
            <ProjectCard 
                Project="project" 
                OnProjectSelected="HandleProjectSelected"
                OnProjectCompleted="HandleProjectCompleted" />
        }
    }
</div>

@code {
    [Parameter] public EventCallback<ProjectDto> OnProjectSelected { get; set; }
    
    private List<ProjectDto> Projects = new();
    private bool IsLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        Projects = (await ProjectService.GetAllProjectsAsync()).ToList();
        IsLoading = false;
    }
    
    private async Task HandleProjectSelected(ProjectDto project)
    {
        await OnProjectSelected.InvokeAsync(project);
    }
    
    private async Task HandleProjectCompleted(ProjectDto project)
    {
        await CoordinationService.CompleteProjectWithTasksAsync(new ProjectId(project.Id));
        await RefreshProjects();
    }
    
    private async Task RefreshProjects()
    {
        IsLoading = true;
        Projects = (await ProjectService.GetAllProjectsAsync()).ToList();
        IsLoading = false;
        StateHasChanged();
    }
}
```

```csharp
// ProjectCard.razor - Reusable project display component
@using ProjectManagement.Application.Shared.DTOs

<div class="project-card @GetStatusClass(Project.Status)">
    <div class="project-header">
        <h3 class="project-title" @onclick="() => OnProjectSelected.InvokeAsync(Project)">
            @Project.Name
        </h3>
        <div class="project-actions">
            @if (Project.Status != "Completed")
            {
                <button class="btn btn-success btn-sm" @onclick="() => OnProjectCompleted.InvokeAsync(Project)">
                    Complete
                </button>
            }
        </div>
    </div>
    
    <p class="project-description">@Project.Description</p>
    
    <div class="project-progress">
        <div class="progress-bar">
            <div class="progress-fill" style="width: @GetProgressPercentage()%"></div>
        </div>
        <span class="progress-text">@Project.CompletedTaskCount / @Project.TaskCount tasks</span>
    </div>
    
    <div class="project-meta">
        <span class="project-owner">Owner: @Project.OwnerDisplayName</span>
        @if (Project.DueDate.HasValue)
        {
            <span class="project-due-date">Due: @Project.DueDate.Value.ToString("MMM dd, yyyy")</span>
        }
    </div>
</div>

@code {
    [Parameter] public ProjectDto Project { get; set; } = null!;
    [Parameter] public EventCallback<ProjectDto> OnProjectSelected { get; set; }
    [Parameter] public EventCallback<ProjectDto> OnProjectCompleted { get; set; }
    
    private string GetStatusClass(string status) => status.ToLower() switch
    {
        "completed" => "project-completed",
        "in-progress" => "project-in-progress",
        "not-started" => "project-not-started",
        _ => "project-unknown"
    };
    
    private double GetProgressPercentage()
    {
        if (Project.TaskCount == 0) return 0;
        return (double)Project.CompletedTaskCount / Project.TaskCount * 100;
    }
}
```

#### Task Components
```csharp
// TaskList.razor - Domain-specific task component
@inject ITaskService TaskService

<div class="task-list">
    @if (IsLoading)
    {
        <LoadingSpinner />
    }
    else
    {
        @if (ProjectId.HasValue)
        {
            <h4>Tasks for Project</h4>
        }
        
        @foreach (var task in Tasks)
        {
            <TaskCard 
                Task="task" 
                OnTaskUpdated="HandleTaskUpdated"
                OnTaskCompleted="HandleTaskCompleted" />
        }
        
        @if (!Tasks.Any())
        {
            <EmptyState Message="@GetEmptyMessage()" />
        }
    }
</div>

@code {
    [Parameter] public Guid? ProjectId { get; set; }
    [Parameter] public EventCallback<TaskDto> OnTaskUpdated { get; set; }
    
    private List<TaskDto> Tasks = new();
    private bool IsLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadTasks();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await LoadTasks();
    }
    
    private async Task LoadTasks()
    {
        IsLoading = true;
        
        if (ProjectId.HasValue)
        {
            Tasks = (await TaskService.GetTasksByProjectIdAsync(new ProjectId(ProjectId.Value))).ToList();
        }
        else
        {
            Tasks = (await TaskService.GetAllTasksAsync()).ToList();
        }
        
        IsLoading = false;
    }
    
    private async Task HandleTaskUpdated(TaskDto task)
    {
        await OnTaskUpdated.InvokeAsync(task);
        await LoadTasks(); // Refresh to show updates
    }
    
    private async Task HandleTaskCompleted(TaskDto task)
    {
        await TaskService.CompleteTaskAsync(new TaskId(task.Id));
        await LoadTasks();
    }
    
    private string GetEmptyMessage() => ProjectId.HasValue 
        ? "No tasks in this project yet. Create a task to get started!"
        : "No tasks found. Create your first task!";
}
```

#### Common Shared Components
```csharp
// LoadingSpinner.razor - Shared across all domains
<div class="loading-spinner">
    <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
    <p class="loading-text">@Message</p>
</div>

@code {
    [Parameter] public string Message { get; set; } = "Loading...";
}
```

```csharp
// EmptyState.razor - Shared across all domains
<div class="empty-state">
    <div class="empty-state-icon">
        <i class="@IconClass"></i>
    </div>
    <h3 class="empty-state-title">@Title</h3>
    <p class="empty-state-message">@Message</p>
    @if (ActionText != null && OnAction.HasDelegate)
    {
        <button class="btn btn-primary" @onclick="OnAction">
            @ActionText
        </button>
    }
</div>

@code {
    [Parameter] public string IconClass { get; set; } = "fas fa-inbox";
    [Parameter] public string Title { get; set; } = "Nothing here yet";
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public string? ActionText { get; set; }
    [Parameter] public EventCallback OnAction { get; set; }
}
```

```csharp
// Modal.razor - Shared modal component
<div class="modal @(IsVisible ? "show" : "")" style="display: @(IsVisible ? "block" : "none")" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@Title</h5>
                <button type="button" class="btn-close" @onclick="Close"></button>
            </div>
            <div class="modal-body">
                @ChildContent
            </div>
            @if (ShowFooter)
            {
                <div class="modal-footer">
                    @if (FooterContent != null)
                    {
                        @FooterContent
                    }
                    else
                    {
                        <button type="button" class="btn btn-secondary" @onclick="Close">Close</button>
                        @if (OnSave.HasDelegate)
                        {
                            <button type="button" class="btn btn-primary" @onclick="Save">Save</button>
                        }
                    }
                </div>
            }
        </div>
    </div>
</div>

@if (IsVisible)
{
    <div class="modal-backdrop show"></div>
}

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public bool ShowFooter { get; set; } = true;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    
    private async Task Close()
    {
        IsVisible = false;
        await OnClose.InvokeAsync();
    }
    
    private async Task Save()
    {
        await OnSave.InvokeAsync();
    }
}
```

#### Component Organization Pattern
```
ProjectManagement.UI.Shared/
├── Projects/
│   ├── ProjectList.razor
│   ├── ProjectCard.razor
│   ├── ProjectForm.razor
│   └── ProjectDetails.razor
├── Tasks/
│   ├── TaskList.razor
│   ├── TaskCard.razor
│   ├── TaskForm.razor
│   └── TaskDetails.razor
├── Users/
│   ├── UserList.razor
│   ├── UserCard.razor
│   └── UserForm.razor
├── Common/
│   ├── LoadingSpinner.razor
│   ├── EmptyState.razor
│   ├── Modal.razor
│   ├── Button.razor
│   └── FormControls/
│       ├── TextInput.razor
│       ├── DatePicker.razor
│       └── Dropdown.razor
└── Layouts/
    ├── MainLayout.razor
    ├── NavigationMenu.razor
    └── PageHeader.razor
```

#### MAUI Application (TaskManagement.MAUI)

**Purpose**: Cross-platform mobile/desktop application

```csharp
// Simple program setup
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        // Simple service registration
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        return builder.Build();
    }
}
```

## State Management Strategy

### Phase 1: Component-Level State + Services

**For Simple Scenarios**:
```csharp
// Service with events for state changes
public class TaskStateService
{
    private readonly List<TaskDto> _tasks = new();
    private readonly ITaskService _taskService;
    
    public IReadOnlyList<TaskDto> Tasks => _tasks.AsReadOnly();
    public event Action? TasksChanged;
    
    public async Task LoadTasksAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        _tasks.Clear();
        _tasks.AddRange(tasks);
        TasksChanged?.Invoke();
    }
    
    public async Task AddTaskAsync(CreateTaskRequest request)
    {
        var taskId = await _taskService.CreateTaskAsync(request);
        await LoadTasksAsync(); // Simple refresh
    }
}
```

### Phase 2: Advanced State Management (When Needed)

**For Complex Scenarios** (implement only when requirements demand):
- Fluxor for Redux-like state management
- Event sourcing for audit trails
- Real-time updates with SignalR

## Dependency Injection Strategy

### Service Registration Pattern

```csharp
// Extension methods for clean registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskDomainService, TaskDomainService>();
        return services;
    }
    
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<TaskDbContext>(options => 
            options.UseSqlite("Data Source=tasks.db"));
        services.AddScoped<ITaskRepository, EfTaskRepository>();
        return services;
    }
}
```

**Service Lifetimes**:
- **Singleton**: Configuration, logging, caching services
- **Scoped**: Business services, repositories, DbContext
- **Transient**: Lightweight services, validators, mappers

## Extension Mechanisms

### 1. Multi-Domain Plugin Architecture

#### Domain-Specific Plugin Contracts
```csharp
// Base plugin interface
public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    IEnumerable<string> SupportedDomains { get; }
}

// Project domain plugins
public interface IProjectPlugin : IPlugin
{
    Task<bool> CanExecuteAsync(ProjectContext context);
    Task<PluginResult> ExecuteAsync(ProjectContext context);
}

// Task domain plugins
public interface ITaskPlugin : IPlugin
{
    Task<bool> CanExecuteAsync(TaskContext context);
    Task<PluginResult> ExecuteAsync(TaskContext context);
}

// User domain plugins
public interface IUserPlugin : IPlugin
{
    Task<bool> CanExecuteAsync(UserContext context);
    Task<PluginResult> ExecuteAsync(UserContext context);
}

// Cross-domain plugins for complex workflows
public interface ICrossDomainPlugin : IPlugin
{
    Task<bool> CanExecuteAsync(CrossDomainContext context);
    Task<PluginResult> ExecuteAsync(CrossDomainContext context);
}
```

#### Plugin Context Objects
```csharp
// Domain-specific contexts
public record ProjectContext(
    ProjectDto Project,
    IEnumerable<TaskDto> ProjectTasks,
    UserDto Owner,
    IDictionary<string, object> Properties
);

public record TaskContext(
    TaskDto Task,
    ProjectDto? Project,
    UserDto Assignee,
    IDictionary<string, object> Properties
);

public record UserContext(
    UserDto User,
    IEnumerable<ProjectDto> OwnedProjects,
    IEnumerable<TaskDto> AssignedTasks,
    IDictionary<string, object> Properties
);

public record CrossDomainContext(
    IEnumerable<ProjectDto> Projects,
    IEnumerable<TaskDto> Tasks,
    IEnumerable<UserDto> Users,
    IDictionary<string, object> Properties
);

// Plugin execution result
public record PluginResult(
    bool Success,
    string Message,
    IDictionary<string, object> OutputData,
    IEnumerable<DomainEvent> GeneratedEvents
);
```

#### Plugin Discovery and Management Service
```csharp
public class PluginService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PluginService> _logger;
    private readonly Dictionary<string, List<IPlugin>> _pluginsByDomain;
    
    public PluginService(IServiceProvider serviceProvider, ILogger<PluginService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pluginsByDomain = DiscoverPlugins();
    }
    
    // Get plugins for specific domain
    public async Task<IEnumerable<IProjectPlugin>> GetAvailableProjectPluginsAsync(ProjectContext context)
    {
        var plugins = _serviceProvider.GetServices<IProjectPlugin>();
        var availablePlugins = new List<IProjectPlugin>();
        
        foreach (var plugin in plugins)
        {
            if (await plugin.CanExecuteAsync(context))
                availablePlugins.Add(plugin);
        }
        
        return availablePlugins;
    }
    
    public async Task<IEnumerable<ITaskPlugin>> GetAvailableTaskPluginsAsync(TaskContext context)
    {
        var plugins = _serviceProvider.GetServices<ITaskPlugin>();
        var availablePlugins = new List<ITaskPlugin>();
        
        foreach (var plugin in plugins)
        {
            if (await plugin.CanExecuteAsync(context))
                availablePlugins.Add(plugin);
        }
        
        return availablePlugins;
    }
    
    // Execute plugin with error handling and logging
    public async Task<PluginResult> ExecutePluginAsync<T>(T plugin, object context) where T : IPlugin
    {
        try
        {
            _logger.LogInformation("Executing plugin {PluginName} v{Version}", plugin.Name, plugin.Version);
            
            return plugin switch
            {
                IProjectPlugin projectPlugin when context is ProjectContext projectContext 
                    => await projectPlugin.ExecuteAsync(projectContext),
                ITaskPlugin taskPlugin when context is TaskContext taskContext 
                    => await taskPlugin.ExecuteAsync(taskContext),
                IUserPlugin userPlugin when context is UserContext userContext 
                    => await userPlugin.ExecuteAsync(userContext),
                ICrossDomainPlugin crossPlugin when context is CrossDomainContext crossContext 
                    => await crossPlugin.ExecuteAsync(crossContext),
                _ => new PluginResult(false, "Unsupported plugin or context type", new Dictionary<string, object>(), Array.Empty<DomainEvent>())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plugin {PluginName} execution failed", plugin.Name);
            return new PluginResult(false, $"Plugin execution failed: {ex.Message}", new Dictionary<string, object>(), Array.Empty<DomainEvent>());
        }
    }
    
    private Dictionary<string, List<IPlugin>> DiscoverPlugins()
    {
        var pluginsByDomain = new Dictionary<string, List<IPlugin>>();
        
        // Discover all plugin types
        var pluginTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();
            
        foreach (var pluginType in pluginTypes)
        {
            var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
            
            foreach (var domain in plugin.SupportedDomains)
            {
                if (!pluginsByDomain.ContainsKey(domain))
                    pluginsByDomain[domain] = new List<IPlugin>();
                    
                pluginsByDomain[domain].Add(plugin);
            }
        }
        
        return pluginsByDomain;
    }
}
```

#### Example Plugin Implementations
```csharp
// Project automation plugin
public class ProjectTemplatePlugin : IProjectPlugin
{
    public string Name => "Project Template Creator";
    public string Description => "Creates projects from predefined templates";
    public string Version => "1.0.0";
    public IEnumerable<string> SupportedDomains => new[] { "Projects" };
    
    public Task<bool> CanExecuteAsync(ProjectContext context)
    {
        // Can execute if project is newly created and has no tasks
        return Task.FromResult(!context.ProjectTasks.Any());
    }
    
    public async Task<PluginResult> ExecuteAsync(ProjectContext context)
    {
        var templateName = context.Properties.GetValueOrDefault("TemplateName")?.ToString();
        
        if (string.IsNullOrEmpty(templateName))
        {
            return new PluginResult(false, "Template name is required", new Dictionary<string, object>(), Array.Empty<DomainEvent>());
        }
        
        // Create tasks from template
        var tasks = GetTemplateItems(templateName);
        var events = new List<DomainEvent>();
        
        foreach (var taskTemplate in tasks)
        {
            // Generate events for task creation
            events.Add(new TaskCreatedFromTemplateEvent(
                context.Project.Id,
                taskTemplate.Title,
                taskTemplate.Description
            ));
        }
        
        return new PluginResult(
            true,
            $"Created {tasks.Count} tasks from {templateName} template",
            new Dictionary<string, object> { ["CreatedTaskCount"] = tasks.Count },
            events
        );
    }
    
    private List<TaskTemplate> GetTemplateItems(string templateName)
    {
        // Return predefined task templates
        return templateName.ToLower() switch
        {
            "software-project" => new List<TaskTemplate>
            {
                new("Requirements Analysis", "Gather and document requirements"),
                new("Design Architecture", "Design system architecture"),
                new("Implementation", "Implement core features"),
                new("Testing", "Write and execute tests"),
                new("Deployment", "Deploy to production")
            },
            "marketing-campaign" => new List<TaskTemplate>
            {
                new("Market Research", "Research target audience"),
                new("Content Creation", "Create marketing content"),
                new("Campaign Launch", "Launch marketing campaign"),
                new("Performance Analysis", "Analyze campaign performance")
            },
            _ => new List<TaskTemplate>()
        };
    }
}

// Task automation plugin
public class TaskReminderPlugin : ITaskPlugin
{
    public string Name => "Task Reminder System";
    public string Description => "Sends reminders for overdue tasks";
    public string Version => "1.0.0";
    public IEnumerable<string> SupportedDomains => new[] { "Tasks" };
    
    public Task<bool> CanExecuteAsync(TaskContext context)
    {
        // Can execute if task has due date and is overdue
        return Task.FromResult(
            context.Task.DueDate.HasValue && 
            context.Task.DueDate.Value < DateTime.UtcNow &&
            context.Task.Status != "Completed"
        );
    }
    
    public async Task<PluginResult> ExecuteAsync(TaskContext context)
    {
        var daysOverdue = (DateTime.UtcNow - context.Task.DueDate!.Value).Days;
        
        var reminderEvent = new TaskReminderSentEvent(
            context.Task.Id,
            context.Task.AssigneeId,
            daysOverdue
        );
        
        return new PluginResult(
            true,
            $"Reminder sent for task overdue by {daysOverdue} days",
            new Dictionary<string, object> 
            { 
                ["DaysOverdue"] = daysOverdue,
                ["ReminderType"] = daysOverdue > 7 ? "Urgent" : "Standard"
            },
            new[] { reminderEvent }
        );
    }
}

// Cross-domain reporting plugin
public class ProjectProgressReportPlugin : ICrossDomainPlugin
{
    public string Name => "Project Progress Reporter";
    public string Description => "Generates comprehensive project progress reports";
    public string Version => "1.0.0";
    public IEnumerable<string> SupportedDomains => new[] { "Projects", "Tasks", "Users" };
    
    public Task<bool> CanExecuteAsync(CrossDomainContext context)
    {
        // Can execute if there are projects with tasks
        return Task.FromResult(context.Projects.Any() && context.Tasks.Any());
    }
    
    public async Task<PluginResult> ExecuteAsync(CrossDomainContext context)
    {
        var reportData = new Dictionary<string, object>();
        
        // Calculate overall statistics
        var totalProjects = context.Projects.Count();
        var completedProjects = context.Projects.Count(p => p.Status == "Completed");
        var totalTasks = context.Tasks.Count();
        var completedTasks = context.Tasks.Count(t => t.Status == "Completed");
        
        reportData["TotalProjects"] = totalProjects;
        reportData["CompletedProjects"] = completedProjects;
        reportData["ProjectCompletionRate"] = totalProjects > 0 ? (double)completedProjects / totalProjects * 100 : 0;
        reportData["TotalTasks"] = totalTasks;
        reportData["CompletedTasks"] = completedTasks;
        reportData["TaskCompletionRate"] = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;
        
        // Generate per-user statistics
        var userStats = context.Users.Select(user => new
        {
            UserId = user.Id,
            UserName = user.DisplayName,
            OwnedProjects = context.Projects.Count(p => p.OwnerId == user.Id),
            AssignedTasks = context.Tasks.Count(t => t.AssigneeId == user.Id),
            CompletedTasks = context.Tasks.Count(t => t.AssigneeId == user.Id && t.Status == "Completed")
        }).ToList();
        
        reportData["UserStatistics"] = userStats;
        
        var reportEvent = new ProjectProgressReportGeneratedEvent(
            DateTime.UtcNow,
            reportData
        );
        
        return new PluginResult(
            true,
            "Project progress report generated successfully",
            reportData,
            new[] { reportEvent }
        );
    }
}

// Supporting types
public record TaskTemplate(string Title, string Description);
public record TaskCreatedFromTemplateEvent(Guid ProjectId, string Title, string Description) : DomainEvent(DateTime.UtcNow);
public record TaskReminderSentEvent(Guid TaskId, Guid AssigneeId, int DaysOverdue) : DomainEvent(DateTime.UtcNow);
public record ProjectProgressReportGeneratedEvent(DateTime GeneratedAt, IDictionary<string, object> ReportData) : DomainEvent(DateTime.UtcNow);
```

#### Plugin Registration and Usage
```csharp
// Service registration in MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Register core services
        builder.Services.AddApplicationServices();
        
        // Register plugins
        builder.Services.AddTransient<IProjectPlugin, ProjectTemplatePlugin>();
        builder.Services.AddTransient<ITaskPlugin, TaskReminderPlugin>();
        builder.Services.AddTransient<ICrossDomainPlugin, ProjectProgressReportPlugin>();
        
        // Register plugin service
        builder.Services.AddSingleton<PluginService>();
        
        return builder.Build();
    }
}

// Usage in UI components
@inject PluginService PluginService

// In component code
private async Task ExecuteProjectPlugins()
{
    var context = new ProjectContext(selectedProject, projectTasks, projectOwner, new Dictionary<string, object>());
    var availablePlugins = await PluginService.GetAvailableProjectPluginsAsync(context);
    
    foreach (var plugin in availablePlugins)
    {
        var result = await PluginService.ExecutePluginAsync(plugin, context);
        if (result.Success)
        {
            // Handle successful plugin execution
            ShowMessage($"Plugin '{plugin.Name}' executed: {result.Message}");
        }
    }
}
```

### 2. Event-Driven Extensions

```csharp
// Simple event system
public interface ITaskEventPublisher
{
    Task PublishAsync<T>(T eventData) where T : class;
}

// Events for extension points
public record TaskCreatedEvent(TaskId TaskId, string Title);
public record TaskCompletedEvent(TaskId TaskId, DateTime CompletedAt);
public record TaskOverdueEvent(TaskId TaskId, DateTime DueDate);
```

### 3. Custom Field Extensions

```csharp
// Flexible field system
public interface ICustomFieldProvider
{
    Task<IEnumerable<CustomField>> GetFieldsAsync(string entityType);
    Task<object?> GetFieldValueAsync(string entityId, string fieldName);
    Task SetFieldValueAsync(string entityId, string fieldName, object? value);
}
```

## Testing Strategy

### Test Structure (Single Project with Domain Organization)
```
Tests/
├── Unit Tests/
│   ├── ProjectManagement.Domain.Tests/
│   │   ├── Projects/          # Project domain unit tests
│   │   ├── Tasks/             # Task domain unit tests
│   │   ├── Users/             # User domain unit tests
│   │   └── Shared/            # Shared domain tests
│   └── ProjectManagement.Application.Tests/
│       ├── Projects/          # Project service tests
│       ├── Tasks/             # Task service tests
│       ├── Users/             # User service tests
│       └── Coordination/      # Cross-domain coordination tests
├── Integration Tests/
│   └── ProjectManagement.Infrastructure.Tests/
│       ├── Database/          # EF Core integration tests
│       └── External/          # Third-party integration tests
├── Architectural Tests/       # NEW: Boundary enforcement
│   └── ProjectManagement.Architecture.Tests/
│       ├── DomainBoundaryTests.cs
│       ├── DependencyTests.cs
│       └── NamingConventionTests.cs
└── UI Tests/
    └── ProjectManagement.UI.Tests/
        ├── Projects/          # Project component tests
        ├── Tasks/             # Task component tests
        └── Common/            # Shared component tests
```

### Architectural Testing for Boundary Enforcement

```csharp
// DomainBoundaryTests.cs - Enforce domain separation
using NetArchTest.Rules;

[TestFixture]
public class DomainBoundaryTests
{
    [Test]
    public void Projects_Domain_Should_Not_Reference_Tasks_Domain()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain.Projects")
            .Should().NotHaveDependencyOn("ProjectManagement.Domain.Tasks")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            $"Projects domain should not depend on Tasks domain. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
    
    [Test]
    public void Tasks_Domain_Should_Not_Reference_Projects_Domain()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain.Tasks")
            .Should().NotHaveDependencyOn("ProjectManagement.Domain.Projects")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            $"Tasks domain should not depend on Projects domain. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
    
    [Test]
    public void Users_Domain_Should_Not_Reference_Other_Domains()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain.Users")
            .Should().NotHaveDependencyOn("ProjectManagement.Domain.Projects")
            .And().Should().NotHaveDependencyOn("ProjectManagement.Domain.Tasks")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            $"Users domain should not depend on other domains. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
    
    [Test]
    public void Domain_Should_Not_Reference_Application_Layer()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain")
            .Should().NotHaveDependencyOn("ProjectManagement.Application")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain layer should not depend on Application layer");
    }
    
    [Test]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain")
            .Should().NotHaveDependencyOn("ProjectManagement.Infrastructure")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain layer should not depend on Infrastructure layer");
    }
}

// DependencyTests.cs - Ensure clean dependencies
[TestFixture]
public class DependencyTests
{
    [Test]
    public void Application_Services_Should_Only_Depend_On_Domain_And_Shared()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Application")
            .And().HaveNameEndingWith("Service")
            .Should().OnlyHaveDependenciesOn(
                "ProjectManagement.Domain",
                "ProjectManagement.Application.Shared",
                "System",
                "Microsoft.Extensions",
                "AutoMapper"  // Example allowed external dependency
            )
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Application services should only depend on Domain and allowed external libraries");
    }
    
    [Test]
    public void Repositories_Should_Be_In_Infrastructure_Only()
    {
        var result = Types.InCurrentDomain()
            .That().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("ProjectManagement.Infrastructure")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Repository implementations should only be in Infrastructure layer");
    }
}

// NamingConventionTests.cs - Enforce consistent naming
[TestFixture]
public class NamingConventionTests
{
    [Test]
    public void Domain_Entities_Should_Not_Have_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain")
            .And().AreClasses()
            .And().AreNotAbstract()
            .Should().NotHaveNameEndingWith("Entity")
            .And().Should().NotHaveNameEndingWith("Model")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain entities should not have Entity or Model suffixes");
    }
    
    [Test]
    public void Application_DTOs_Should_Have_Dto_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Application")
            .And().AreClasses()
            .And().AreNotAbstract()
            .And().HaveNameEndingWith("Dto")
            .Should().BePublic()
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "DTOs should be public and have Dto suffix");
    }
    
    [Test]
    public void Domain_Services_Should_Have_Service_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("ProjectManagement.Domain")
            .And().AreInterfaces()
            .And().HaveNameStartingWith("I")
            .And().HaveNameContaining("Service")
            .Should().HaveNameEndingWith("Service")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain service interfaces should end with 'Service'");
    }
}
```

### Integration with CI/CD Pipeline

```yaml
# Example: Azure DevOps pipeline step for architectural tests
- task: DotNetCoreCLI@2
  displayName: 'Run Architectural Tests'
  inputs:
    command: 'test'
    projects: '**/*Architecture.Tests.csproj'
    arguments: '--configuration Release --logger trx --collect:"XPlat Code Coverage" --settings $(System.DefaultWorkingDirectory)/CodeCoverage.runsettings'
  condition: always()

- task: PublishTestResults@2
  displayName: 'Publish Architectural Test Results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
    searchFolder: '$(Agent.TempDirectory)'
    failTaskOnFailedTests: true
  condition: always()
```

### Required NuGet Packages for Architectural Testing

```xml
<!-- Add to test project -->
<PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
<PackageReference Include="NUnit" Version="3.13.3" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
```

### Testing Principles
- **Unit Tests**: Fast, isolated, test business logic per domain
- **Integration Tests**: Test data access and external dependencies
- **Component Tests**: Test UI components in isolation per domain
- **Architectural Tests**: Enforce domain boundaries and clean dependencies
- **End-to-End Tests**: Minimal, focused on critical cross-domain user journeys

### Testing Benefits in Single Project Approach
- **Fast Feedback**: Architectural tests catch boundary violations immediately
- **Documentation**: Tests serve as living documentation of architectural decisions
- **Refactoring Safety**: Safe to refactor within domains without breaking boundaries
- **Team Onboarding**: New developers understand boundaries through failing tests
- **Evolution Support**: Tests guide when to extract to separate projects

## Performance Considerations

### Blazor MAUI Optimizations
- **Lazy Loading**: Load components and data on demand
- **Virtualization**: Use `Virtualize` component for large lists
- **State Management**: Minimize re-renders with proper state handling
- **Bundle Size**: Tree-shake unused code, optimize assets

### Database Optimizations
- **Query Efficiency**: Use proper indexing, avoid N+1 queries
- **Connection Pooling**: Leverage EF Core connection pooling
- **Caching**: Implement simple in-memory caching for reference data

## Architecture Evolution Strategy

### Phase 1: Single Project Foundation (Recommended Start)

**Timeline**: Initial 6-12 months  
**Team Size**: 1-3 developers  
**Focus**: Establish domain boundaries and core functionality

#### Implementation Steps:
1. **Domain Establishment**
   - Implement domain models in separate namespaces
   - Create clear interfaces between domains
   - Establish domain events for cross-domain communication

2. **Boundary Enforcement**
   - Implement architectural tests to prevent unwanted dependencies
   - Use code reviews to maintain domain separation
   - Document domain responsibilities clearly

3. **Performance Optimization**
   - Leverage in-process communication for speed
   - Use shared transactions for data consistency
   - Optimize for mobile/desktop performance

#### Success Criteria:
- Clear domain boundaries maintained through discipline
- Fast development velocity
- Good performance on mobile devices
- Easy debugging and maintenance

### Phase 2: Boundary Hardening (6-18 months)

**Triggers for this phase**:
- Team grows beyond 3 developers
- Domain boundaries become frequently violated
- Need for different deployment schedules per domain

#### Implementation Steps:
1. **Extract Domain Contracts**
   - Create explicit interfaces for cross-domain operations
   - Implement messaging patterns for loose coupling
   - Add service bus or event store if needed

2. **Selective Project Extraction**
   - Start with the most independent domain (typically Users)
   - Keep tightly coupled domains together initially
   - Maintain shared infrastructure projects

#### Sample Extracted Structure:
```
📁 ProjectManagement.Solution/
├── 🎯 Core/
│   ├── ProjectManagement.Users.Domain/         # Extracted first
│   ├── ProjectManagement.Users.Application/
│   ├── ProjectManagement.ProjectsTasks.Domain/ # Keep coupled domains together
│   ├── ProjectManagement.ProjectsTasks.Application/
│   └── ProjectManagement.Shared/
├── 🔧 Infrastructure/
│   ├── ProjectManagement.Shared.Persistence/   # Shared infrastructure
│   └── ProjectManagement.Messaging/            # Cross-domain communication
```

### Phase 3: Full Microservices (18+ months)

**Triggers for this phase**:
- Independent scaling requirements per domain
- Different technology stacks needed
- Separate team ownership established
- Clear service boundaries proven in production

#### Implementation Steps:
1. **Complete Domain Separation**
   - Separate databases per domain
   - Implement event sourcing if needed
   - Add API gateways for external access

2. **Infrastructure Maturity**
   - Container orchestration (if needed)
   - Service mesh implementation
   - Advanced monitoring and observability

### Decision Matrix for Evolution

| Criteria | Stay Single | Extract Selectively | Full Separation |
|----------|-------------|-------------------|-----------------|
| **Team Size** | 1-3 devs | 4-8 devs | 8+ devs |
| **Deployment Frequency** | Weekly+ | Different per domain | Daily+ per domain |
| **Domain Coupling** | High | Medium | Low |
| **Performance Needs** | Mobile optimized | Mixed requirements | Service-specific |
| **Operational Complexity** | Low | Medium | High |
| **Development Velocity** | High | Medium | Lower initially |

### Evolution Monitoring

#### Key Metrics to Track:
```csharp
// Example: Architectural health metrics
public class ArchitecturalHealthMetrics
{
    public int CrossDomainDependencies { get; set; }
    public TimeSpan AverageBuildTime { get; set; }
    public int DeploymentFrequency { get; set; }
    public double TestCoverage { get; set; }
    public int TeamMergeConflicts { get; set; }
    
    public EvolutionRecommendation GetRecommendation()
    {
        if (CrossDomainDependencies > 10 || TeamMergeConflicts > 20)
            return EvolutionRecommendation.ConsiderExtraction;
            
        if (AverageBuildTime > TimeSpan.FromMinutes(5))
            return EvolutionRecommendation.OptimizeOrExtract;
            
        return EvolutionRecommendation.StayCurrentStructure;
    }
}
```

#### Warning Signs for Evolution:
- 🚨 **Frequent cross-domain dependency violations**
- 🚨 **Merge conflicts increasing with team growth**
- 🚨 **Build/deployment times becoming problematic**
- 🚨 **Different domains requiring different release schedules**
- 🚨 **Performance bottlenecks in specific domains**

### Migration Strategy from Existing Code

#### Phase 1: Foundation (Current → Single Project)
1. Create new solution structure with domain folders
2. Migrate existing models to appropriate domain namespaces
3. Implement domain services and coordination layer
4. Add architectural tests for boundary enforcement

#### Phase 2: Enhancement (Single Project Optimization)
1. Add comprehensive testing per domain
2. Implement plugin architecture
3. Add cross-domain event handling
4. Performance optimization

#### Phase 3: Selective Evolution (If Needed)
1. Extract domains based on actual pain points
2. Implement messaging between extracted domains
3. Maintain backward compatibility during transition
4. Monitor and optimize performance

## Technology Stack Recommendations

### Core Technologies
- **.NET 9.0**: Latest performance improvements and features
- **Blazor Hybrid**: Cross-platform UI with code sharing
- **Entity Framework Core**: Simple, convention-based ORM
- **SQLite**: Embedded database for simplicity (upgradeable to PostgreSQL/SQL Server)

### Development Tools
- **Hot Reload**: Fast development iteration
- **bUnit**: Blazor component testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests

### Future Considerations
- **MediatR**: When CQRS patterns become necessary
- **Fluxor**: For complex state management scenarios
- **SignalR**: For real-time features
- **AutoMapper**: For complex object mapping scenarios

## Success Metrics

### Simplicity Metrics
- **Lines of Code**: Minimize boilerplate and ceremony
- **Cyclomatic Complexity**: Keep methods and classes simple
- **Dependency Count**: Minimize external dependencies

### Extensibility Metrics
- **Time to Add Feature**: Measure development velocity
- **Code Reuse**: Components and services reused across features
- **Test Coverage**: Maintain high coverage with simple tests

### Maintainability Metrics
- **Code Review Time**: Quick understanding and review
- **Onboarding Time**: New developers productive quickly
- **Bug Fix Time**: Simple architecture enables fast fixes

## Conclusion

This architecture prioritizes simplicity while providing clear extension points for future growth. The layered approach with clean boundaries ensures that complexity can be added incrementally as requirements evolve, without requiring major architectural changes.

The focus on convention over configuration, composition over inheritance, and progressive complexity ensures that the codebase remains maintainable and extensible while avoiding over-engineering for current requirements.

Key benefits:
- ✅ **Low Initial Complexity**: Start simple, add complexity as needed
- ✅ **High Extensibility**: Multiple extension points without over-engineering
- ✅ **Clear Separation**: Well-defined layer boundaries
- ✅ **Testable**: Architecture supports comprehensive testing
- ✅ **Maintainable**: Simple patterns that team members can easily understand
- ✅ **Performant**: Optimized for mobile and desktop scenarios
- ✅ **Future-Proof**: Can evolve with changing requirements

This architecture serves as a solid foundation that can grow with your application's needs while maintaining simplicity and clarity throughout its evolution.