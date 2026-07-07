namespace Ucms.Domain.Constants;

/// <summary>
/// Tizim bo'yicha barcha statik ruxsat (permission) konstantalari.
/// ClaimType = "permission", ClaimValue = quyidagi qiymatlardan biri.
/// </summary>
public static class Permissions
{
    public static class Projects
    {
        public const string View   = "projects.view";
        public const string Create = "projects.create";
        public const string Edit   = "projects.edit";
        public const string Delete = "projects.delete";
    }

    public static class Brigades
    {
        public const string View   = "brigades.view";
        public const string Create = "brigades.create";
        public const string Edit   = "brigades.edit";
        public const string Delete = "brigades.delete";
    }

    public static class Warehouse
    {
        public const string View   = "warehouse.view";
        public const string Create = "warehouse.create";
        public const string Edit   = "warehouse.edit";
        public const string Delete = "warehouse.delete";
    }

    public static class Finance
    {
        public const string View   = "finance.view";
        public const string Create = "finance.create";
        public const string Edit   = "finance.edit";
        public const string Delete = "finance.delete";
        public const string Cancel = "finance.cancel";
    }

    public static class Personnel
    {
        public const string View   = "personnel.view";
        public const string Create = "personnel.create";
        public const string Edit   = "personnel.edit";
        public const string Delete = "personnel.delete";
    }

    public static class Refs
    {
        public const string View   = "refs.view";
        public const string Create = "refs.create";
        public const string Edit   = "refs.edit";
        public const string Delete = "refs.delete";
    }

    public static class Admin
    {
        public const string View = "admin.view";
    }

    /// <summary>
    /// Barcha permissionslar ro'yxati — RolesPage UI uchun ishlatiladi.
    /// </summary>
    public static IReadOnlyList<string> All =>
    [
        Projects.View,  Projects.Create,  Projects.Edit,  Projects.Delete,
        Brigades.View,  Brigades.Create,  Brigades.Edit,  Brigades.Delete,
        Warehouse.View, Warehouse.Create, Warehouse.Edit, Warehouse.Delete,
        Finance.View,   Finance.Create,   Finance.Edit,   Finance.Delete,  Finance.Cancel,
        Personnel.View, Personnel.Create, Personnel.Edit, Personnel.Delete,
        Refs.View,      Refs.Create,      Refs.Edit,      Refs.Delete,
        Admin.View,
    ];

    /// <summary>
    /// Permission modullarini guruhlash (UI checkbox uchun).
    /// </summary>
    public static IReadOnlyList<PermissionGroup> Groups =>
    [
        new("projects",  "Loyihalar",   [Projects.View,  Projects.Create,  Projects.Edit,  Projects.Delete]),
        new("brigades",  "Brigadalar",  [Brigades.View,  Brigades.Create,  Brigades.Edit,  Brigades.Delete]),
        new("warehouse", "Ombor",       [Warehouse.View, Warehouse.Create, Warehouse.Edit, Warehouse.Delete]),
        new("finance",   "Moliya",      [Finance.View,   Finance.Create,   Finance.Edit,   Finance.Delete,  Finance.Cancel]),
        new("personnel", "Xodimlar",    [Personnel.View, Personnel.Create, Personnel.Edit, Personnel.Delete]),
        new("refs",      "Ma'lumotnoma",[Refs.View,      Refs.Create,      Refs.Edit,      Refs.Delete]),
        new("admin",     "Admin",       [Admin.View]),
    ];

    public const string ClaimType = "permission";
}

public record PermissionGroup(string Key, string Label, IReadOnlyList<string> Permissions);
