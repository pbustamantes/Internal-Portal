# API Reference

**Base URL:** `http://localhost:5001/api`

## Authentication

All endpoints except Auth registration, login, and token refresh require a JWT Bearer token:

```
Authorization: Bearer <accessToken>
```

Tokens are issued via the `/api/auth/login` and `/api/auth/register` endpoints. Access tokens expire after the configured duration; use `/api/auth/refresh` to obtain a new pair.

---

## Error Responses

All errors return a consistent JSON structure:

```json
{
  "title": "string",
  "detail": "string",
  "errors": { }
}
```

| Exception | Status Code | `errors` field |
|---|---|---|
| `ValidationException` | 400 Bad Request | `{ "fieldName": ["error1", "error2"] }` |
| `DomainException` | 400 Bad Request | `null` |
| `ForbiddenException` | 403 Forbidden | `null` |
| `NotFoundException` | 404 Not Found | `null` |
| Unhandled | 500 Internal Server Error | `null` |

---

## Auth (`/api/auth`)

### POST `/api/auth/register`

Register a new user account.

**Auth:** None

**Request Body:**

| Field | Type | Required | Validation |
|---|---|---|---|
| `email` | string | Yes | Valid email, max 256 chars |
| `password` | string | Yes | 8-128 characters |
| `firstName` | string | Yes | Max 100 chars |
| `lastName` | string | Yes | Max 100 chars |
| `department` | string | No | - |

**Response:** `200 OK`

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "2025-01-15T12:00:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "department": "Engineering",
    "role": "Employee",
    "profilePictureUrl": null
  }
}
```

**Errors:** `400` email already exists, validation failure

---

### POST `/api/auth/login`

Authenticate with email and password.

**Auth:** None

**Request Body:**

| Field | Type | Required | Validation |
|---|---|---|---|
| `email` | string | Yes | Valid email |
| `password` | string | Yes | Not empty |

**Response:** `200 OK` — same shape as register response

**Errors:** `400` invalid credentials, account inactive

---

### POST `/api/auth/refresh`

Exchange a refresh token for a new access/refresh token pair. The old refresh token is revoked.

**Auth:** None

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `refreshToken` | string | Yes |

**Response:** `200 OK` — same shape as register response

**Errors:** `400` token not found, expired, or revoked

---

### POST `/api/auth/revoke`

Revoke a refresh token.

**Auth:** Required

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `refreshToken` | string | Yes |

**Response:** `204 No Content`

**Errors:** `400` `401`

---

### POST `/api/auth/change-password`

Change the authenticated user's password.

**Auth:** Required

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `currentPassword` | string | Yes |
| `newPassword` | string | Yes |

**Response:** `204 No Content`

**Errors:** `400` incorrect current password `401`

---

## Users (`/api/users`)

All endpoints require authentication.

### GET `/api/users/me`

Get the authenticated user's profile.

**Response:** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "department": "Engineering",
  "role": "Employee",
  "profilePictureUrl": "/uploads/profiles/abc123.jpg"
}
```

**Errors:** `403` `404`

---

### PUT `/api/users/me`

Update the authenticated user's profile.

**Request Body:**

| Field | Type | Required |
|---|---|---|
| `firstName` | string | Yes |
| `lastName` | string | Yes |
| `department` | string | No |

**Response:** `200 OK` — UserDto

**Errors:** `400` `403` `404`

---

### POST `/api/users/me/profile-picture`

Upload a profile picture. Replaces existing picture if one exists.

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Constraints |
|---|---|---|---|
| `file` | binary | Yes | Max 5 MB, formats: .jpg .jpeg .png .gif .webp |

**Response:** `200 OK` — UserDto with updated `profilePictureUrl`

**Errors:** `400` empty file, exceeds size limit, invalid format `403` `404`

---

### DELETE `/api/users/me/profile-picture`

Remove the authenticated user's profile picture.

**Response:** `200 OK` — UserDto with `profilePictureUrl: null`

**Errors:** `403` `404`

---

### GET `/api/users/me/events`

Get the authenticated user's event registrations.

**Response:** `200 OK`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "John Doe",
    "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "eventTitle": "Team Standup",
    "status": "Confirmed",
    "registeredAtUtc": "2025-01-15T10:00:00Z"
  }
]
```

**Errors:** `403`

---

## Events (`/api/events`)

All endpoints require authentication. Mutating operations (update, delete, publish, cancel) require the user to be the event organizer or an Admin.

### GET `/api/events`

List events with pagination, search, and category filtering.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page |
| `search` | string | null | Filter by title (contains) |
| `categoryId` | guid | null | Filter by category |

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Team Standup",
      "startUtc": "2025-01-15T09:00:00Z",
      "endUtc": "2025-01-15T09:30:00Z",
      "maxAttendees": 50,
      "currentAttendees": 12,
      "status": "Published",
      "categoryName": "Meeting",
      "categoryColor": "#3B82F6",
      "organizerName": "Jane Smith"
    }
  ],
  "pageNumber": 1,
  "totalPages": 5,
  "totalCount": 42,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

### GET `/api/events/{id}`

Get full event details. If the event is Published and past, it is automatically marked as Completed.

**Path Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | guid | Event ID |

**Response:** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Team Standup",
  "description": "Daily sync meeting for the engineering team.",
  "startUtc": "2025-01-15T09:00:00Z",
  "endUtc": "2025-01-15T09:30:00Z",
  "minAttendees": 5,
  "maxAttendees": 50,
  "currentAttendees": 12,
  "status": "Published",
  "recurrence": "Daily",
  "locationStreet": "123 Main St",
  "locationCity": "Seattle",
  "locationState": "WA",
  "locationZipCode": "98101",
  "locationBuilding": "Building A",
  "locationRoom": "Room 301",
  "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "organizerName": "Jane Smith",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryName": "Meeting",
  "categoryColor": "#3B82F6",
  "venueId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "venueName": "Main Campus",
  "createdAtUtc": "2025-01-10T08:00:00Z"
}
```

**Errors:** `404`

---

### POST `/api/events`

Create a new event in Draft status.

**Request Body:**

| Field | Type | Required | Validation |
|---|---|---|---|
| `title` | string | Yes | Max 200 chars |
| `description` | string | No | Max 4000 chars |
| `startUtc` | datetime | Yes | Must be in the future (> now - 5 min) |
| `endUtc` | datetime | Yes | Must be after `startUtc` |
| `minAttendees` | int | Yes | >= 0 |
| `maxAttendees` | int | Yes | >= `minAttendees` |
| `street` | string | No | - |
| `city` | string | No | - |
| `state` | string | No | - |
| `zipCode` | string | No | - |
| `building` | string | No | - |
| `room` | string | No | - |
| `recurrence` | int | Yes | 0=None, 1=Daily, 2=Weekly, 3=Monthly |
| `categoryId` | guid | No | - |
| `venueId` | guid | No | - |

**Response:** `201 Created` — EventDto, with `Location` header

**Errors:** `400` validation failure `403`

---

### PUT `/api/events/{id}`

Update an existing event. Blocked for past or completed events.

**Path Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | guid | Event ID (must match body `id`) |

**Request Body:** Same fields as POST, plus `id` (guid, required).

**Response:** `200 OK` — EventDto

**Errors:** `400` past/completed event, id mismatch, validation `403` not organizer/admin `404`

---

### DELETE `/api/events/{id}`

Delete an event. Blocked for past or completed events.

**Response:** `204 No Content`

**Errors:** `400` past/completed event `403` `404`

---

### POST `/api/events/{id}/publish`

Publish a draft event. Sends a real-time notification to all connected users via SignalR. Blocked for past events.

**Response:** `204 No Content`

**Errors:** `400` not in Draft status, past event `403` `404`

---

### POST `/api/events/{id}/cancel`

Cancel an event. Sends a real-time notification to all connected users via SignalR. Blocked for past events.

**Response:** `204 No Content`

**Errors:** `400` already cancelled, past event `403` `404`

---

### GET `/api/events/{id}/attendees`

List all attendees for an event.

**Response:** `200 OK`

```json
[
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "John Doe",
    "email": "john@example.com",
    "department": "Engineering",
    "registrationStatus": "Confirmed",
    "registeredAtUtc": "2025-01-12T14:30:00Z",
    "profilePictureUrl": "/uploads/profiles/abc123.jpg"
  }
]
```

**Errors:** `404`

---

### GET `/api/events/upcoming`

Get the next upcoming published events.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `count` | int | 5 | Number of events to return |

**Response:** `200 OK` — array of EventSummaryDto

---

### POST `/api/events/{eventId}/register`

Register the authenticated user for an event. If the event is at capacity, the registration is waitlisted. Sends a targeted real-time notification to the user. Blocked for past events.

**Path Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `eventId` | guid | Event ID |

**Response:** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "John Doe",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventTitle": "Team Standup",
  "status": "Confirmed",
  "registeredAtUtc": "2025-01-12T14:30:00Z"
}
```

**Errors:** `400` not published, past event, already registered `403` `404`

---

### DELETE `/api/events/{eventId}/register`

Cancel the authenticated user's registration for an event.

**Response:** `204 No Content`

**Errors:** `400` already cancelled `403` `404`

---

## Notifications (`/api/notifications`)

All endpoints require authentication.

### GET `/api/notifications`

Get the authenticated user's notifications.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `unreadOnly` | bool | false | Return only unread notifications |

**Response:** `200 OK`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "New Event Published",
    "message": "A new event 'Team Standup' has been published!",
    "type": "EventCreated",
    "isRead": false,
    "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAtUtc": "2025-01-12T14:30:00Z"
  }
]
```

**Notification Types:** `EventCreated` `EventUpdated` `EventCancelled` `RegistrationConfirmed` `RegistrationCancelled` `Reminder` `Waitlisted` `PromotedFromWaitlist`

---

### PUT `/api/notifications/{id}/read`

Mark a notification as read.

**Response:** `204 No Content`

**Errors:** `404`

---

## Venues (`/api/venues`)

Read endpoints require authentication. Create, update, and delete require the **Admin** role.

### GET `/api/venues`

List all venues.

**Response:** `200 OK`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Main Campus",
    "capacity": 200,
    "street": "123 Main St",
    "city": "Seattle",
    "state": "WA",
    "zipCode": "98101",
    "building": "Building A",
    "room": null
  }
]
```

---

### GET `/api/venues/{id}`

Get a venue by ID.

**Response:** `200 OK` — VenueDto

**Errors:** `404`

---

### POST `/api/venues`

Create a new venue.

**Auth:** Admin only

**Request Body:**

| Field | Type | Required | Validation |
|---|---|---|---|
| `name` | string | Yes | Max 200 chars |
| `capacity` | int | Yes | > 0 |
| `street` | string | Yes | Not empty |
| `city` | string | Yes | Not empty |
| `state` | string | Yes | Not empty |
| `zipCode` | string | Yes | Not empty |
| `building` | string | No | - |
| `room` | string | No | - |

**Response:** `201 Created` — VenueDto, with `Location` header

**Errors:** `400` validation `403` not admin

---

### PUT `/api/venues/{id}`

Update a venue.

**Auth:** Admin only

**Request Body:** Same fields as POST, plus `id` (guid, must match route).

**Response:** `200 OK` — VenueDto

**Errors:** `400` id mismatch, validation `403` `404`

---

### DELETE `/api/venues/{id}`

Delete a venue.

**Auth:** Admin only

**Response:** `204 No Content`

**Errors:** `403` `404`

---

## Reports (`/api/reports`)

All endpoints require the **Admin** role.

### GET `/api/reports/attendance`

Get attendance statistics for all events.

**Response:** `200 OK`

```json
[
  {
    "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "eventTitle": "Team Standup",
    "startUtc": "2025-01-15T09:00:00Z",
    "totalRegistrations": 25,
    "confirmedCount": 20,
    "cancelledCount": 3,
    "waitlistedCount": 2,
    "attendanceRate": 0.80
  }
]
```

---

### GET `/api/reports/monthly`

Get monthly event and registration statistics.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `year` | int | Current year | Year to report on |

**Response:** `200 OK`

```json
[
  {
    "year": 2025,
    "month": 1,
    "totalEvents": 15,
    "totalRegistrations": 120,
    "averageAttendees": 8
  }
]
```

---

### GET `/api/reports/popular`

Get the most popular events by registration fill rate.

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `top` | int | 10 | Number of events to return |

**Response:** `200 OK`

```json
[
  {
    "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Company All-Hands",
    "categoryName": "Meeting",
    "registrationCount": 150,
    "maxAttendees": 200,
    "fillRate": 0.75
  }
]
```

---

## Calendar (`/api/calendar`)

### GET `/api/calendar`

Get events within a date range for calendar display.

**Auth:** Required

**Query Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `start` | datetime | Yes | Range start (ISO 8601) |
| `end` | datetime | Yes | Range end (ISO 8601) |

**Response:** `200 OK` — array of EventSummaryDto

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Team Standup",
    "startUtc": "2025-01-15T09:00:00Z",
    "endUtc": "2025-01-15T09:30:00Z",
    "maxAttendees": 50,
    "currentAttendees": 12,
    "status": "Published",
    "categoryName": "Meeting",
    "categoryColor": "#3B82F6",
    "organizerName": "Jane Smith"
  }
]
```

---

## Real-time Notifications (SignalR)

The API provides real-time notifications via a SignalR WebSocket hub.

**Hub URL:** `/hubs/notifications`

**Connection:** Pass the JWT access token as a query parameter:

```
wss://localhost:5001/hubs/notifications?access_token=<accessToken>
```

**Events received by client:**

| Event Name | Payload | Trigger |
|---|---|---|
| `ReceiveNotification` | `{ title, message, timestamp }` | Event published, cancelled, or user registered |

**Connection lifecycle:**
- On connect, the user is added to a group keyed by their user ID
- Broadcast notifications are sent to all connected clients
- Targeted notifications are sent only to the specific user's group
- On disconnect, the user is removed from their group

---

## Enumerations

All enums are stored as strings in the database and returned as strings in API responses. When sending enum values in request bodies, use the integer representation.

### EventStatus

| Value | Name | Description |
|---|---|---|
| 0 | Draft | Initial state, only visible to organizer |
| 1 | Published | Visible and open for registration |
| 2 | Cancelled | Event has been cancelled |
| 3 | Completed | Event end date has passed (set automatically) |

### RegistrationStatus

| Value | Name | Description |
|---|---|---|
| 0 | Pending | Awaiting processing |
| 1 | Confirmed | Registration confirmed |
| 2 | Cancelled | Registration cancelled by user |
| 3 | Waitlisted | Event at capacity, user is on waitlist |

### UserRole

| Value | Name | Description |
|---|---|---|
| 0 | Employee | Standard user |
| 1 | Organizer | Can create and manage events |
| 2 | Admin | Full access including venues, reports, and user management |

### RecurrencePattern

| Value | Name |
|---|---|
| 0 | None |
| 1 | Daily |
| 2 | Weekly |
| 3 | Monthly |

### NotificationType

| Value | Name |
|---|---|
| 0 | EventCreated |
| 1 | EventUpdated |
| 2 | EventCancelled |
| 3 | RegistrationConfirmed |
| 4 | RegistrationCancelled |
| 5 | Reminder |
| 6 | Waitlisted |
| 7 | PromotedFromWaitlist |
