namespace InternalPortal.Domain.Enums;

public enum NotificationType
{
    EventCreated = 0,
    EventUpdated = 1,
    EventCancelled = 2,
    RegistrationConfirmed = 3,
    RegistrationCancelled = 4,
    Reminder = 5,
    Waitlisted = 6,
    PromotedFromWaitlist = 7
}
