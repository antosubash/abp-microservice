using Volo.Abp.Reflection;

namespace Tasky.SaaS.Permissions;

public static class SaaSPermissions
{
    public const string GroupName = "SaaS";

    public static class Tenants
    {
        public const string Default = GroupName + ".Tenants";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(SaaSPermissions));
    }
}
