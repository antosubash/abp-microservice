using Tasky.IdentityService.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Tasky.IdentityService.Permissions;

public class IdentityServicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var identityGroup = context.AddGroup(IdentityServicePermissions.GroupName, L("Permission:IdentityService"));
        var userPermissions = identityGroup.AddPermission(
            IdentityServicePermissions.Users.Default,
            L("Permission:Users")
        );
        userPermissions.AddChild(IdentityServicePermissions.Users.Create, L("Permission:Create"));
        userPermissions.AddChild(IdentityServicePermissions.Users.Update, L("Permission:Update"));
        userPermissions.AddChild(IdentityServicePermissions.Users.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<IdentityServiceResource>(name);
    }
}
