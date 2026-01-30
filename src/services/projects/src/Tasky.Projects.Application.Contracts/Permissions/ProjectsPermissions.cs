using Volo.Abp.Reflection;

namespace Tasky.Projects.Permissions;

public static class ProjectsPermissions
{
    public const string GroupName = "Projects";

    public static class Issues
    {
        public const string Default = GroupName + ".Issues";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ProjectsPermissions));
    }
}
