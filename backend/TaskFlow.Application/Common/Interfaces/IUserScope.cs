namespace TaskFlow.Application.Common.Interfaces;

/// <summary>The identity and claims that decide which teams, projects and tasks a user can see.</summary>
public interface IUserScope
{
    Guid Id { get; }

    bool HasClaim(string claimId);
}
