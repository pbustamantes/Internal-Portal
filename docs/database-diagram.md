# Database Diagram

```mermaid
erDiagram
    Users {
        Guid Id PK
        string Email UK "MaxLength(256)"
        string PasswordHash "MaxLength(512)"
        string FirstName "MaxLength(100)"
        string LastName "MaxLength(100)"
        string Department "nullable, MaxLength(100)"
        string ProfilePictureUrl "nullable, MaxLength(500)"
        UserRole Role "Employee | Organizer | Admin"
        bool IsActive "default: true"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    Events {
        Guid Id PK
        string Title "MaxLength(200)"
        string Description "nullable, MaxLength(4000)"
        DateTime StartUtc "owned: Schedule"
        DateTime EndUtc "owned: Schedule"
        int MinAttendees "owned: Capacity"
        int MaxAttendees "owned: Capacity"
        string LocationStreet "nullable, MaxLength(200)"
        string LocationCity "nullable, MaxLength(100)"
        string LocationState "nullable, MaxLength(50)"
        string LocationZipCode "nullable, MaxLength(20)"
        string LocationBuilding "nullable, MaxLength(100)"
        string LocationRoom "nullable, MaxLength(50)"
        EventStatus Status "Draft | Published | Cancelled | Completed"
        RecurrencePattern Recurrence "None | Daily | Weekly | Monthly"
        Guid OrganizerId FK "not null"
        Guid CategoryId FK "nullable"
        Guid VenueId FK "nullable"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    Registrations {
        Guid Id PK
        Guid UserId FK "not null"
        Guid EventId FK "not null"
        RegistrationStatus Status "Pending | Confirmed | Cancelled | Waitlisted"
        DateTime RegisteredAtUtc
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    EventCategories {
        Guid Id PK
        string Name UK "MaxLength(100)"
        string Description "nullable, MaxLength(500)"
        string ColorHex "MaxLength(9), default: #3B82F6"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    Venues {
        Guid Id PK
        string Name "MaxLength(200)"
        int Capacity
        string Street "MaxLength(200)"
        string City "MaxLength(100)"
        string State "MaxLength(50)"
        string ZipCode "MaxLength(20)"
        string Building "nullable, MaxLength(100)"
        string Room "nullable, MaxLength(50)"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    Notifications {
        Guid Id PK
        Guid UserId FK "not null"
        Guid EventId FK "nullable"
        string Title "MaxLength(200)"
        string Message "MaxLength(2000)"
        NotificationType Type "EventCreated | EventUpdated | etc."
        bool IsRead "default: false"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    RefreshTokens {
        Guid Id PK
        string Token UK "MaxLength(512)"
        Guid UserId FK "not null"
        DateTime ExpiresUtc
        string CreatedByIp "nullable, MaxLength(50)"
        DateTime RevokedAtUtc "nullable"
        string RevokedByIp "nullable, MaxLength(50)"
        string ReplacedByToken "nullable, MaxLength(512)"
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc "nullable"
    }

    Users ||--o{ Events : "organizes (Restrict)"
    Users ||--o{ Registrations : "registers (Cascade)"
    Users ||--o{ Notifications : "receives (Cascade)"
    Users ||--o{ RefreshTokens : "has (Cascade)"
    Events ||--o{ Registrations : "has (Cascade)"
    Events ||--o{ Notifications : "references (SetNull)"
    EventCategories ||--o{ Events : "categorizes (SetNull)"
    Venues ||--o{ Events : "hosts (SetNull)"
```

## Relationship Diagram

```mermaid
flowchart TB
    subgraph Core["Core Tables"]
        Users["<b>Users</b><br/><i>Id (PK), Email (UK), Role</i>"]
        Events["<b>Events</b><br/><i>Id (PK), Title, Status</i>"]
    end

    subgraph Junction["Junction Table"]
        Registrations["<b>Registrations</b><br/><i>Id (PK), Status</i><br/><i>UK: (UserId, EventId)</i>"]
    end

    subgraph Supporting["Supporting Tables"]
        EventCategories["<b>EventCategories</b><br/><i>Id (PK), Name (UK)</i>"]
        Venues["<b>Venues</b><br/><i>Id (PK), Name, Capacity</i>"]
        Notifications["<b>Notifications</b><br/><i>Id (PK), Title, Type, IsRead</i>"]
        RefreshTokens["<b>RefreshTokens</b><br/><i>Id (PK), Token (UK), ExpiresUtc</i>"]
    end

    Users -- "OrganizerId (1:N)<br/>OnDelete: Restrict" --> Events
    Users -- "UserId (1:N)<br/>OnDelete: Cascade" --> Registrations
    Users -- "UserId (1:N)<br/>OnDelete: Cascade" --> Notifications
    Users -- "UserId (1:N)<br/>OnDelete: Cascade" --> RefreshTokens
    Events -- "EventId (1:N)<br/>OnDelete: Cascade" --> Registrations
    Events -. "EventId (1:N)<br/>OnDelete: SetNull" .-> Notifications
    EventCategories -. "CategoryId (1:N)<br/>OnDelete: SetNull" .-> Events
    Venues -. "VenueId (1:N)<br/>OnDelete: SetNull" .-> Events

    style Core fill:#dbeafe,stroke:#3b82f6,stroke-width:2px
    style Junction fill:#fef3c7,stroke:#f59e0b,stroke-width:2px
    style Supporting fill:#f3e8ff,stroke:#a855f7,stroke-width:2px
    style Users fill:#bfdbfe,stroke:#2563eb
    style Events fill:#bfdbfe,stroke:#2563eb
    style Registrations fill:#fde68a,stroke:#d97706
    style EventCategories fill:#e9d5ff,stroke:#9333ea
    style Venues fill:#e9d5ff,stroke:#9333ea
    style Notifications fill:#e9d5ff,stroke:#9333ea
    style RefreshTokens fill:#e9d5ff,stroke:#9333ea
```

> **Solid arrows** (`-->`) represent required FK relationships with `Cascade` or `Restrict` delete.
> **Dashed arrows** (`-.->`) represent optional/nullable FK relationships with `SetNull` delete.
> Arrow direction points from the parent (one) to the child (many).

## Relationships

| From | To | Type | FK Column | On Delete |
|---|---|---|---|---|
| Users | Events | One-to-Many | Events.OrganizerId | Restrict |
| Users | Registrations | One-to-Many | Registrations.UserId | Cascade |
| Users | Notifications | One-to-Many | Notifications.UserId | Cascade |
| Users | RefreshTokens | One-to-Many | RefreshTokens.UserId | Cascade |
| Events | Registrations | One-to-Many | Registrations.EventId | Cascade |
| Events | Notifications | One-to-Many | Notifications.EventId | SetNull |
| EventCategories | Events | One-to-Many | Events.CategoryId | SetNull |
| Venues | Events | One-to-Many | Events.VenueId | SetNull |

## Unique Constraints

| Table | Column(s) |
|---|---|
| Users | Email |
| EventCategories | Name |
| RefreshTokens | Token |
| Registrations | (UserId, EventId) composite |

## Owned Value Objects

| Owner | Property | Value Object | Embedded Columns |
|---|---|---|---|
| Events | Schedule | DateTimeRange | StartUtc, EndUtc |
| Events | Capacity | Capacity | MinAttendees, MaxAttendees |
| Events | Location | Address | LocationStreet, LocationCity, LocationState, LocationZipCode, LocationBuilding, LocationRoom |
| Venues | Address | Address | Street, City, State, ZipCode, Building, Room |

## Enums (stored as strings, MaxLength 20-30)

| Enum | Values |
|---|---|
| UserRole | Employee (0), Organizer (1), Admin (2) |
| EventStatus | Draft (0), Published (1), Cancelled (2), Completed (3) |
| RegistrationStatus | Pending (0), Confirmed (1), Cancelled (2), Waitlisted (3) |
| RecurrencePattern | None (0), Daily (1), Weekly (2), Monthly (3) |
| NotificationType | EventCreated (0), EventUpdated (1), EventCancelled (2), RegistrationConfirmed (3), RegistrationCancelled (4), Reminder (5), Waitlisted (6), PromotedFromWaitlist (7) |
