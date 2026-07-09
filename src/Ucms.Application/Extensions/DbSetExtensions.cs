namespace Ucms.Application.Extensions;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Ucms.Domain.Common;
using Ucms.Domain.Entities;

/// <summary>
/// EF Core DbSet uchun tashkilot ierarxiyasiga asoslangan so'rov kengaytmalari.
/// Recursive CTE orqali joriy tashkilot va uning barcha avlod tashkilotlaridagi
/// entitylarni filtrlaydi.
/// </summary>
public static class DbSetExtensions
{
    // ── Organization-specific ───────────────────────────────────────────────

    /// <summary>
    /// Berilgan tashkilotdan boshlab ota-tashkilotlar zanjirini rekursiv oladi.
    /// </summary>
    public static IQueryable<Organization> IncludeParents(
        this DbSet<Organization> query, Guid tenantId)
    {
        return query.FromSql($"""
            WITH RECURSIVE cte_organizations AS (
                SELECT *
                FROM "Organizations"
                WHERE "Id" = {tenantId}

                UNION ALL

                SELECT c.*
                FROM "Organizations" c
                INNER JOIN cte_organizations r
                ON r."ParentId" = c."Id" AND r."IsDeleted" = false
            )
            SELECT *
            FROM cte_organizations
            """);
    }

    /// <summary>
    /// Berilgan tashkilot va uning barcha avlod tashkilotlarini rekursiv oladi.
    /// </summary>
    public static IQueryable<Organization> IncludeChilds(
        this DbSet<Organization> query, Guid tenantId)
    {
        return query.FromSql($"""
            WITH RECURSIVE cte_organizations AS (
                SELECT *
                FROM "Organizations"
                WHERE "Id" = {tenantId} AND "IsDeleted" = false

                UNION ALL

                SELECT c.*
                FROM "Organizations" c
                INNER JOIN cte_organizations r
                ON r."Id" = c."ParentId" AND r."IsDeleted" = false
            )
            SELECT *
            FROM cte_organizations
            """);
    }

    /// <summary>
    /// Berilgan tashkilot ID ro'yxati uchun barcha avlodlarni rekursiv oladi.
    /// </summary>
    public static IQueryable<Organization> IncludeChilds(
        this DbSet<Organization> query, List<Guid> tenantIds)
    {
        var param = new NpgsqlParameter("tenantIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
        {
            Value = tenantIds.ToArray()
        };
        return query.FromSqlRaw("""
            WITH RECURSIVE cte_organizations AS (
                SELECT *
                FROM "Organizations"
                WHERE "Id" = ANY(@tenantIds) AND "IsDeleted" = false

                UNION ALL

                SELECT c.*
                FROM "Organizations" c
                INNER JOIN cte_organizations r
                ON r."Id" = c."ParentId" AND c."IsDeleted" = false
            )
            SELECT *
            FROM cte_organizations
            """, param);
    }

    // ── Generic IHasOrganization ────────────────────────────────────────────

    /// <summary>
    /// Berilgan tashkilot va uning barcha avlodlariga tegishli entitylarni qaytaradi.
    /// </summary>
    public static IQueryable<TEntity> IncludeChilds<TEntity>(
        this DbSet<TEntity> query, Guid tenantId)
        where TEntity : class, IHasOrganization
    {
        var tableName = query.EntityType.GetTableName()
            ?? typeof(TEntity).Name + "s";

        var param = new NpgsqlParameter("tenantId", NpgsqlDbType.Uuid) { Value = tenantId };

        return query.FromSqlRaw($"""
            WITH RECURSIVE cte_organizations AS (
                SELECT "Id", "ParentId", "IsDeleted"
                FROM "Organizations"
                WHERE "Id" = @tenantId AND "IsDeleted" = false

                UNION ALL

                SELECT c."Id", c."ParentId", c."IsDeleted"
                FROM "Organizations" c
                INNER JOIN cte_organizations r
                ON r."Id" = c."ParentId" AND r."IsDeleted" = false
            )
            SELECT e.*
            FROM "{tableName}" e
            INNER JOIN cte_organizations o
            ON o."Id" = e."OrganizationId"
            """, param);
    }

    /// <summary>
    /// Berilgan tashkilotlar ro'yxati va ularning avlodlariga tegishli entitylarni qaytaradi.
    /// </summary>
    public static IQueryable<TEntity> IncludeChilds<TEntity>(
        this DbSet<TEntity> query, List<Guid> tenantIds)
        where TEntity : class, IHasOrganization
    {
        var tableName = query.EntityType.GetTableName()
            ?? typeof(TEntity).Name + "s";

        var param = new NpgsqlParameter("tenantIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
        {
            Value = tenantIds.ToArray()
        };

        return query.FromSqlRaw($"""
            WITH RECURSIVE cte_organizations AS (
                SELECT "Id", "ParentId", "IsDeleted"
                FROM "Organizations"
                WHERE "Id" = ANY(@tenantIds) AND "IsDeleted" = false

                UNION ALL

                SELECT c."Id", c."ParentId", c."IsDeleted"
                FROM "Organizations" c
                INNER JOIN cte_organizations r
                ON r."Id" = c."ParentId" AND r."IsDeleted" = false
            )
            SELECT e.*
            FROM "{tableName}" e
            INNER JOIN cte_organizations o
            ON o."Id" = e."OrganizationId"
            """, param);
    }

    /// <summary>
    /// Kontekst bo'yicha smart filtrlash:
    /// - organizationId berilmagan: butun tenantId daraxtini qaytaradi
    /// - organizationId == tenantId: faqat o'sha tashkilotni qaytaradi (tez yo'l)
    /// - boshqa: tenantId daraxtidan organizationId bo'yicha filtrlaydi
    /// </summary>
    public static IQueryable<TEntity> IncludeChilds<TEntity>(
        this DbSet<TEntity> query, Guid tenantId, Guid? organizationId)
        where TEntity : class, IHasOrganization
    {
        if (organizationId.HasValue)
        {
            return organizationId.Value == tenantId
                ? query.Where(e => e.OrganizationId == organizationId.Value)
                : query.IncludeChilds(tenantId).Where(e => e.OrganizationId == organizationId.Value);
        }

        return query.IncludeChilds(tenantId);
    }
}
