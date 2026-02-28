
using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;

    public MissionService(IMissionRepository missionRepository)
    {
        _missionRepository = missionRepository;
    }

    public async System.Threading.Tasks.Task<MissionDto?> GetByIdAsync(int id)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
            return null;

        return new MissionDto(mission.Id, mission.Title, mission.Description, mission.IsCompleted);
    }

    public async System.Threading.Tasks.Task<Core.DTOs.PagedResult<MissionDto>> ListAsync(int skip, int take, string? filterText = null)
    {
        var pagedMissions = await _missionRepository.ListAsync(skip, take, filterText);
        var missionDtos = pagedMissions.Items.Select(m => new MissionDto(m.Id, m.Title, m.Description, m.IsCompleted)).ToList();

        return new Core.DTOs.PagedResult<MissionDto>
        {
            Items = missionDtos,
            TotalCount = pagedMissions.TotalCount
        };
    }

    public async System.Threading.Tasks.Task<MissionDto> CreateAsync(CreateMissionDto mission)
    {
        var addedMission = await _missionRepository.AddAsync(mission.Name, mission.Description);
        return new MissionDto(addedMission.Id, addedMission.Title, addedMission.Description, addedMission.IsCompleted);
    }

    public async System.Threading.Tasks.Task<MissionDto> UpdateAsync(UpdateMissionDto mission)
    {
        var updatedMission = await _missionRepository.UpdateAsync(mission.Id, mission.Name, mission.Description);
        return new MissionDto(updatedMission.Id, updatedMission.Title, updatedMission.Description, updatedMission.IsCompleted);
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var projects = await _missionRepository.ListProjectsAsync(id, 0, 1);
        var tasks = await _missionRepository.ListTasksAsync(id, 0, 1);

        if (projects.TotalCount > 0 || tasks.TotalCount > 0)
            return false; // Cannot delete mission with associated projects or tasks

        return await _missionRepository.DeleteAsync(id);
    }

    public async System.Threading.Tasks.Task<bool> CompleteAsync(int id)
    {
        return await _missionRepository.CompleteAsync(id);
    }

    public async System.Threading.Tasks.Task<bool> ResetAsync(int id)
    {
        return await _missionRepository.ResetAsync(id);
    }

    public async Task<PagedResult<ProjectDto>> ListProjectsAsync(int id, int skip, int take, string? filterText = null)
    {
        var pagedProjects = await _missionRepository.ListProjectsAsync(id, skip, take, filterText);
        var projectDtos = pagedProjects.Items.Select(p => new ProjectDto(p.Id, p.Name, p.Description ?? string.Empty)).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<PagedResult<EligibleProjectDto>> ListEligibleProjectsAsync(int id, int skip, int take, string? filterText = null)
    {
        var pagedProjects = await _missionRepository.ListEligibleProjectsAsync(id, skip, take, filterText);
        var dtos = pagedProjects.Items.Select(p => new EligibleProjectDto(p.Id, p.Name, p.Description ?? string.Empty, p.MissionTitle)).ToList();

        return new PagedResult<EligibleProjectDto>
        {
            Items = dtos,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<PagedResult<TaskDto>> ListTasksAsync(int id, int skip, int take, string? filterText = null)
    {
        var pagedTasks = await _missionRepository.ListTasksAsync(id, skip, take, filterText);
        var taskDtos = pagedTasks.Items.Select(t => new TaskDto(t.Id, t.Name, t.Description, t.IsCompleted)).ToList();

        return new PagedResult<TaskDto>
        {
            Items = taskDtos,
            TotalCount = pagedTasks.TotalCount
        };
    }

    public async Task MoveProjectAsync(int missionId, int projectId)
    {
        await _missionRepository.MoveProjectAsync(missionId, projectId);
    }
}