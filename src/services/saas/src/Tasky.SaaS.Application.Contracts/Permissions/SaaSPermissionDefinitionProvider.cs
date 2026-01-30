using System;
using Tasky.SaaS.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Tasky.SaaS.Permissions;

public class SaaSPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var saasGroup = context.AddGroup(SaaSPermissions.GroupName, L("Permission:SaaS"));

        var tenantsPermission = saasGroup.AddPermission(SaaSPermissions.Tenants.Default, L("Permission:SaaS:Tenants"));
        tenantsPermission.AddChild(SaaSPermissions.Tenants.Create, L("Permission:SaaS:Tenants.Create"));
        tenantsPermission.AddChild(SaaSPermissions.Tenants.Update, L("Permission:SaaS:Tenants.Update"));
        tenantsPermission.AddChild(SaaSPermissions.Tenants.Delete, L("Permission:SaaS:Tenants.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SaaSResource>(name);
    }
}
