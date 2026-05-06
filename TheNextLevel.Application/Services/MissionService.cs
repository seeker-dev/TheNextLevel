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

    public async Task<MissionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, ct);
        if (mission == null)
            return null;

        return MissionDto.From(mission);
    }

    public async Task<PagedResult<MissionDto>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        var pagedMissions = await _missionRepository.ListAsync(skip, take, ct);
        var missionDtos = pagedMissions.Items.Select(MissionDto.From).ToList();

        return new PagedResult<MissionDto>
        {
            Items = missionDtos,
            TotalCount = pagedMissions.TotalCount
        };
    }

    public async Task<MissionDto> CreateAsync(CreateMissionDto mission, CancellationToken ct = default)
    {
        var addedMission = await _missionRepository.CreateAsync(mission.Name, mission.Description, ct);
        return MissionDto.From(addedMission);
    }

    public async Task<MissionDto> UpdateAsync(int missionId, UpdateMissionDto mission, CancellationToken ct = default)
    {
        var updatedMission = await _missionRepository.UpdateAsync(missionId, mission.Name, mission.Description, ct);
        return MissionDto.From(updatedMission);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var projects = await _projectRepository.ListByMissionIdAsync(id, 0, 1, ct);
        //var tasks = await _taskRepository.ListByMissionIdAsync(id, 0, 1, ct);

        if (projects.TotalCount > 0) //|| tasks.TotalCount > 0)
            return false; // Cannot delete mission with associated projects or tasks

        return await _missionRepository.DeleteAsync(id, ct);
    }

    public async Task<bool> CompleteAsync(int id, CancellationToken ct = default)
    {
        return await _missionRepository.CompleteAsync(id, ct);
    }

    public async Task<bool> ResetAsync(int id, CancellationToken ct = default)
    {
        return await _missionRepository.ResetAsync(id, ct);
    }
}
