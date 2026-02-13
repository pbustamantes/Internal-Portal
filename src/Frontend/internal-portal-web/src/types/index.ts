export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  department?: string;
  role: string;
  profilePictureUrl?: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: User;
}

export interface EventDto {
  id: string;
  title: string;
  description?: string;
  startUtc: string;
  endUtc: string;
  minAttendees: number;
  maxAttendees: number;
  currentAttendees: number;
  status: string;
  recurrence: string;
  locationStreet?: string;
  locationCity?: string;
  locationState?: string;
  locationZipCode?: string;
  locationBuilding?: string;
  locationRoom?: string;
  organizerId: string;
  organizerName: string;
  categoryId?: string;
  categoryName?: string;
  categoryColor?: string;
  venueId?: string;
  venueName?: string;
  createdAtUtc: string;
}

export interface EventSummary {
  id: string;
  title: string;
  startUtc: string;
  endUtc: string;
  maxAttendees: number;
  currentAttendees: number;
  status: string;
  categoryName?: string;
  categoryColor?: string;
  organizerName: string;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface Attendee {
  userId: string;
  fullName: string;
  email: string;
  department?: string;
  registrationStatus: string;
  registeredAtUtc: string;
  profilePictureUrl?: string;
}

export interface Registration {
  id: string;
  userId: string;
  userName: string;
  eventId: string;
  eventTitle: string;
  status: string;
  registeredAtUtc: string;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  eventId?: string;
  createdAtUtc: string;
}

export interface EventAttendanceReport {
  eventId: string;
  eventTitle: string;
  startUtc: string;
  totalRegistrations: number;
  confirmedCount: number;
  cancelledCount: number;
  waitlistedCount: number;
  attendanceRate: number;
}

export interface MonthlyEventsReport {
  year: number;
  month: number;
  totalEvents: number;
  totalRegistrations: number;
  averageAttendees: number;
}

export interface PopularEvent {
  eventId: string;
  title: string;
  categoryName?: string;
  registrationCount: number;
  maxAttendees: number;
  fillRate: number;
}

export interface CreateEventForm {
  title: string;
  description?: string;
  startUtc: string;
  endUtc: string;
  minAttendees: number;
  maxAttendees: number;
  street?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  building?: string;
  room?: string;
  recurrence: number;
  categoryId?: string;
  venueId?: string;
}

export interface Venue {
  id: string;
  name: string;
  capacity: number;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  building?: string;
  room?: string;
}

export interface CreateVenueForm {
  name: string;
  capacity: number;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  building?: string;
  room?: string;
}

export interface ErrorResponse {
  title: string;
  detail: string;
  errors?: Record<string, string[]>;
}
