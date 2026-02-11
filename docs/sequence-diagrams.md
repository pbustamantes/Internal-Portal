# Sequence Diagrams

## 1. User Registration

```mermaid
sequenceDiagram
    actor Client
    participant Auth as AuthController
    participant MediatR
    participant Handler as RegisterUserCommandHandler
    participant UserRepo as IUserRepository
    participant Identity as IIdentityService
    participant JWT as IJwtService
    participant TokenRepo as IRefreshTokenRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database

    Client->>Auth: POST /api/auth/register
    Auth->>MediatR: Send(RegisterUserCommand)
    MediatR->>Handler: Handle(command)

    Handler->>UserRepo: EmailExistsAsync(email)
    UserRepo-->>Handler: false
    Handler->>Identity: HashPassword(password)
    Identity-->>Handler: passwordHash

    Note over Handler: User.Create(email, hash,<br/>firstName, lastName)

    Handler->>UserRepo: AddAsync(user)
    Handler->>JWT: GenerateAccessToken(user)
    JWT-->>Handler: accessToken
    Handler->>JWT: GenerateRefreshToken()
    JWT-->>Handler: refreshToken
    Handler->>TokenRepo: AddAsync(refreshToken)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()

    Note over DbCtx: Set CreatedAtUtc audit fields

    DbCtx->>DB: INSERT User, RefreshToken
    DB-->>DbCtx: OK

    Note over DbCtx: No domain events to dispatch

    DbCtx-->>UoW: OK
    UoW-->>Handler: OK
    Handler-->>MediatR: AuthResponse
    MediatR-->>Auth: AuthResponse
    Auth-->>Client: 200 OK {accessToken, refreshToken, user}
```

## 2. User Login

```mermaid
sequenceDiagram
    actor Client
    participant Auth as AuthController
    participant MediatR
    participant Handler as LoginCommandHandler
    participant UserRepo as IUserRepository
    participant Identity as IIdentityService
    participant JWT as IJwtService
    participant TokenRepo as IRefreshTokenRepository
    participant UoW as IUnitOfWork
    participant DB as Database

    Client->>Auth: POST /api/auth/login
    Auth->>MediatR: Send(LoginCommand)
    MediatR->>Handler: Handle(command)

    Handler->>UserRepo: GetByEmailAsync(email)
    UserRepo-->>Handler: user

    alt User not found or inactive
        Handler-->>Client: 400 Bad Request
    end

    Handler->>Identity: VerifyPassword(password, user.PasswordHash)
    Identity-->>Handler: true

    alt Password mismatch
        Handler-->>Client: 400 Bad Request
    end

    Handler->>JWT: GenerateAccessToken(user)
    JWT-->>Handler: accessToken
    Handler->>JWT: GenerateRefreshToken()
    JWT-->>Handler: refreshToken
    Handler->>TokenRepo: AddAsync(refreshToken)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DB: INSERT RefreshToken
    DB-->>UoW: OK

    Handler-->>MediatR: AuthResponse
    MediatR-->>Auth: AuthResponse
    Auth-->>Client: 200 OK {accessToken, refreshToken, user}
```

## 3. Token Refresh

```mermaid
sequenceDiagram
    actor Client
    participant Auth as AuthController
    participant MediatR
    participant Handler as RefreshTokenCommandHandler
    participant TokenRepo as IRefreshTokenRepository
    participant UserRepo as IUserRepository
    participant JWT as IJwtService
    participant UoW as IUnitOfWork
    participant DB as Database

    Client->>Auth: POST /api/auth/refresh
    Auth->>MediatR: Send(RefreshTokenCommand)
    MediatR->>Handler: Handle(command)

    Handler->>TokenRepo: GetByTokenAsync(refreshToken)
    TokenRepo-->>Handler: existingToken

    alt Token not found or inactive
        Handler-->>Client: 400 Bad Request
    end

    Handler->>UserRepo: GetByIdAsync(existingToken.UserId)
    UserRepo-->>Handler: user

    Note over Handler: Revoke old token:<br/>RevokedAtUtc = now<br/>ReplacedByToken = newToken

    Handler->>TokenRepo: UpdateAsync(existingToken)
    Handler->>TokenRepo: AddAsync(newRefreshToken)
    Handler->>JWT: GenerateAccessToken(user)
    JWT-->>Handler: newAccessToken
    Handler->>UoW: SaveChangesAsync()
    UoW->>DB: UPDATE old token, INSERT new token
    DB-->>UoW: OK

    Handler-->>MediatR: AuthResponse
    MediatR-->>Auth: AuthResponse
    Auth-->>Client: 200 OK {newAccessToken, newRefreshToken, user}
```

## 4. Create Event (Draft)

```mermaid
sequenceDiagram
    actor Client
    participant MW as ExceptionHandlingMiddleware
    participant JWTAuth as JWT Authentication
    participant Events as EventsController
    participant MediatR
    participant Handler as CreateEventCommandHandler
    participant CurrentUser as ICurrentUserService
    participant EventRepo as IEventRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database

    Client->>MW: POST /api/events<br/>Authorization: Bearer {token}
    MW->>JWTAuth: Validate JWT
    JWTAuth-->>MW: Claims (userId, email, role)
    MW->>Events: CreateEvent(command)
    Events->>MediatR: Send(CreateEventCommand)
    MediatR->>Handler: Handle(command)

    Handler->>CurrentUser: UserId
    CurrentUser-->>Handler: userId

    Note over Handler: Create Event aggregate:<br/>Status = Draft<br/>Schedule = DateTimeRange<br/>Capacity = Capacity<br/>Location = Address

    Handler->>EventRepo: AddAsync(event)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()

    Note over DbCtx: Set CreatedAtUtc

    DbCtx->>DB: INSERT Event
    DB-->>DbCtx: OK

    Note over DbCtx: No domain events<br/>(Draft status)

    DbCtx-->>UoW: OK
    Handler-->>MediatR: EventDto
    MediatR-->>Events: EventDto
    Events-->>MW: 201 Created
    MW-->>Client: 201 Created {eventDto}<br/>Location: /api/events/{id}
```

## 5. Publish Event (with real-time notification)

```mermaid
sequenceDiagram
    actor Client
    participant Events as EventsController
    participant MediatR
    participant Handler as PublishEventCommandHandler
    participant CurrentUser as ICurrentUserService
    participant EventRepo as IEventRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database
    participant EventHandler as EventCreatedDomainEventHandler
    participant NotifSvc as NotificationService
    participant SignalR as SignalR Hub
    actor AllClients as Connected Clients

    Client->>Events: POST /api/events/{id}/publish<br/>Authorization: Bearer {token}
    Events->>MediatR: Send(PublishEventCommand)
    MediatR->>Handler: Handle(command)

    Handler->>EventRepo: GetByIdAsync(id)
    EventRepo-->>Handler: event
    Handler->>CurrentUser: UserId, Role
    CurrentUser-->>Handler: userId, role

    Note over Handler: Authorization check:<br/>organizer or admin

    Note over Handler: event.Publish()<br/>→ Validate not past<br/>→ Validate status == Draft<br/>→ Status = Published<br/>→ AddDomainEvent(<br/>EventCreatedDomainEvent)

    Handler->>EventRepo: UpdateAsync(event)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()

    Note over DbCtx: Set UpdatedAtUtc

    DbCtx->>DB: UPDATE Event (status → Published)
    DB-->>DbCtx: OK

    Note over DbCtx: Extract & dispatch<br/>domain events

    DbCtx->>MediatR: Publish(EventCreatedDomainEvent)
    MediatR->>EventHandler: Handle(event)
    EventHandler->>NotifSvc: SendToAllAsync("New Event Published", message)
    NotifSvc->>SignalR: Clients.All.SendAsync("ReceiveNotification")
    SignalR-->>AllClients: Real-time notification (WebSocket)
    NotifSvc-->>EventHandler: OK
    EventHandler-->>MediatR: OK
    MediatR-->>DbCtx: OK

    DbCtx-->>UoW: OK
    Handler-->>Events: Unit
    Events-->>Client: 204 No Content
```

## 6. Cancel Event (with real-time notification)

```mermaid
sequenceDiagram
    actor Client
    participant Events as EventsController
    participant MediatR
    participant Handler as CancelEventCommandHandler
    participant EventRepo as IEventRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database
    participant EventHandler as EventCancelledDomainEventHandler
    participant NotifSvc as NotificationService
    participant SignalR as SignalR Hub
    actor AllClients as Connected Clients

    Client->>Events: POST /api/events/{id}/cancel<br/>Authorization: Bearer {token}
    Events->>MediatR: Send(CancelEventCommand)
    MediatR->>Handler: Handle(command)

    Handler->>EventRepo: GetByIdAsync(id)
    EventRepo-->>Handler: event

    Note over Handler: Authorization check

    Note over Handler: event.Cancel()<br/>→ Validate not past<br/>→ Validate not already cancelled<br/>→ Status = Cancelled<br/>→ AddDomainEvent(<br/>EventCancelledDomainEvent)

    Handler->>EventRepo: UpdateAsync(event)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()
    DbCtx->>DB: UPDATE Event (status → Cancelled)
    DB-->>DbCtx: OK

    DbCtx->>MediatR: Publish(EventCancelledDomainEvent)
    MediatR->>EventHandler: Handle(event)
    EventHandler->>NotifSvc: SendToAllAsync("Event Cancelled", message)
    NotifSvc->>SignalR: Clients.All.SendAsync("ReceiveNotification")
    SignalR-->>AllClients: Real-time notification (WebSocket)

    DbCtx-->>UoW: OK
    Handler-->>Events: Unit
    Events-->>Client: 204 No Content
```

## 7. Register for Event (with targeted notification)

```mermaid
sequenceDiagram
    actor Client
    participant Events as EventsController
    participant MediatR
    participant Handler as CreateRegistrationCommandHandler
    participant CurrentUser as ICurrentUserService
    participant EventRepo as IEventRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database
    participant RegHandler as UserRegisteredDomainEventHandler
    participant NotifSvc as NotificationService
    participant SignalR as SignalR Hub

    Client->>Events: POST /api/events/{id}/register<br/>Authorization: Bearer {token}
    Events->>MediatR: Send(CreateRegistrationCommand)
    MediatR->>Handler: Handle(command)

    Handler->>CurrentUser: UserId
    CurrentUser-->>Handler: userId
    Handler->>EventRepo: GetByIdWithDetailsAsync(eventId)
    EventRepo-->>Handler: event (with registrations)

    Note over Handler: event.Register(userId)<br/>→ Validate not past<br/>→ Validate status == Published<br/>→ Validate not already registered<br/>→ Check capacity

    alt Capacity available
        Note over Handler: Status = Confirmed
    else Capacity full
        Note over Handler: Status = Waitlisted
    end

    Note over Handler: Create Registration<br/>AddDomainEvent(<br/>UserRegisteredDomainEvent)

    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()

    Note over DbCtx: Set CreatedAtUtc

    DbCtx->>DB: INSERT Registration
    DB-->>DbCtx: OK

    DbCtx->>MediatR: Publish(UserRegisteredDomainEvent)
    MediatR->>RegHandler: Handle(event)

    alt status == Confirmed
        RegHandler->>NotifSvc: SendNotificationAsync(userId,<br/>"Registration confirmed!")
    else status == Waitlisted
        RegHandler->>NotifSvc: SendNotificationAsync(userId,<br/>"Added to waitlist.")
    end

    NotifSvc->>SignalR: Clients.User(userId).SendAsync("ReceiveNotification")
    SignalR-->>Client: Targeted real-time notification (WebSocket)

    DbCtx-->>UoW: OK
    Handler-->>Events: RegistrationDto
    Events-->>Client: 200 OK {registrationDto}
```

## 8. Cancel Registration

```mermaid
sequenceDiagram
    actor Client
    participant Events as EventsController
    participant MediatR
    participant Handler as CancelRegistrationCommandHandler
    participant CurrentUser as ICurrentUserService
    participant RegRepo as IRegistrationRepository
    participant UoW as IUnitOfWork
    participant DbCtx as ApplicationDbContext
    participant DB as Database

    Client->>Events: DELETE /api/events/{id}/register<br/>Authorization: Bearer {token}
    Events->>MediatR: Send(CancelRegistrationCommand)
    MediatR->>Handler: Handle(command)

    Handler->>CurrentUser: UserId
    CurrentUser-->>Handler: userId
    Handler->>RegRepo: GetByUserAndEventAsync(userId, eventId)
    RegRepo-->>Handler: registration

    alt Registration not found
        Handler-->>Client: 404 Not Found
    end

    Note over Handler: registration.Cancel()<br/>→ Validate not already cancelled<br/>→ Status = Cancelled<br/>→ AddDomainEvent(<br/>RegistrationCancelledDomainEvent)

    Handler->>RegRepo: UpdateAsync(registration)
    Handler->>UoW: SaveChangesAsync()
    UoW->>DbCtx: SaveChangesAsync()
    DbCtx->>DB: UPDATE Registration (status → Cancelled)
    DB-->>DbCtx: OK

    Note over DbCtx: RegistrationCancelledDomainEvent<br/>dispatched (no handler registered)

    DbCtx-->>UoW: OK
    Handler-->>Events: Unit
    Events-->>Client: 204 No Content
```

## 9. View Event (with auto-complete for past events)

```mermaid
sequenceDiagram
    actor Client
    participant Events as EventsController
    participant MediatR
    participant Handler as GetEventByIdQueryHandler
    participant EventRepo as IEventRepository
    participant UoW as IUnitOfWork
    participant DB as Database

    Client->>Events: GET /api/events/{id}<br/>Authorization: Bearer {token}
    Events->>MediatR: Send(GetEventByIdQuery)
    MediatR->>Handler: Handle(query)

    Handler->>EventRepo: GetByIdWithDetailsAsync(id)
    EventRepo-->>Handler: event (with organizer, category,<br/>venue, registrations)

    alt Event is Published AND EndUtc < now
        Note over Handler: event.Complete()<br/>→ Status = Completed
        Handler->>UoW: SaveChangesAsync()
        UoW->>DB: UPDATE Event (status → Completed)
        DB-->>UoW: OK
    end

    Note over Handler: Map to EventDto<br/>(count confirmed attendees)

    Handler-->>MediatR: EventDto
    MediatR-->>Events: EventDto
    Events-->>Client: 200 OK {eventDto}
```

## 10. Error Handling Flow

```mermaid
sequenceDiagram
    actor Client
    participant MW as ExceptionHandlingMiddleware
    participant Controller
    participant MediatR
    participant Handler
    participant Domain as Domain Entity

    Client->>MW: Any HTTP Request
    MW->>Controller: Forward request

    alt Validation Error
        Controller->>MediatR: Send(command)
        MediatR->>Handler: Handle(command)
        Handler-->>MW: throw ValidationException(errors)
        MW-->>Client: 400 Bad Request {title, detail, errors}
    end

    alt Not Found
        Controller->>MediatR: Send(command)
        MediatR->>Handler: Handle(command)
        Handler-->>MW: throw NotFoundException(name, key)
        MW-->>Client: 404 Not Found {title, detail}
    end

    alt Forbidden
        Controller->>MediatR: Send(command)
        MediatR->>Handler: Handle(command)
        Handler-->>MW: throw ForbiddenException()
        MW-->>Client: 403 Forbidden {title, detail}
    end

    alt Domain Rule Violation
        Controller->>MediatR: Send(command)
        MediatR->>Handler: Handle(command)
        Handler->>Domain: Publish() / Cancel() / Register()
        Domain-->>MW: throw DomainException(message)
        MW-->>Client: 400 Bad Request {title, detail}
    end

    alt Unhandled Error
        Controller->>MediatR: Send(command)
        MediatR-->>MW: throw Exception
        Note over MW: Log error
        MW-->>Client: 500 Internal Server Error
    end
```

## 11. SignalR Connection Lifecycle

```mermaid
sequenceDiagram
    actor Client as Browser Client
    participant SignalR as SignalR Hub
    participant NotifSvc as NotificationService
    participant NotifCtrl as NotificationsController
    participant MediatR
    participant DB as Database

    Note over Client,SignalR: Connection Setup
    Client->>SignalR: WebSocket upgrade<br/>?access_token={jwt}
    SignalR->>SignalR: Validate JWT from query string
    SignalR->>SignalR: OnConnectedAsync()<br/>Groups.AddToGroupAsync(userId)
    SignalR-->>Client: Connected

    Note over Client,DB: Receive Real-time Notifications
    NotifSvc->>SignalR: Clients.User(userId)<br/>.SendAsync("ReceiveNotification")
    SignalR-->>Client: {title, message, timestamp}

    Note over Client,DB: Fetch Notification History
    Client->>NotifCtrl: GET /api/notifications?unreadOnly=true
    NotifCtrl->>MediatR: Send(GetUserNotificationsQuery)
    MediatR->>DB: SELECT FROM Notifications<br/>WHERE UserId = @userId
    DB-->>MediatR: notifications
    MediatR-->>NotifCtrl: List<NotificationDto>
    NotifCtrl-->>Client: 200 OK [{id, title, message, isRead}]

    Note over Client,DB: Mark Notification as Read
    Client->>NotifCtrl: PUT /api/notifications/{id}/read
    NotifCtrl->>MediatR: Send(MarkNotificationReadCommand)
    MediatR->>DB: UPDATE Notification SET IsRead = true
    DB-->>MediatR: OK
    MediatR-->>NotifCtrl: Unit
    NotifCtrl-->>Client: 204 No Content

    Note over Client,SignalR: Disconnection
    Client->>SignalR: Disconnect
    SignalR->>SignalR: OnDisconnectedAsync()<br/>Groups.RemoveFromGroupAsync(userId)
```

## 12. Complete Event Lifecycle

```mermaid
sequenceDiagram
    actor Organizer
    actor Attendee
    participant API
    participant Domain as Event Entity
    participant DB as Database
    participant SignalR
    actor AllUsers as All Connected Users

    Note over Organizer,AllUsers: Phase 1: Creation
    Organizer->>API: POST /api/events
    API->>Domain: new Event(Status: Draft)
    Domain->>DB: INSERT Event
    API-->>Organizer: 201 Created (Draft)

    Note over Organizer,AllUsers: Phase 2: Publishing
    Organizer->>API: POST /api/events/{id}/publish
    API->>Domain: event.Publish()
    Domain->>Domain: Status = Published
    Domain->>Domain: AddDomainEvent(EventCreated)
    Domain->>DB: UPDATE Event
    DB-->>Domain: OK
    Domain->>SignalR: Broadcast "New Event Published"
    SignalR-->>AllUsers: Real-time notification
    API-->>Organizer: 204 No Content

    Note over Organizer,AllUsers: Phase 3: Registration
    Attendee->>API: POST /api/events/{id}/register
    API->>Domain: event.Register(userId)
    Domain->>Domain: Check capacity
    alt Has space
        Domain->>Domain: Status = Confirmed
    else Full
        Domain->>Domain: Status = Waitlisted
    end
    Domain->>Domain: AddDomainEvent(UserRegistered)
    Domain->>DB: INSERT Registration
    DB-->>Domain: OK
    Domain->>SignalR: Notify attendee
    SignalR-->>Attendee: "Registration confirmed!" or "Added to waitlist"
    API-->>Attendee: 200 OK

    Note over Organizer,AllUsers: Phase 4: Cancellation (optional path)
    Organizer->>API: POST /api/events/{id}/cancel
    API->>Domain: event.Cancel()
    Domain->>Domain: Status = Cancelled
    Domain->>Domain: AddDomainEvent(EventCancelled)
    Domain->>DB: UPDATE Event
    DB-->>Domain: OK
    Domain->>SignalR: Broadcast "Event Cancelled"
    SignalR-->>AllUsers: Real-time notification
    API-->>Organizer: 204 No Content

    Note over Organizer,AllUsers: Phase 5: Auto-completion (time-based)
    Attendee->>API: GET /api/events/{id}
    API->>Domain: Check IsInPast && Published
    alt Past event
        Domain->>Domain: event.Complete()
        Domain->>Domain: Status = Completed
        Domain->>DB: UPDATE Event
    end
    API-->>Attendee: 200 OK (Completed)

    Note over Organizer,AllUsers: Past events block all mutations
    Organizer->>API: PUT /api/events/{id}
    API->>Domain: event.EnsureModifiable()
    Domain-->>API: throw DomainException("Past events cannot be modified")
    API-->>Organizer: 400 Bad Request
```
