using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Shared;
using TaskFlow.Application.Commands.Teams.AddTeamMembers;
using TaskFlow.Application.Commands.Teams.CreateTeam;
using TaskFlow.Application.Commands.Teams.DeleteTeam;
using TaskFlow.Application.Commands.Teams.RemoveTeamMember;
using TaskFlow.Application.Commands.Teams.UpdateTeam;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Middleware.Attributes;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Controllers.Teams;

[Route("api/team")]
[ApiController]
[Authorization(PackageTypes.Teams)]
public class TeamController(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever)
    : CommandControllerBase(messageBus, sendNotificationCommandHandler, currentUserRetriever)
{
    [HttpPost]
    [Claim(ClaimTypes.TeamsManage)]
    public Task<AcceptedResult> Post([FromBody] CreateTeamCommand command)
    {
        return PublishAsync(command with { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Team);
    }

    [HttpPut("{id:guid}")]
    [Claim(ClaimTypes.TeamsManage)]
    public Task<AcceptedResult> Put(Guid id, [FromBody] UpdateTeamCommand command)
    {
        return PublishAsync(command with { Id = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Team);
    }

    /// <summary>Deletes a team that owns no active projects; its memberships are removed.</summary>
    [HttpDelete("{id:guid}")]
    [Claim(ClaimTypes.TeamsManage)]
    public Task<AcceptedResult> Delete(Guid id)
    {
        return PublishAsync(new DeleteTeamCommand(id) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.Team);
    }

    /// <summary>Adds members (body: userIds); users who already are members are skipped.</summary>
    [HttpPost("{id:guid}/member")]
    [Claim(ClaimTypes.TeamsMembersManage)]
    public Task<AcceptedResult> PostMembers(Guid id, [FromBody] AddTeamMembersCommand command)
    {
        return PublishAsync(command with { TeamId = id, UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.TeamMember);
    }

    /// <summary>Removes a member; their tasks in the team's projects become unassigned.</summary>
    [HttpDelete("{id:guid}/member/{userId:guid}")]
    [Claim(ClaimTypes.TeamsMembersManage)]
    public Task<AcceptedResult> DeleteMember(Guid id, Guid userId)
    {
        return PublishAsync(new RemoveTeamMemberCommand(id, userId) { UserCommandContext = UserContext, CorrelationId = Guid.NewGuid() }, EntityType.TeamMember);
    }
}
