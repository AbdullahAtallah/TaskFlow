using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Roles.Common;

public static class RoleRules
{
    private static readonly IReadOnlyDictionary<string, string> PackageByClaimId =
        ClaimTypes.All.ToDictionary(claim => claim.Id, claim => claim.PackageId);

    public static void AddRoleFieldRules<T>(this AbstractValidator<T> validator) where T : IRoleFieldsCommand
    {
        validator.RuleFor(command => command.Name).RequiredText("Name", FieldLengths.RoleName);

        validator.RuleFor(command => command.Description).OptionalText("Description", FieldLengths.RoleDescription);

        validator.RuleFor(command => command.Packages)
            .NotNull()
            .WithMessage("Packages are required.")
            .Must(packages => ValidationRules.BeDistinct(packages))
            .WithMessage("A package can only be granted once.");

        validator.RuleForEach(command => command.Packages)
            .Must(packageId => PackageTypes.All.Contains(packageId))
            .WithMessage((_, packageId) => $"'{packageId}' is not a package.");

        // Everyone who registers gets the role for new users, so it must never open user and role management.
        validator.RuleFor(command => command.Packages)
            .Must((command, packages) => !command.IsDefault || packages?.Contains(PackageTypes.Management) != true)
            .WithMessage(ErrorMessages.RoleForNewUsersCannotGrantManagement);

        validator.RuleFor(command => command.Claims)
            .NotNull()
            .WithMessage("Claims are required.")
            .Must(claims => ValidationRules.BeDistinct(claims))
            .WithMessage("A claim can only be granted once.");

        validator.RuleForEach(command => command.Claims)
            .Must(claimId => claimId is not null && PackageByClaimId.ContainsKey(claimId))
            .WithMessage((_, claimId) => $"'{claimId}' is not a claim.")
            .Must((command, claimId) => claimId is null
                                        || !PackageByClaimId.TryGetValue(claimId, out var packageId)
                                        || command.Packages?.Contains(packageId) == true)
            .WithMessage((_, claimId) => $"The claim '{claimId}' requires the {PackageByClaimId[claimId]} package.");
    }
}
