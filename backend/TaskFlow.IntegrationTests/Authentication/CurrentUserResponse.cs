using TaskFlow.Application.Common.DTOs;

namespace TaskFlow.IntegrationTests.Authentication;

/// <summary>The JSON of GET /api/user/current.</summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string UserName,
    string DisplayName,
    List<EntityReferenceDto> Roles,
    List<string> Packages,
    List<string> Features,
    List<string> Claims);
