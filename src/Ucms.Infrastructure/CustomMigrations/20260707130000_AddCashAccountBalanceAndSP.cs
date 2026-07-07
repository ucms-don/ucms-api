namespace Ucms.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// 1. CashAccounts jadvaliga Balance ustuni qo'shadi (numeric(18,2), default 0).
/// 2. Mavjud ma'lumotlar uchun balansni CashTransactions.SUM orqali backfill qiladi.
/// 3. apply_cash_balance_delta() PostgreSQL funksiyasini yaratadi:
///    — SELECT ... FOR UPDATE orqali qatorni lock qiladi
///    — p_direction: 1 = In (kirim), 2 = Out (chiqim)
///    — p_allow_overdraft = false bo'lsa, manfiy balansda EXCEPTION tashlanadi
/// </summary>
public partial class AddCashAccountBalanceAndSP : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Balance ustuni
        migrationBuilder.AddColumn<decimal>(
            name: "Balance",
            table: "CashAccounts",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m);

        // 2. Backfill: mavjud transaksiyalardan hisoblash
        //    CashDirection: In = 1, Out = 2
        migrationBuilder.Sql(@"
            UPDATE ""CashAccounts"" ca
            SET ""Balance"" = COALESCE((
                SELECT SUM(
                    CASE WHEN t.""Direction"" = 1
                         THEN  t.""Amount""
                         ELSE -t.""Amount""
                    END)
                FROM ""CashTransactions"" t
                WHERE t.""CashAccountId"" = ca.""Id""
                  AND t.""IsDeleted"" = FALSE
            ), 0)
        ");

        // 3. PostgreSQL funksiyasi
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION apply_cash_balance_delta(
                p_account_id      UUID,
                p_amount          NUMERIC(18,2),
                p_direction       INTEGER,        -- 1 = In (kirim), 2 = Out (chiqim)
                p_allow_overdraft BOOLEAN DEFAULT FALSE
            )
            RETURNS NUMERIC(18,2)
            LANGUAGE plpgsql
            AS $$
            DECLARE
                v_current NUMERIC(18,2);
                v_delta   NUMERIC(18,2);
                v_new     NUMERIC(18,2);
            BEGIN
                IF p_amount <= 0 THEN
                    RAISE EXCEPTION 'p_amount musbat bo''lishi shart: %', p_amount
                        USING ERRCODE = 'P0003';
                END IF;

                IF p_direction NOT IN (1, 2) THEN
                    RAISE EXCEPTION 'p_direction faqat 1 (In) yoki 2 (Out) bo''lishi mumkin: %', p_direction
                        USING ERRCODE = 'P0004';
                END IF;

                v_delta := CASE WHEN p_direction = 1 THEN p_amount ELSE -p_amount END;

                SELECT ""Balance""
                INTO   v_current
                FROM   ""CashAccounts""
                WHERE  ""Id"" = p_account_id
                  AND  ""IsDeleted"" = FALSE
                FOR UPDATE;

                IF NOT FOUND THEN
                    RAISE EXCEPTION 'CashAccount topilmadi: %', p_account_id
                        USING ERRCODE = 'P0001';
                END IF;

                v_new := v_current + v_delta;

                IF p_direction = 2 AND NOT p_allow_overdraft AND v_new < 0 THEN
                    RAISE EXCEPTION 'Kassada mablag'' yetarli emas. Mavjud: %, Kerakli: %',
                        v_current, p_amount
                        USING ERRCODE = 'P0002';
                END IF;

                UPDATE ""CashAccounts""
                SET    ""Balance""   = v_new,
                       ""UpdatedAt"" = NOW()
                WHERE  ""Id"" = p_account_id;

                RETURN v_new;
            END;
            $$;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS apply_cash_balance_delta(UUID, NUMERIC, INTEGER, BOOLEAN)");

        migrationBuilder.DropColumn(
            name: "Balance",
            table: "CashAccounts");
    }
}
