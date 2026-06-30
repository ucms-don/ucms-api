namespace Ucms.Domain.Enums;

public enum ActStatus
{
    Draft         = 1,
    Issued        = 2,
    SignedState   = 3, // переименовано, обновите места использования
    PaidPartially = 4,
    PaidFully     = 5,
}
