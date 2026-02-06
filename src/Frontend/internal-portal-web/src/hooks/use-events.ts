import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/lib/api';
import type { EventDto, EventSummary, PaginatedList, Attendee, CreateEventForm } from '@/types';

export function useEvents(page = 1, pageSize = 10, search?: string, categoryId?: string) {
  return useQuery({
    queryKey: ['events', page, pageSize, search, categoryId],
    queryFn: async () => {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (search) params.set('search', search);
      if (categoryId) params.set('categoryId', categoryId);
      const { data } = await api.get<PaginatedList<EventSummary>>(`/events?${params}`);
      return data;
    },
  });
}

export function useEvent(id: string) {
  return useQuery({
    queryKey: ['events', id],
    queryFn: async () => {
      const { data } = await api.get<EventDto>(`/events/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

export function useUpcomingEvents(count = 5) {
  return useQuery({
    queryKey: ['events', 'upcoming', count],
    queryFn: async () => {
      const { data } = await api.get<EventSummary[]>(`/events/upcoming?count=${count}`);
      return data;
    },
  });
}

export function useCalendarEvents(start: string, end: string) {
  return useQuery({
    queryKey: ['calendar', start, end],
    queryFn: async () => {
      const { data } = await api.get<EventSummary[]>(`/calendar?start=${start}&end=${end}`);
      return data;
    },
    enabled: !!start && !!end,
  });
}

export function useEventAttendees(eventId: string) {
  return useQuery({
    queryKey: ['events', eventId, 'attendees'],
    queryFn: async () => {
      const { data } = await api.get<Attendee[]>(`/events/${eventId}/attendees`);
      return data;
    },
    enabled: !!eventId,
  });
}

export function useCreateEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (event: CreateEventForm) => {
      const { data } = await api.post<EventDto>('/events', event);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
}

export function useUpdateEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...event }: CreateEventForm & { id: string }) => {
      const { data } = await api.put<EventDto>(`/events/${id}`, { id, ...event });
      return data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['events', variables.id] });
    },
  });
}

export function useDeleteEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/events/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
}

export function usePublishEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.post(`/events/${id}/publish`);
    },
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['events', id] });
    },
  });
}

export function useCancelEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.post(`/events/${id}/cancel`);
    },
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['events', id] });
    },
  });
}
