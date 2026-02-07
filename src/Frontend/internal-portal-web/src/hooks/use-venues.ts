import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/lib/api';
import type { Venue, CreateVenueForm } from '@/types';

export function useVenues() {
  return useQuery({
    queryKey: ['venues'],
    queryFn: async () => {
      const { data } = await api.get<Venue[]>('/venues');
      return data;
    },
  });
}

export function useVenue(id: string) {
  return useQuery({
    queryKey: ['venues', id],
    queryFn: async () => {
      const { data } = await api.get<Venue>(`/venues/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

export function useCreateVenue() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (venue: CreateVenueForm) => {
      const { data } = await api.post<Venue>('/venues', venue);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['venues'] });
    },
  });
}

export function useUpdateVenue() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...venue }: CreateVenueForm & { id: string }) => {
      const { data } = await api.put<Venue>(`/venues/${id}`, { id, ...venue });
      return data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['venues'] });
      queryClient.invalidateQueries({ queryKey: ['venues', variables.id] });
    },
  });
}

export function useDeleteVenue() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/venues/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['venues'] });
    },
  });
}
