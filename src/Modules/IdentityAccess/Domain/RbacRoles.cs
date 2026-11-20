namespace IdentityAccess.Domain;

public static class RbacRoles
{
    public const string SystemAdministrator = "SystemAdministrator";
    public const string BusinessOperator = "BusinessOperator";
    public const string IntegrationClient = "IntegrationClient";

    public static readonly IReadOnlyCollection<string> All =
    [
        SystemAdministrator,
        BusinessOperator,
        IntegrationClient
    ];
}
