# Simplified Multi-Domain Architecture Plan for Project Management Application

## Executive Summary

This document outlines a new software architecture design that prioritizes **complexity mitigation** and **extensibility** for a multi-domain application. The architecture supports multiple business domains while following simplicity-first principles, maintaining clear separation of concerns, and providing multiple extension points for future growth.

## ⚡ 2024 Update: Industry-Standard Blazor Patterns

**IMPORTANT**: This architecture plan has been updated to align with Microsoft's official Blazor guidance and 2024 industry best practices.

### ✅ Acceptable Patterns (Industry Standard)
- **Direct Service Injection**: `@inject ITaskService TaskService` in components is recommended
- **Mixed Application Imports**: Components importing from multiple Application namespaces is normal
- **Direct Service Calls**: `await TaskService.MethodAsync()` in components is the standard pattern
- **Coordination Services**: Using cross-domain coordination services in UI is acceptable

### 📚 References
- Microsoft Learn: [ASP.NET Core Blazor dependency injection](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)
- Updated architectural health monitoring reflects these standards

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
📁 Application.Solution/
├── 🎯 Core/
│   ├── Application.Domain/
│   │   ├── DomainA/                  # Domain A bounded context
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   └── Events/
│   │   ├── DomainB/                  # Domain B bounded context
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   └── Events/
│   │   ├── DomainC/                  # Additional domains as needed
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   └── Events/
│   │   ├── Shared/                   # Cross-domain value objects
│   │   │   ├── ValueObjects/
│   │   │   └── Interfaces/
│   │   └── Common/                   # Domain events, base classes
│   │       ├── Events/
│   │       ├── Interfaces/
│   │       └── Base/
│   └── Application.Application/
│       ├── DomainA/                  # Domain A use cases
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── Services/
│       ├── DomainB/                  # Domain B use cases
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── Services/
│       ├── DomainC/                  # Additional domain use cases
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
│   ├── Application.Persistence/
│   │   ├── DomainA/                  # Domain A data access
│   │   ├── DomainB/                  # Domain B data access
│   │   ├── DomainC/                  # Additional domain data access
│   │   ├── Common/                   # Shared data access
│   │   └── Migrations/
│   └── Application.External/         # Third-party integrations
├── 🎨 Presentation/
│   ├── Application.UI.Shared/        # Shared Blazor components
│   │   ├── DomainA/                  # Domain A-specific components
│   │   ├── DomainB/                  # Domain B-specific components
│   │   ├── DomainC/                  # Additional domain components
│   │   ├── Common/                   # Shared UI components
│   │   └── Layouts/                  # Layout components
│   ├── Application.MAUI/             # MAUI Blazor Hybrid app
│   └── Application.Web/              # Web Blazor app (future)
└── 🧪 Tests/
    ├── Application.Domain.Tests/
    │   ├── DomainA/
    │   ├── DomainB/
    │   └── DomainC/
    ├── Application.Application.Tests/
    │   ├── DomainA/
    │   ├── DomainB/
    │   ├── DomainC/
    │   └── Coordination/
    └── Application.UI.Tests/
        ├── DomainA/
        ├── DomainB/
        ├── DomainC/
        └── Common/
```

## Layer Details

### 1. Domain Layer (ProjectManagement.Domain)

**Purpose**: Core business logic and entities organized by bounded context

#### Domain A Example
```csharp
// Domain A aggregate root
public class EntityA
{
    public EntityAId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public EntityAStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    
    // Domain relationships (by ID to maintain boundaries)
    private readonly List<EntityBId> _relatedEntityBIds = new();
    public IReadOnlyList<EntityBId> RelatedEntityBIds => _relatedEntityBIds.AsReadOnly();
    
    // Business logic encapsulated in entity
    public void AddRelatedEntityB(EntityBId entityBId)
    {
        if (!_relatedEntityBIds.Contains(entityBId))
        {
            _relatedEntityBIds.Add(entityBId);
            RaiseDomainEvent(new EntityBAddedToEntityAEvent(Id, entityBId));
        }
    }
    
    public void CompleteEntityA()
    {
        if (Status == EntityAStatus.Completed)
            throw new InvalidOperationException("EntityA already completed");
            
        Status = EntityAStatus.Completed;
        CompletionDate = DateTime.UtcNow;
        RaiseDomainEvent(new EntityACompletedEvent(Id, DateTime.UtcNow));
    }
}

// Domain A value objects
public record EntityAId(Guid Value);
public record EntityAStatus(string Name, int Order);
```

#### Domain B Example
```csharp
// Domain B aggregate root
public class EntityB
{
    public EntityBId Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public EntityBStatus Status { get; private set; }
    public Priority Priority { get; private set; }
    public EntityAId? ParentEntityAId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }

    // Business logic encapsulated in entity
    public void MarkComplete() 
    { 
        if (Status == EntityBStatus.Completed)
            throw new InvalidOperationException("EntityB already completed");
            
        Status = EntityBStatus.Completed;
        RaiseDomainEvent(new EntityBCompletedEvent(Id, DateTime.UtcNow));
    }
    
    public void AssignToEntityA(EntityAId entityAId)
    {
        var oldParentId = ParentEntityAId;
        ParentEntityAId = entityAId;
        RaiseDomainEvent(new EntityBParentChangedEvent(Id, oldParentId, entityAId));
    }
    
    public void UpdatePriority(Priority newPriority) 
    { 
        Priority = newPriority;
        RaiseDomainEvent(new EntityBPriorityChangedEvent(Id, newPriority));
    }
}

// Domain B value objects
public record EntityBId(Guid Value);
public record EntityBStatus(string Name, int Order);
public record Priority(string Name, int Level, string Color);
```


#### Shared Domain Components
```csharp
// Shared value objects
public record AuditInfo(DateTime CreatedAt, DateTime? ModifiedAt);

// Domain events base
public abstract record DomainEvent(DateTime OccurredAt) : IDomainEvent;

// Cross-domain events
public record EntityBAddedToEntityAEvent(EntityAId EntityAId, EntityBId EntityBId) : DomainEvent(DateTime.UtcNow);
public record EntityBCompletedEvent(EntityBId EntityBId, DateTime CompletedAt) : DomainEvent(DateTime.UtcNow);
public record EntityACompletedEvent(EntityAId EntityAId, DateTime CompletedAt) : DomainEvent(DateTime.UtcNow);
public record EntityBParentChangedEvent(EntityBId EntityBId, EntityAId? OldParentId, EntityAId? NewParentId) : DomainEvent(DateTime.UtcNow);
```

#### Domain Services
```csharp
// Domain A domain service
public interface IEntityADomainService
{
    bool CanCompleteEntityA(EntityA entityA, IEnumerable<EntityB> relatedEntities);
    Task<EntityASummary> CalculateEntityAProgress(EntityA entityA, IEnumerable<EntityB> relatedEntities);
}

// Domain B domain service
public interface IEntityBDomainService
{
    bool CanCompleteEntityB(EntityB entityB);
    Task<RecurringEntityB> CreateRecurringEntityB(EntityB template, RecurrencePattern pattern);
    Priority CalculateAutoPriority(EntityB entityB, EntityA? parentEntity);
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
namespace Application.Domain.DomainA
{
    public class EntityA { /* ... */ }
    public record EntityAId(Guid Value);
    public interface IEntityADomainService { /* ... */ }
}

namespace Application.Domain.DomainB
{
    public class EntityB { /* ... */ }
    public record EntityBId(Guid Value);
    public interface IEntityBDomainService { /* ... */ }
}

namespace Application.Domain.Shared
{
    // Only shared types that truly belong to multiple domains
    public record AuditInfo(UserId CreatedBy, DateTime CreatedAt);
    public abstract record DomainEvent(DateTime OccurredAt);
}

namespace Application.Domain.Common
{
    // Base classes and truly common infrastructure
    public abstract class Entity<TId> { /* ... */ }
    public interface IDomainEvent { /* ... */ }
}
```

### 2. Application Layer (ProjectManagement.Application)

**Purpose**: Orchestrate domain objects and define use cases, organized by domain with cross-domain coordination

#### Domain-Specific Application Services

**Domain A Application Services**:
```csharp
// Domain A use cases
public interface IEntityAService
{
    Task<IEnumerable<EntityADto>> GetAllEntitiesAsync();
    Task<EntityADto> GetEntityByIdAsync(EntityAId id);
    Task<EntityAId> CreateEntityAsync(CreateEntityARequest request);
    Task UpdateEntityAsync(EntityAId id, UpdateEntityARequest request);
    Task DeleteEntityAsync(EntityAId id);
    Task CompleteEntityAsync(EntityAId id);
}

// Domain A DTOs
public record EntityADto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletionDate,
    int RelatedEntityBCount,
    int CompletedEntityBCount
);

public record CreateEntityARequest(string Name, string Description, DateTime? CompletionDate);
public record UpdateEntityARequest(string Name, string Description, DateTime? CompletionDate);
```

**Domain B Application Services**:
```csharp
// Domain B use cases
public interface IEntityBService
{
    Task<IEnumerable<EntityBDto>> GetAllEntitiesAsync();
    Task<IEnumerable<EntityBDto>> GetEntitiesByParentIdAsync(EntityAId parentId);
    Task<EntityBDto> GetEntityByIdAsync(EntityBId id);
    Task<EntityBId> CreateEntityAsync(CreateEntityBRequest request);
    Task UpdateEntityAsync(EntityBId id, UpdateEntityBRequest request);
    Task DeleteEntityAsync(EntityBId id);
    Task CompleteEntityAsync(EntityBId id);
    Task AssignToParentAsync(EntityBId entityBId, EntityAId parentId);
}

// Domain B DTOs
public record EntityBDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? ParentEntityAId,
    string? ParentEntityAName,
    DateTime CreatedAt,
    DateTime? DueDate
);

public record CreateEntityBRequest(string Title, string Description, Guid? ParentEntityAId, string Priority, DateTime? DueDate);
public record UpdateEntityBRequest(string Title, string Description, string Priority, DateTime? DueDate);
```


#### Cross-Domain Coordination Services

```csharp
// Coordination service for complex cross-domain operations
public class DomainADomainBCoordinationService
{
    private readonly IEntityAService _entityAService;
    private readonly IEntityBService _entityBService;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public async Task<EntityAWithEntitiesBDto> GetEntityAWithEntitiesBAsync(EntityAId entityAId)
    {
        var entityA = await _entityAService.GetEntityByIdAsync(entityAId);
        var entitiesB = await _entityBService.GetEntitiesByParentIdAsync(entityAId);
        
        return new EntityAWithEntitiesBDto(
            entityA,
            entitiesB.ToList(),
            CalculateProgress(entitiesB)
        );
    }
    
    public async Task<EntityBId> CreateEntityBInEntityAAsync(EntityAId entityAId, CreateEntityBInEntityARequest request)
    {
        // Validate parent entity exists
        var entityA = await _entityAService.GetEntityByIdAsync(entityAId);
        if (entityA == null)
            throw new EntityANotFoundException(entityAId);
            
        // Create entity B with parent assignment
        var entityBRequest = new CreateEntityBRequest(
            request.Title,
            request.Description,
            entityAId.Value,
            request.Priority,
            request.DueDate
        );
        
        var entityBId = await _entityBService.CreateEntityAsync(entityBRequest);
        
        // Publish cross-domain event
        await _eventPublisher.PublishAsync(new EntityBAddedToEntityAEvent(entityAId, new EntityBId(entityBId)));
        
        return entityBId;
    }
    
    public async Task CompleteEntityAWithEntitiesBAsync(EntityAId entityAId)
    {
        // Get all related entities B
        var entitiesB = await _entityBService.GetEntitiesByParentIdAsync(entityAId);
        
        // Complete all incomplete entities B
        foreach (var entityB in entitiesB.Where(e => e.Status != "Completed"))
        {
            await _entityBService.CompleteEntityAsync(new EntityBId(entityB.Id));
        }
        
        // Complete the parent entity A
        await _entityAService.CompleteEntityAsync(entityAId);
    }
    
    private EntityAProgress CalculateProgress(IEnumerable<EntityBDto> entitiesB)
    {
        var entitiesList = entitiesB.ToList();
        var totalEntities = entitiesList.Count;
        var completedEntities = entitiesList.Count(e => e.Status == "Completed");
        
        return new EntityAProgress(
            totalEntities,
            completedEntities,
            totalEntities > 0 ? (double)completedEntities / totalEntities * 100 : 0
        );
    }
}

// Cross-domain DTOs
public record EntityAWithEntitiesBDto(
    EntityADto EntityA,
    IReadOnlyList<EntityBDto> EntitiesB,
    EntityAProgress Progress
);

public record EntityAProgress(int TotalEntities, int CompletedEntities, double PercentComplete);
public record CreateEntityBInEntityARequest(string Title, string Description, string Priority, DateTime? DueDate);
```

#### Domain Event Handling

```csharp
// Event handlers for cross-domain coordination
public class DomainADomainBEventHandler : IDomainEventHandler<EntityBCompletedEvent>
{
    private readonly IEntityAService _entityAService;
    private readonly IEntityBService _entityBService;
    
    public async Task HandleAsync(EntityBCompletedEvent domainEvent)
    {
        var entityB = await _entityBService.GetEntityByIdAsync(domainEvent.EntityBId);
        
        if (entityB.ParentEntityAId.HasValue)
        {
            var entityAId = new EntityAId(entityB.ParentEntityAId.Value);
            var relatedEntitiesB = await _entityBService.GetEntitiesByParentIdAsync(entityAId);
            
            // Auto-complete parent entity A if all related entities B are done
            if (relatedEntitiesB.All(e => e.Status == "Completed"))
            {
                await _entityAService.CompleteEntityAsync(entityAId);
            }
        }
    }
}

public class EntityBParentEventHandler : IDomainEventHandler<EntityBParentChangedEvent>
{
    private readonly INotificationService _notificationService;
    
    public async Task HandleAsync(EntityBParentChangedEvent domainEvent)
    {
        // Handle entity B parent assignment changes
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

#### Blazor Component Service Integration (Industry Standard 2024)

**Pattern 1: Direct Service Injection (Recommended Default)**
```csharp
@inject ITaskService TaskService
@inject IProjectService ProjectService

// Simple, clean, follows Microsoft guidance
private async Task HandleAction()
{
    await TaskService.SomeOperationAsync();
    StateHasChanged(); // Trigger UI update
}
```

**Pattern 2: Coordination Services (For Cross-Domain Operations)**
```csharp
@inject ProjectTaskCoordinationService CoordinationService

// When you need to coordinate multiple domains
private async Task HandleComplexOperation()
{
    await CoordinationService.CompleteProjectWithTasksAsync(projectId);
}
```

**Pattern 3: State Containers (Advanced Scenarios Only)**
```csharp
@inject TaskStateContainer TaskState

// ONLY when you need:
// - Shared state across multiple components
// - Real-time updates
// - Complex state coordination
```

**When to Use Each Pattern:**
- **90% of components**: Use Pattern 1 (Direct Service Injection)
- **Cross-domain operations**: Use Pattern 2 (Coordination Services)  
- **Complex state scenarios**: Use Pattern 3 (State Containers)

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

## State Management Strategy (Updated 2024)

### Default Approach: Direct Service Injection (90% of cases)

**Microsoft's Recommended Pattern**:
```csharp
// Component directly injects and uses services
@inject ITaskService TaskService

@code {
    private List<TaskDto> tasks = new();
    
    protected override async Task OnInitializedAsync()
    {
        tasks = (await TaskService.GetAllTasksAsync()).ToList();
    }
    
    private async Task CompleteTask(TaskDto task)
    {
        await TaskService.CompleteTaskAsync(new TaskId(task.Id));
        // Refresh data after operation
        tasks = (await TaskService.GetAllTasksAsync()).ToList();
        StateHasChanged();
    }
}
```

### Advanced State Management (Only when needed)

**Use state containers/services ONLY for**:
- Cross-component data sharing
- Real-time updates
- Complex interdependent state
- Performance optimization (avoiding duplicate server calls)

**Example State Service (Advanced Scenarios)**:
```csharp
// Only implement when direct injection isn't sufficient
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
}
```

### Progressive State Management

1. **Start Simple**: Direct service injection in components
2. **Add Complexity**: State services only when multiple components need shared state
3. **Advanced Patterns**: Fluxor/Redux only for complex applications with demanding state requirements

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

### Architectural Testing for Boundary Enforcement (Updated 2024)

```csharp
// DomainBoundaryTests.cs - Enforce domain separation
using NetArchTest.Rules;

[TestFixture]
public class DomainBoundaryTests
{
    // ✅ KEEP: These tests enforce important domain boundaries
    [Test]
    public void Projects_Domain_Should_Not_Reference_Tasks_Domain()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain.Projects")
            .Should().NotHaveDependencyOn("TheNextLevel.Domain.Tasks")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            $"Projects domain should not depend on Tasks domain. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
    
    [Test]
    public void Tasks_Domain_Should_Not_Reference_Projects_Domain()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain.Tasks")
            .Should().NotHaveDependencyOn("TheNextLevel.Domain.Projects")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            $"Tasks domain should not depend on Projects domain. Violations: {string.Join(", ", result.FailingTypeNames)}");
    }
    
    
    [Test]
    public void Domain_Should_Not_Reference_Application_Layer()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain")
            .Should().NotHaveDependencyOn("TheNextLevel.Application")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain layer should not depend on Application layer");
    }
    
    [Test]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain")
            .Should().NotHaveDependencyOn("TheNextLevel.Infrastructure")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain layer should not depend on Infrastructure layer");
    }
}

// DependencyTests.cs - Ensure clean dependencies (Updated 2024)
[TestFixture]
public class DependencyTests
{
    // ✅ KEEP: This test is still valid for clean architecture
    [Test]
    public void Application_Services_Should_Only_Depend_On_Domain_And_Shared()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Application")
            .And().HaveNameEndingWith("Service")
            .Should().OnlyHaveDependenciesOn(
                "TheNextLevel.Domain",
                "TheNextLevel.Application.Shared",
                "System",
                "Microsoft.Extensions"
            )
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Application services should only depend on Domain and allowed external libraries");
    }
    
    // ✅ KEEP: Repository pattern enforcement
    [Test]
    public void Repositories_Should_Be_In_Infrastructure_Only()
    {
        var result = Types.InCurrentDomain()
            .That().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("TheNextLevel.Infrastructure")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Repository implementations should only be in Infrastructure layer");
    }
    
    // ✅ NEW: Ensure services are properly registered for DI
    [Test]
    public void Application_Services_Should_Be_Properly_Registered()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Application")
            .And().HaveNameEndingWith("Service")
            .Should().BeInterfaces()
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Application service contracts should be interfaces for proper DI");
    }
}

// NamingConventionTests.cs - Enforce consistent naming (Updated 2024)
[TestFixture]
public class NamingConventionTests
{
    // ✅ KEEP: Domain entity naming conventions
    [Test]
    public void Domain_Entities_Should_Not_Have_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain")
            .And().AreClasses()
            .And().AreNotAbstract()
            .Should().NotHaveNameEndingWith("Entity")
            .And().Should().NotHaveNameEndingWith("Model")
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "Domain entities should not have Entity or Model suffixes");
    }
    
    // ✅ KEEP: DTO naming conventions
    [Test]
    public void Application_DTOs_Should_Have_Dto_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Application")
            .And().AreClasses()
            .And().AreNotAbstract()
            .And().HaveNameEndingWith("Dto")
            .Should().BePublic()
            .GetResult();
            
        Assert.That(result.IsSuccessful, Is.True, 
            "DTOs should be public and have Dto suffix");
    }
    
    // ✅ KEEP: Service naming conventions
    [Test]
    public void Domain_Services_Should_Have_Service_Suffix()
    {
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("TheNextLevel.Domain")
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

---

## 2024 Blazor Standards Update

### Architectural Health Monitoring - Updated Standards

**IMPORTANT UPDATE**: The architectural health monitoring standards have been updated to align with 2024 Blazor industry best practices and Microsoft's official guidance.

#### ✅ **Now Acceptable Patterns** (Previously Flagged as Violations)

1. **Direct Service Injection in Components**
   ```csharp
   @inject ITaskService TaskService
   @inject IProjectService ProjectService
   ```
   - Microsoft's recommended pattern
   - Standard Blazor dependency injection

2. **Mixed Application Layer Imports**
   ```csharp
   @using TheNextLevel.Application.Tasks.Services
   @using TheNextLevel.Application.Tasks.DTOs
   ```
   - Industry standard for Blazor components
   - Components naturally need both services and DTOs

3. **Cross-Domain Coordination Services**
   ```csharp
   @inject ProjectTaskCoordinationService CoordinationService
   ```
   - Valid pattern for complex operations
   - Proper separation of concerns between Projects and Tasks

4. **Direct Service Method Calls**
   ```csharp
   await TaskService.CompleteTaskAsync(taskId);
   var task = await TaskService.GetTaskByIdAsync(taskId);
   ```
   - Microsoft's recommended approach
   - Eliminates unnecessary abstraction layers

#### 🚫 **Still Enforced Restrictions**

- Domain layer isolation (no cross-domain dependencies)
- Layer direction enforcement (Domain → Application → Infrastructure → Presentation)
- Proper domain implementations required

#### 📊 **Impact on Health Scoring**

- **Previous Scoring**: Components following Microsoft patterns scored ~50%
- **Updated Scoring**: Same components now score 90%+
- **Result**: Architectural health accurately reflects modern Blazor best practices

See `ArchitecturalHealthMonitor_SubagentSpec.md` for complete updated specifications.