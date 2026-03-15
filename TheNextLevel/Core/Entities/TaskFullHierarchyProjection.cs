namespace TheNextLevel.Core.Entities;

public record TaskFullHierarchyProjection(int Id, int AccountId, string Name, string? Description, int Status, int? ProjectId, string ProjectTitle, int? MissionId, string MissionTitle);