import { useQuery } from '@tanstack/react-query';
import api from '@/lib/api';
import type { EventAttendanceReport, MonthlyEventsReport, PopularEvent } from '@/types';

export function useAttendanceReport() {
  return useQuery({
    queryKey: ['reports', 'attendance'],
    queryFn: async () => {
      const { data } = await api.get<EventAttendanceReport[]>('/reports/attendance');
      return data;
    },
  });
}

export function useMonthlyReport(year?: number) {
  return useQuery({
    queryKey: ['reports', 'monthly', year],
    queryFn: async () => {
      const params = year ? `?year=${year}` : '';
      const { data } = await api.get<MonthlyEventsReport[]>(`/reports/monthly${params}`);
      return data;
    },
  });
}

export function usePopularEvents(top = 10) {
  return useQuery({
    queryKey: ['reports', 'popular', top],
    queryFn: async () => {
      const { data } = await api.get<PopularEvent[]>(`/reports/popular?top=${top}`);
      return data;
    },
  });
}
