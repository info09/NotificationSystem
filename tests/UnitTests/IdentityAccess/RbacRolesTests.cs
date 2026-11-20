using IdentityAccess.Domain;

namespace UnitTests.IdentityAccess;

public sealed class RbacRolesTests
{
    [Fact]
    public void All_ContainsExpectedRoles()
    {
        Assert.Contains(RbacRoles.SystemAdministrator, RbacRoles.All);
        Assert.Contains(RbacRoles.BusinessOperator, RbacRoles.All);
        Assert.Contains(RbacRoles.IntegrationClient, RbacRoles.All);
        Assert.Equal(3, RbacRoles.All.Count);
    }

    [Fact]
    public void RoleConstants_HaveExpectedValues()
    {
        Assert.Equal("SystemAdministrator", RbacRoles.SystemAdministrator);
        Assert.Equal("BusinessOperator", RbacRoles.BusinessOperator);
        Assert.Equal("IntegrationClient", RbacRoles.IntegrationClient);
    }
}
