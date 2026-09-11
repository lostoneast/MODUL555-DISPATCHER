namespace DispatcherApp.Domain.Enums;

public enum AssignmentReason
{
    Initial = 0,

    DispatcherTransfer = 10,
    UrgentNeed = 20,
    ConstructionReplanning = 30,
    Correction = 40,

    Other = 100
}