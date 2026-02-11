# Class Diagrams

## 1. Domain Layer — Entities & Value Objects

```mermaid
classDiagram
    direction TB

    class BaseEntity {
        +Guid Id
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        +IReadOnlyCollection~BaseDomainEvent~ DomainEvents
        +AddDomainEvent(BaseDomainEvent) void
        +RemoveDomainEvent(BaseDomainEvent) void
        +ClearDomainEvents() void
    }

    class User {
        +string Email
        +string PasswordHash
        +string FirstName
        +string LastName
        +string? Department
        +string? ProfilePictureUrl
        +UserRole Role
        +bool IsActive
        +string FullName
        +IReadOnlyCollection~Registration~ Registrations
        +IReadOnlyCollection~RefreshToken~ RefreshTokens
        +IReadOnlyCollection~Notification~ Notifications
        +IReadOnlyCollection~Event~ OrganizedEvents
        +Create(email, passwordHash, firstName, lastName, department?, role?)$ User
    }

    class Event {
        +string Title
        +string? Description
        +DateTimeRange Schedule
        +Capacity Capacity
        +Address? Location
        +EventStatus Status
        +RecurrencePattern Recurrence
        +Guid OrganizerId
        +User Organizer
        +Guid? CategoryId
        +EventCategory? Category
        +Guid? VenueId
        +Venue? Venue
        +IReadOnlyCollection~Registration~ Registrations
        +bool IsInPast
        +Complete() void
        +EnsureModifiable() void
        +Register(Guid userId) Registration
        +Publish() void
        +Cancel() void
    }

    class Registration {
        +Guid UserId
        +User User
        +Guid EventId
        +Event Event
        +RegistrationStatus Status
        +DateTime RegisteredAtUtc
        +Confirm() void
        +Cancel() void
    }

    class Notification {
        +Guid UserId
        +User User
        +Guid? EventId
        +Event? Event
        +string Title
        +string Message
        +NotificationType Type
        +bool IsRead
        +MarkAsRead() void
    }

    class RefreshToken {
        +string Token
        +Guid UserId
        +User User
        +DateTime ExpiresUtc
        +string? CreatedByIp
        +DateTime? RevokedAtUtc
        +string? RevokedByIp
        +string? ReplacedByToken
        +bool IsExpired
        +bool IsRevoked
        +bool IsActive
    }

    class EventCategory {
        +string Name
        +string? Description
        +string ColorHex
        +IReadOnlyCollection~Event~ Events
    }

    class Venue {
        +string Name
        +Address Address
        +int Capacity
        +IReadOnlyCollection~Event~ Events
    }

    class DateTimeRange {
        <<value object>>
        +DateTime StartUtc
        +DateTime EndUtc
        +TimeSpan Duration
        +Overlaps(DateTimeRange other) bool
    }

    class Capacity {
        <<value object>>
        +int MinAttendees
        +int MaxAttendees
        +IsFull(int currentCount) bool
        +RemainingSpots(int currentCount) int
    }

    class Address {
        <<value object>>
        +string Street
        +string City
        +string State
        +string ZipCode
        +string? Building
        +string? Room
    }

    BaseEntity <|-- User
    BaseEntity <|-- Event
    BaseEntity <|-- Registration
    BaseEntity <|-- Notification
    BaseEntity <|-- RefreshToken
    BaseEntity <|-- EventCategory
    BaseEntity <|-- Venue

    Event *-- DateTimeRange : Schedule
    Event *-- Capacity : Capacity
    Event *-- Address : Location
    Venue *-- Address : Address

    User "1" --> "*" Event : organizes
    User "1" --> "*" Registration : registers
    User "1" --> "*" Notification : receives
    User "1" --> "*" RefreshToken : has
    Event "1" --> "*" Registration : has
    Event "1" --> "*" Notification : references
    EventCategory "1" --> "*" Event : categorizes
    Venue "1" --> "*" Event : hosts
```

## 2. Domain Layer — Enums, Events & Exceptions

```mermaid
classDiagram
    direction TB

    class EventStatus {
        <<enumeration>>
        Draft = 0
        Published = 1
        Cancelled = 2
        Completed = 3
    }

    class UserRole {
        <<enumeration>>
        Employee = 0
        Organizer = 1
        Admin = 2
    }

    class RegistrationStatus {
        <<enumeration>>
        Pending = 0
        Confirmed = 1
        Cancelled = 2
        Waitlisted = 3
    }

    class NotificationType {
        <<enumeration>>
        EventCreated = 0
        EventUpdated = 1
        EventCancelled = 2
        RegistrationConfirmed = 3
        RegistrationCancelled = 4
        Reminder = 5
        Waitlisted = 6
        PromotedFromWaitlist = 7
    }

    class RecurrencePattern {
        <<enumeration>>
        None = 0
        Daily = 1
        Weekly = 2
        Monthly = 3
    }

    class BaseDomainEvent {
        <<abstract>>
        +DateTime OccurredOnUtc
    }
    class INotification {
        <<interface>>
    }

    class EventCreatedDomainEvent {
        +Guid EventId
        +string Title
    }
    class EventCancelledDomainEvent {
        +Guid EventId
        +string Title
    }
    class UserRegisteredDomainEvent {
        +Guid UserId
        +Guid EventId
        +RegistrationStatus Status
    }
    class RegistrationConfirmedDomainEvent {
        +Guid UserId
        +Guid EventId
    }
    class RegistrationCancelledDomainEvent {
        +Guid UserId
        +Guid EventId
    }

    INotification <|.. BaseDomainEvent
    BaseDomainEvent <|-- EventCreatedDomainEvent
    BaseDomainEvent <|-- EventCancelledDomainEvent
    BaseDomainEvent <|-- UserRegisteredDomainEvent
    BaseDomainEvent <|-- RegistrationConfirmedDomainEvent
    BaseDomainEvent <|-- RegistrationCancelledDomainEvent

    class DomainException {
        +DomainException(string message)
        +DomainException(string message, Exception inner)
    }
    class NotFoundException {
        +NotFoundException(string name, object key)
    }
    class ForbiddenException {
        +ForbiddenException(string message?)
    }
    class ValidationException {
        +IDictionary~string, string[]~ Errors
        +ValidationException()
        +ValidationException(IEnumerable~ValidationFailure~ failures)
    }

    Exception <|-- DomainException
    Exception <|-- NotFoundException
    Exception <|-- ForbiddenException
    Exception <|-- ValidationException
```

## 3. Domain Layer — Repository Interfaces

```mermaid
classDiagram
    direction TB

    class IRepository~T~ {
        <<interface>>
        +GetByIdAsync(Guid id, CancellationToken) Task~T?~
        +GetAllAsync(CancellationToken) Task~IReadOnlyList~T~~
        +AddAsync(T entity, CancellationToken) Task~T~
        +UpdateAsync(T entity, CancellationToken) Task
        +DeleteAsync(T entity, CancellationToken) Task
    }

    class IUserRepository {
        <<interface>>
        +GetByEmailAsync(string email, CancellationToken) Task~User?~
        +EmailExistsAsync(string email, CancellationToken) Task~bool~
    }

    class IEventRepository {
        <<interface>>
        +GetPagedAsync(int page, int pageSize, string? search, Guid? categoryId, CancellationToken) Task~(IReadOnlyList~Event~, int)~
        +GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken) Task~IReadOnlyList~Event~~
        +GetUpcomingAsync(int count, CancellationToken) Task~IReadOnlyList~Event~~
        +GetByIdWithDetailsAsync(Guid id, CancellationToken) Task~Event?~
    }

    class IRegistrationRepository {
        <<interface>>
        +GetByEventIdAsync(Guid eventId, CancellationToken) Task~IReadOnlyList~Registration~~
        +GetByUserIdAsync(Guid userId, CancellationToken) Task~IReadOnlyList~Registration~~
        +GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken) Task~Registration?~
    }

    class INotificationRepository {
        <<interface>>
        +GetByUserIdAsync(Guid userId, bool unreadOnly, CancellationToken) Task~IReadOnlyList~Notification~~
        +GetUnreadCountAsync(Guid userId, CancellationToken) Task~int~
    }

    class IRefreshTokenRepository {
        <<interface>>
        +GetByTokenAsync(string token, CancellationToken) Task~RefreshToken?~
        +RevokeAllForUserAsync(Guid userId, CancellationToken) Task
    }

    class IUnitOfWork {
        <<interface>>
        +SaveChangesAsync(CancellationToken) Task~int~
        +Dispose() void
    }

    IRepository~T~ <|-- IUserRepository
    IRepository~T~ <|-- IEventRepository
    IRepository~T~ <|-- IRegistrationRepository
    IRepository~T~ <|-- INotificationRepository
    IRepository~T~ <|-- IRefreshTokenRepository
```

## 4. Application Layer — Service Interfaces & Models

```mermaid
classDiagram
    direction TB

    class IApplicationDbContext {
        <<interface>>
        +DbSet~Event~ Events
        +DbSet~User~ Users
        +DbSet~Registration~ Registrations
        +DbSet~Notification~ Notifications
        +DbSet~RefreshToken~ RefreshTokens
        +DbSet~EventCategory~ EventCategories
        +DbSet~Venue~ Venues
        +SaveChangesAsync(CancellationToken) Task~int~
    }

    class ICurrentUserService {
        <<interface>>
        +Guid? UserId
        +string? Email
        +string? Role
    }

    class IJwtService {
        <<interface>>
        +GenerateAccessToken(User user) string
        +GenerateRefreshToken() string
        +ValidateToken(string token) bool
    }

    class IIdentityService {
        <<interface>>
        +HashPassword(string password) string
        +VerifyPassword(string password, string hash) bool
    }

    class IEmailService {
        <<interface>>
        +SendEmailAsync(string to, string subject, string body, CancellationToken) Task
    }

    class INotificationService {
        <<interface>>
        +SendNotificationAsync(Guid userId, string title, string message, CancellationToken) Task
        +SendToAllAsync(string title, string message, CancellationToken) Task
    }

    class PaginatedList~T~ {
        +IReadOnlyList~T~ Items
        +int PageNumber
        +int TotalPages
        +int TotalCount
        +bool HasPreviousPage
        +bool HasNextPage
    }

    class Result~T~ {
        +bool IsSuccess
        +T? Value
        +string? Error
        +Success(T value)$ Result~T~
        +Failure(string error)$ Result~T~
    }
```

## 5. Application Layer — DTOs

```mermaid
classDiagram
    direction TB

    class AuthResponse {
        <<record>>
        +string AccessToken
        +string RefreshToken
        +DateTime ExpiresAt
        +UserDto User
    }

    class UserDto {
        <<record>>
        +Guid Id
        +string Email
        +string FirstName
        +string LastName
        +string? Department
        +string Role
        +string? ProfilePictureUrl
    }

    class EventDto {
        <<record>>
        +Guid Id
        +string Title
        +string? Description
        +DateTime StartUtc
        +DateTime EndUtc
        +int MinAttendees
        +int MaxAttendees
        +int CurrentAttendees
        +string Status
        +string Recurrence
        +string? LocationStreet
        +string? LocationCity
        +string? LocationState
        +string? LocationZipCode
        +string? LocationBuilding
        +string? LocationRoom
        +Guid OrganizerId
        +string OrganizerName
        +Guid? CategoryId
        +string? CategoryName
        +string? CategoryColor
        +Guid? VenueId
        +string? VenueName
        +DateTime CreatedAtUtc
    }

    class EventSummaryDto {
        <<record>>
        +Guid Id
        +string Title
        +DateTime StartUtc
        +DateTime EndUtc
        +int MaxAttendees
        +int CurrentAttendees
        +string Status
        +string? CategoryName
        +string? CategoryColor
        +string OrganizerName
    }

    class AttendeeDto {
        <<record>>
        +Guid UserId
        +string FullName
        +string Email
        +string? Department
        +string RegistrationStatus
        +DateTime RegisteredAtUtc
        +string? ProfilePictureUrl
    }

    class RegistrationDto {
        <<record>>
        +Guid Id
        +Guid UserId
        +string UserName
        +Guid EventId
        +string EventTitle
        +string Status
        +DateTime RegisteredAtUtc
    }

    class NotificationDto {
        <<record>>
        +Guid Id
        +string Title
        +string Message
        +string Type
        +bool IsRead
        +Guid? EventId
        +DateTime CreatedAtUtc
    }

    class VenueDto {
        <<record>>
        +Guid Id
        +string Name
        +int Capacity
        +string Street
        +string City
        +string State
        +string ZipCode
        +string? Building
        +string? Room
    }

    class EventAttendanceReportDto {
        <<record>>
        +Guid EventId
        +string EventTitle
        +DateTime StartUtc
        +int TotalRegistrations
        +int ConfirmedCount
        +int CancelledCount
        +int WaitlistedCount
        +double AttendanceRate
    }

    class MonthlyEventsReportDto {
        <<record>>
        +int Year
        +int Month
        +int TotalEvents
        +int TotalRegistrations
        +int AverageAttendees
    }

    class PopularEventDto {
        <<record>>
        +Guid EventId
        +string Title
        +string? CategoryName
        +int RegistrationCount
        +int MaxAttendees
        +double FillRate
    }

    AuthResponse --> UserDto
```

## 6. Application Layer — CQRS: Auth Feature

```mermaid
classDiagram
    direction LR

    class LoginCommand {
        <<record>>
        +string Email
        +string Password
    }
    class LoginCommandValidator {
        +LoginCommandValidator()
    }
    class LoginCommandHandler {
        -IUserRepository _userRepository
        -IIdentityService _identityService
        -IJwtService _jwtService
        -IRefreshTokenRepository _refreshTokenRepository
        -IUnitOfWork _unitOfWork
        +Handle(LoginCommand, CancellationToken) Task~AuthResponse~
    }

    class RegisterUserCommand {
        <<record>>
        +string Email
        +string Password
        +string FirstName
        +string LastName
        +string? Department
    }
    class RegisterUserCommandValidator {
        +RegisterUserCommandValidator()
    }
    class RegisterUserCommandHandler {
        -IUserRepository _userRepository
        -IIdentityService _identityService
        -IJwtService _jwtService
        -IRefreshTokenRepository _refreshTokenRepository
        -IUnitOfWork _unitOfWork
        +Handle(RegisterUserCommand, CancellationToken) Task~AuthResponse~
    }

    class RefreshTokenCommand {
        <<record>>
        +string RefreshToken
    }
    class RefreshTokenCommandHandler {
        -IRefreshTokenRepository _refreshTokenRepository
        -IUserRepository _userRepository
        -IJwtService _jwtService
        -IUnitOfWork _unitOfWork
        +Handle(RefreshTokenCommand, CancellationToken) Task~AuthResponse~
    }

    class RevokeTokenCommand {
        <<record>>
        +string RefreshToken
    }
    class RevokeTokenCommandHandler {
        -IRefreshTokenRepository _refreshTokenRepository
        -IUnitOfWork _unitOfWork
        +Handle(RevokeTokenCommand, CancellationToken) Task~Unit~
    }

    class ChangePasswordCommand {
        <<record>>
        +string CurrentPassword
        +string NewPassword
    }
    class ChangePasswordCommandHandler {
        -IUserRepository _userRepository
        -IIdentityService _identityService
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(ChangePasswordCommand, CancellationToken) Task~Unit~
    }

    LoginCommand --> LoginCommandHandler : handles
    LoginCommand --> LoginCommandValidator : validates
    RegisterUserCommand --> RegisterUserCommandHandler : handles
    RegisterUserCommand --> RegisterUserCommandValidator : validates
    RefreshTokenCommand --> RefreshTokenCommandHandler : handles
    RevokeTokenCommand --> RevokeTokenCommandHandler : handles
    ChangePasswordCommand --> ChangePasswordCommandHandler : handles
```

## 7. Application Layer — CQRS: Events Feature

```mermaid
classDiagram
    direction LR

    class CreateEventCommand {
        <<record>>
        +string Title
        +string? Description
        +DateTime StartUtc
        +DateTime EndUtc
        +int MinAttendees
        +int MaxAttendees
        +RecurrencePattern Recurrence
        +Guid? CategoryId
        +Guid? VenueId
    }
    class CreateEventCommandValidator {
        +CreateEventCommandValidator()
    }
    class CreateEventCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(CreateEventCommand, CancellationToken) Task~EventDto~
    }

    class UpdateEventCommand {
        <<record>>
        +Guid Id
        +string Title
        +string? Description
        +DateTime StartUtc
        +DateTime EndUtc
        +int MinAttendees
        +int MaxAttendees
        +RecurrencePattern Recurrence
        +Guid? CategoryId
        +Guid? VenueId
    }
    class UpdateEventCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(UpdateEventCommand, CancellationToken) Task~EventDto~
    }

    class DeleteEventCommand {
        <<record>>
        +Guid Id
    }
    class DeleteEventCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(DeleteEventCommand, CancellationToken) Task~Unit~
    }

    class PublishEventCommand {
        <<record>>
        +Guid Id
    }
    class PublishEventCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(PublishEventCommand, CancellationToken) Task~Unit~
    }

    class CancelEventCommand {
        <<record>>
        +Guid Id
    }
    class CancelEventCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(CancelEventCommand, CancellationToken) Task~Unit~
    }

    class GetEventsQuery {
        <<record>>
        +int Page
        +int PageSize
        +string? Search
        +Guid? CategoryId
    }
    class GetEventsQueryHandler {
        -IEventRepository _eventRepository
        -IUnitOfWork _unitOfWork
        +Handle(GetEventsQuery, CancellationToken) Task~PaginatedList~EventSummaryDto~~
    }

    class GetEventByIdQuery {
        <<record>>
        +Guid Id
    }
    class GetEventByIdQueryHandler {
        -IEventRepository _eventRepository
        -IUnitOfWork _unitOfWork
        +Handle(GetEventByIdQuery, CancellationToken) Task~EventDto~
    }

    class GetEventsByDateRangeQuery {
        <<record>>
        +DateTime StartUtc
        +DateTime EndUtc
    }
    class GetEventsByDateRangeQueryHandler {
        -IEventRepository _eventRepository
        +Handle(GetEventsByDateRangeQuery, CancellationToken) Task~IReadOnlyList~EventSummaryDto~~
    }

    class GetUpcomingEventsQuery {
        <<record>>
        +int Count
    }
    class GetUpcomingEventsQueryHandler {
        -IEventRepository _eventRepository
        +Handle(GetUpcomingEventsQuery, CancellationToken) Task~IReadOnlyList~EventSummaryDto~~
    }

    class GetEventAttendeesQuery {
        <<record>>
        +Guid EventId
    }
    class GetEventAttendeesQueryHandler {
        -IRegistrationRepository _registrationRepository
        -IEventRepository _eventRepository
        +Handle(GetEventAttendeesQuery, CancellationToken) Task~IReadOnlyList~AttendeeDto~~
    }

    CreateEventCommand --> CreateEventCommandHandler : handles
    CreateEventCommand --> CreateEventCommandValidator : validates
    UpdateEventCommand --> UpdateEventCommandHandler : handles
    DeleteEventCommand --> DeleteEventCommandHandler : handles
    PublishEventCommand --> PublishEventCommandHandler : handles
    CancelEventCommand --> CancelEventCommandHandler : handles
    GetEventsQuery --> GetEventsQueryHandler : handles
    GetEventByIdQuery --> GetEventByIdQueryHandler : handles
    GetEventsByDateRangeQuery --> GetEventsByDateRangeQueryHandler : handles
    GetUpcomingEventsQuery --> GetUpcomingEventsQueryHandler : handles
    GetEventAttendeesQuery --> GetEventAttendeesQueryHandler : handles
```

## 8. Application Layer — CQRS: Registrations, Notifications, Venues & Reports

```mermaid
classDiagram
    direction LR

    class CreateRegistrationCommand {
        <<record>>
        +Guid EventId
    }
    class CreateRegistrationCommandHandler {
        -IEventRepository _eventRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(CreateRegistrationCommand, CancellationToken) Task~RegistrationDto~
    }

    class CancelRegistrationCommand {
        <<record>>
        +Guid EventId
    }
    class CancelRegistrationCommandHandler {
        -IRegistrationRepository _registrationRepository
        -ICurrentUserService _currentUserService
        -IUnitOfWork _unitOfWork
        +Handle(CancelRegistrationCommand, CancellationToken) Task~Unit~
    }

    class GetUserRegistrationsQuery {
        <<record>>
    }
    class GetUserRegistrationsQueryHandler {
        -IRegistrationRepository _registrationRepository
        -ICurrentUserService _currentUserService
        +Handle(GetUserRegistrationsQuery, CancellationToken) Task~IReadOnlyList~RegistrationDto~~
    }

    class SendNotificationCommand {
        <<record>>
        +Guid UserId
        +string Title
        +string Message
        +NotificationType Type
        +Guid? EventId
    }
    class SendNotificationCommandHandler {
        -INotificationRepository _notificationRepository
        -INotificationService _notificationService
        -IUnitOfWork _unitOfWork
        +Handle(SendNotificationCommand, CancellationToken) Task~Unit~
    }

    class MarkNotificationReadCommand {
        <<record>>
        +Guid Id
    }
    class MarkNotificationReadCommandHandler {
        -INotificationRepository _notificationRepository
        -IUnitOfWork _unitOfWork
        +Handle(MarkNotificationReadCommand, CancellationToken) Task~Unit~
    }

    class GetUserNotificationsQuery {
        <<record>>
        +bool UnreadOnly
    }
    class GetUserNotificationsQueryHandler {
        -INotificationRepository _notificationRepository
        -ICurrentUserService _currentUserService
        +Handle(GetUserNotificationsQuery, CancellationToken) Task~IReadOnlyList~NotificationDto~~
    }

    class CreateVenueCommand {
        <<record>>
        +string Name
        +int Capacity
        +string Street
        +string City
        +string State
        +string ZipCode
        +string? Building
        +string? Room
    }
    class CreateVenueCommandHandler {
        -IApplicationDbContext _context
        -IUnitOfWork _unitOfWork
        +Handle(CreateVenueCommand, CancellationToken) Task~VenueDto~
    }

    class UpdateVenueCommand {
        <<record>>
        +Guid Id
        +string Name
        +int Capacity
    }
    class UpdateVenueCommandHandler {
        -IApplicationDbContext _context
        -IUnitOfWork _unitOfWork
        +Handle(UpdateVenueCommand, CancellationToken) Task~VenueDto~
    }

    class DeleteVenueCommand {
        <<record>>
        +Guid Id
    }
    class DeleteVenueCommandHandler {
        -IApplicationDbContext _context
        -IUnitOfWork _unitOfWork
        +Handle(DeleteVenueCommand, CancellationToken) Task~Unit~
    }

    CreateRegistrationCommand --> CreateRegistrationCommandHandler : handles
    CancelRegistrationCommand --> CancelRegistrationCommandHandler : handles
    GetUserRegistrationsQuery --> GetUserRegistrationsQueryHandler : handles
    SendNotificationCommand --> SendNotificationCommandHandler : handles
    MarkNotificationReadCommand --> MarkNotificationReadCommandHandler : handles
    GetUserNotificationsQuery --> GetUserNotificationsQueryHandler : handles
    CreateVenueCommand --> CreateVenueCommandHandler : handles
    UpdateVenueCommand --> UpdateVenueCommandHandler : handles
    DeleteVenueCommand --> DeleteVenueCommandHandler : handles
```

## 9. Application Layer — Domain Event Handlers

```mermaid
classDiagram
    direction TB

    class INotificationHandler~T~ {
        <<interface>>
        +Handle(T notification, CancellationToken) Task
    }

    class EventCreatedDomainEventHandler {
        -INotificationService _notificationService
        +Handle(EventCreatedDomainEvent, CancellationToken) Task
    }

    class EventCancelledDomainEventHandler {
        -INotificationService _notificationService
        +Handle(EventCancelledDomainEvent, CancellationToken) Task
    }

    class UserRegisteredDomainEventHandler {
        -INotificationService _notificationService
        +Handle(UserRegisteredDomainEvent, CancellationToken) Task
    }

    INotificationHandler~T~ <|.. EventCreatedDomainEventHandler
    INotificationHandler~T~ <|.. EventCancelledDomainEventHandler
    INotificationHandler~T~ <|.. UserRegisteredDomainEventHandler
```

## 10. Persistence Layer — Repositories & DbContext

```mermaid
classDiagram
    direction TB

    class ApplicationDbContext {
        +DbSet~Event~ Events
        +DbSet~User~ Users
        +DbSet~Registration~ Registrations
        +DbSet~Notification~ Notifications
        +DbSet~RefreshToken~ RefreshTokens
        +DbSet~EventCategory~ EventCategories
        +DbSet~Venue~ Venues
        -IMediator? _mediator
        #OnModelCreating(ModelBuilder) void
        +SaveChangesAsync(CancellationToken) Task~int~
    }

    class RepositoryBase~T~ {
        #ApplicationDbContext Context
        +GetByIdAsync(Guid id, CancellationToken) Task~T?~
        +GetAllAsync(CancellationToken) Task~IReadOnlyList~T~~
        +AddAsync(T entity, CancellationToken) Task~T~
        +UpdateAsync(T entity, CancellationToken) Task
        +DeleteAsync(T entity, CancellationToken) Task
    }

    class UnitOfWork {
        -ApplicationDbContext _context
        +SaveChangesAsync(CancellationToken) Task~int~
        +Dispose() void
    }

    class EventRepository {
        +GetPagedAsync(page, pageSize, search?, categoryId?, CancellationToken) Task
        +GetByDateRangeAsync(start, end, CancellationToken) Task
        +GetUpcomingAsync(count, CancellationToken) Task
        +GetByIdWithDetailsAsync(id, CancellationToken) Task~Event?~
    }

    class UserRepository {
        +GetByEmailAsync(email, CancellationToken) Task~User?~
        +EmailExistsAsync(email, CancellationToken) Task~bool~
    }

    class RegistrationRepository {
        +GetByEventIdAsync(eventId, CancellationToken) Task
        +GetByUserIdAsync(userId, CancellationToken) Task
        +GetByUserAndEventAsync(userId, eventId, CancellationToken) Task~Registration?~
    }

    class NotificationRepository {
        +GetByUserIdAsync(userId, unreadOnly, CancellationToken) Task
        +GetUnreadCountAsync(userId, CancellationToken) Task~int~
    }

    class RefreshTokenRepository {
        +GetByTokenAsync(token, CancellationToken) Task~RefreshToken?~
        +RevokeAllForUserAsync(userId, CancellationToken) Task
    }

    DbContext <|-- ApplicationDbContext
    ApplicationDbContext ..|> IApplicationDbContext

    RepositoryBase~T~ ..|> IRepository~T~
    RepositoryBase~T~ --> ApplicationDbContext

    RepositoryBase~T~ <|-- EventRepository
    RepositoryBase~T~ <|-- UserRepository
    RepositoryBase~T~ <|-- RegistrationRepository
    RepositoryBase~T~ <|-- NotificationRepository
    RepositoryBase~T~ <|-- RefreshTokenRepository

    EventRepository ..|> IEventRepository
    UserRepository ..|> IUserRepository
    RegistrationRepository ..|> IRegistrationRepository
    NotificationRepository ..|> INotificationRepository
    RefreshTokenRepository ..|> IRefreshTokenRepository

    UnitOfWork ..|> IUnitOfWork
    UnitOfWork --> ApplicationDbContext
```

## 11. Infrastructure Layer — Services

```mermaid
classDiagram
    direction TB

    class JwtService {
        -IConfiguration _configuration
        +GenerateAccessToken(User user) string
        +GenerateRefreshToken() string
        +ValidateToken(string token) bool
    }

    class IdentityService {
        +HashPassword(string password) string
        +VerifyPassword(string password, string hash) bool
    }

    class CurrentUserService {
        -IHttpContextAccessor _httpContextAccessor
        +Guid? UserId
        +string? Email
        +string? Role
    }

    class EmailService {
        -ILogger~EmailService~ _logger
        +SendEmailAsync(to, subject, body, CancellationToken) Task
    }

    class NotificationServiceImpl["NotificationService"] {
        -IHubContext~NotificationHub~ _hubContext
        +SendNotificationAsync(userId, title, message, CancellationToken) Task
        +SendToAllAsync(title, message, CancellationToken) Task
    }

    class NotificationHub {
        +OnConnectedAsync() Task
        +OnDisconnectedAsync(Exception?) Task
    }

    JwtService ..|> IJwtService
    IdentityService ..|> IIdentityService
    CurrentUserService ..|> ICurrentUserService
    EmailService ..|> IEmailService
    NotificationServiceImpl ..|> INotificationService
    NotificationServiceImpl --> NotificationHub

    Hub <|-- NotificationHub
```

## 12. API Layer — Controllers & Middleware

```mermaid
classDiagram
    direction TB

    class ControllerBase {
        <<abstract>>
    }

    class AuthController {
        -IMediator _mediator
        +Register(RegisterUserCommand) IActionResult
        +Login(LoginCommand) IActionResult
        +RefreshToken(RefreshTokenCommand) IActionResult
        +RevokeToken(RevokeTokenCommand) IActionResult
        +ChangePassword(ChangePasswordCommand) IActionResult
    }

    class EventsController {
        -IMediator _mediator
        +GetEvents(page, pageSize, search?, categoryId?) IActionResult
        +GetEvent(Guid id) IActionResult
        +CreateEvent(CreateEventCommand) IActionResult
        +UpdateEvent(Guid id, UpdateEventCommand) IActionResult
        +DeleteEvent(Guid id) IActionResult
        +PublishEvent(Guid id) IActionResult
        +CancelEvent(Guid id) IActionResult
        +GetAttendees(Guid id) IActionResult
        +GetUpcoming(int count) IActionResult
        +Register(Guid eventId) IActionResult
        +CancelRegistration(Guid eventId) IActionResult
    }

    class UsersController {
        -IMediator _mediator
        -ICurrentUserService _currentUserService
        -IUserRepository _userRepository
        -IUnitOfWork _unitOfWork
        -IWebHostEnvironment _env
        +GetCurrentUser() IActionResult
        +UpdateProfile(UpdateProfileRequest) IActionResult
        +UploadProfilePicture(IFormFile file) IActionResult
        +DeleteProfilePicture() IActionResult
        +GetMyRegistrations() IActionResult
    }

    class NotificationsController {
        -IMediator _mediator
        +GetNotifications(bool unreadOnly) IActionResult
        +MarkAsRead(Guid id) IActionResult
    }

    class VenuesController {
        -IMediator _mediator
        +GetAll() IActionResult
        +GetById(Guid id) IActionResult
        +Create(CreateVenueCommand) IActionResult
        +Update(Guid id, UpdateVenueCommand) IActionResult
        +Delete(Guid id) IActionResult
    }

    class ReportsController {
        -IMediator _mediator
        +GetAttendanceReport() IActionResult
        +GetMonthlyReport(int? year) IActionResult
        +GetPopularEvents(int top) IActionResult
    }

    class CalendarController {
        -IMediator _mediator
        +GetCalendarEvents(DateTime start, DateTime end) IActionResult
    }

    class ExceptionHandlingMiddleware {
        -RequestDelegate _next
        -ILogger _logger
        +InvokeAsync(HttpContext context) Task
        -HandleExceptionAsync(HttpContext, Exception)$ Task
    }

    class ErrorResponse {
        <<record>>
        +string Title
        +string Detail
        +object? Errors
    }

    ControllerBase <|-- AuthController
    ControllerBase <|-- EventsController
    ControllerBase <|-- UsersController
    ControllerBase <|-- NotificationsController
    ControllerBase <|-- VenuesController
    ControllerBase <|-- ReportsController
    ControllerBase <|-- CalendarController

    ExceptionHandlingMiddleware --> ErrorResponse
```

## 13. Architecture Overview — Layer Dependencies

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer"]
        Controllers["Controllers<br/><i>Auth, Events, Users,<br/>Notifications, Venues,<br/>Reports, Calendar</i>"]
        Middleware["ExceptionHandlingMiddleware"]
    end

    subgraph Application["Application Layer"]
        Commands["Commands & Handlers"]
        Queries["Queries & Handlers"]
        DomainEventHandlers["Domain Event Handlers"]
        Interfaces["Service Interfaces<br/><i>IJwtService, IIdentityService,<br/>ICurrentUserService, IEmailService,<br/>INotificationService</i>"]
        DTOs["DTOs & Models"]
    end

    subgraph Domain["Domain Layer"]
        Entities["Entities<br/><i>User, Event, Registration,<br/>Notification, RefreshToken,<br/>EventCategory, Venue</i>"]
        ValueObjects["Value Objects<br/><i>Address, Capacity,<br/>DateTimeRange</i>"]
        DomainEvents["Domain Events"]
        RepoInterfaces["Repository Interfaces<br/><i>IRepository&lt;T&gt;, IUnitOfWork,<br/>IEventRepository, etc.</i>"]
        Enums["Enums"]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        Services["Services<br/><i>JwtService, IdentityService,<br/>CurrentUserService, EmailService,<br/>NotificationService</i>"]
        Hubs["SignalR Hubs<br/><i>NotificationHub</i>"]
    end

    subgraph Persistence["Persistence Layer"]
        DbContext["ApplicationDbContext"]
        Repositories["Repositories<br/><i>RepositoryBase&lt;T&gt;, UnitOfWork,<br/>EventRepository, etc.</i>"]
        Configurations["Entity Configurations"]
    end

    Controllers --> Commands
    Controllers --> Queries
    Middleware --> Controllers

    Commands --> Entities
    Commands --> RepoInterfaces
    Queries --> Entities
    Queries --> RepoInterfaces
    DomainEventHandlers --> Interfaces
    DomainEventHandlers --> DomainEvents

    Entities --> ValueObjects
    Entities --> Enums
    Entities --> DomainEvents

    Services --> Interfaces
    Hubs -.-> Services

    Repositories --> RepoInterfaces
    DbContext --> Entities
    Configurations --> DbContext

    style Domain fill:#dbeafe,stroke:#3b82f6,stroke-width:2px
    style Application fill:#fef3c7,stroke:#f59e0b,stroke-width:2px
    style Infrastructure fill:#f3e8ff,stroke:#a855f7,stroke-width:2px
    style Persistence fill:#dcfce7,stroke:#22c55e,stroke-width:2px
    style Presentation fill:#fee2e2,stroke:#ef4444,stroke-width:2px
```
