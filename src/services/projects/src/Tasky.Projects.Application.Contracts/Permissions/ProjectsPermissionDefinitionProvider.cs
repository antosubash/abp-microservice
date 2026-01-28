using System;
using Tasky.Projects.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Tasky.Projects.Permissions;

public class ProjectsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var projectsGroup = context.AddGroup(ProjectsPermissions.GroupName, L("Permission:Projects"));
        var projectsPermissions = projectsGroup.AddPermission(
            ProjectsPermissions.Issues.Default,
            L("Permission:Projects:Issues")
        );
        projectsPermissions.AddChild(ProjectsPermissions.Issues.Create, L("Permission:Projects:Issues:Create"));
        projectsPermissions.AddChild(ProjectsPermissions.Issues.Update, L("Permission:Projects:Issues:Update"));
        projectsPermissions.AddChild(ProjectsPermissions.Issues.Delete, L("Permission:Projects:Issues:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectsResource>(name);
    }
}
