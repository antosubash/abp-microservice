using Tasky.Administration.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Tasky.Administration.Permissions;

public class AdministrationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var administrationGroup = context.AddGroup(AdministrationPermissions.GroupName, L("Permission:Administration"));
        var settingsPermissions = administrationGroup.AddPermission(
            AdministrationPermissions.Settings.Default,
            L("Permission:Administration:Settings")
        );
        settingsPermissions.AddChild(
            AdministrationPermissions.Settings.Update,
            L("Permission:Administration:Settings.Update")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AdministrationResource>(name);
    }
}
