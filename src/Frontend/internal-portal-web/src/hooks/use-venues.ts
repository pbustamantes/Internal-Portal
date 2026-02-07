import { useQuery } from '@tanstack/react-query';
import api from '@/lib/api';
import type { Venue } from '@/types';

export function useVenues() {
  return useQuery({
    queryKey: ['venues'],
    queryFn: async () => {
      const { data } = await api.get<Venue[]>('/venues');
      return data;
    },
  });
}
