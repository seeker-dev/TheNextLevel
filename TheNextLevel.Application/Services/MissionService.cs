using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public MissionService(IMissionRepository missionRepository, IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _missionRepository = missionRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    public async Task<MissionDto?> GetByIdAsync(int id)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
            return null;

        return MissionDto.From(mission);
    }

    public async Task<PagedResult<MissionDto>> ListAsync(int skip, int take)
    {
        var pagedMissions = await _missionRepository.ListAsync(skip, take);
        var missionDtos = pagedMissions.Items.Select(MissionDto.From).ToList();

        return new PagedResult<MissionDto>
        {
            Items = missionDtos,
            TotalCount = pagedMissions.TotalCount
        };
    }

    public async Task<MissionDto> CreateAsync(CreateMissionDto mission)
    {
        var addedMission = await _missionRepository.CreateAsync(mission.Name, mission.Description);
        return MissionDto.From(addedMission);
    }

    public async Task<MissionDto> UpdateAsync(int missionId, UpdateMissionDto mission)
    {
        var updatedMission = await _missionRepository.UpdateAsync(missionId, mission.Name, mission.Description);
        return MissionDto.From(updatedMission);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var projects = await _projectRepository.ListByMissionIdAsync(id, 0, 1);
        //var tasks = await _taskRepository.ListByMissionIdAsync(id, 0, 1);

        if (projects.TotalCount > 0) //|| tasks.TotalCount > 0)
            return false; // Cannot delete mission with associated projects or tasks

        return await _missionRepository.DeleteAsync(id);
    }

    public async Task<bool> CompleteAsync(int id)
    {
        return await _missionRepository.CompleteAsync(id);
    }

    public async Task<bool> ResetAsync(int id)
    {
        return await _missionRepository.ResetAsync(id);
    }
}