using Microsoft.AspNetCore.Components;
using NextLevel5.Models;
using Task = NextLevel5.Models.Task;
using TaskStatus = NextLevel5.Models.TaskStatus;

namespace NextLevel5.Components.Pages;

public partial class Home : ComponentBase
{
    private List<Project> Projects = new();
    private Project selectedProject;

    protected override void OnInitialized()
    {
        // Initialize with sample data
        Projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Name = "Home Renovation",
                Color = "bg-blue-600",
                CompletionPercentage = 45,
                Tasks = new List<Task>
                {
                    new() { Id = 1, Title = "Paint living room", Status = TaskStatus.InProgress },
                    new() { Id = 2, Title = "Replace kitchen faucet", Status = TaskStatus.OnHold },
                    new() { Id = 3, Title = "Fix garage door", Status = TaskStatus.Todo },
                    new() { Id = 4, Title = "Install new lights", Status = TaskStatus.Completed }
                }
            },
            // Add more sample projects...
        };

        selectedProject = Projects.FirstOrDefault();
    }

    private string GetProjectCardClasses(Project project)
    {
        var baseClasses = "flex-shrink-0 w-48 h-32 rounded-lg p-4 text-left transition-all " +
                         $"{project.Color} text-white shadow-md hover:shadow-lg hover:brightness-110";

        if (selectedProject?.Id == project.Id)
        {
            baseClasses += " ring-2 ring-blue-400";
        }

        return baseClasses;
    }

    private RenderFragment GetStatusIcon(TaskStatus status) => builder =>
    {
        string iconClass = status switch
        {
            TaskStatus.Completed => "fas fa-check-circle text-green-500",
            TaskStatus.InProgress => "fas fa-clock text-blue-500",
            TaskStatus.OnHold => "fas fa-pause-circle text-yellow-500",
            _ => "far fa-circle text-gray-600"
        };

        builder.OpenElement(0, "i");
        builder.AddAttribute(1, "class", $"{iconClass} w-5 h-5");
        builder.CloseElement();
    };


    private void SelectProject(Project project)
    {
        selectedProject = project;
    }

    private void AddNewProject()
    {
        // Implement new project creation logic
    }

    private void AddNewTask()
    {
        // Implement new task creation logic
    }
}