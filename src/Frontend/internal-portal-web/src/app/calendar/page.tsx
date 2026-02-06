'use client';

import { useState, useMemo } from 'react';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Loading } from '@/components/ui/loading';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useCalendarEvents } from '@/hooks/use-events';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import Link from 'next/link';

export default function CalendarPage() {
  const [currentDate, setCurrentDate] = useState(new Date());
  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();

  const start = new Date(year, month, 1).toISOString();
  const end = new Date(year, month + 1, 0, 23, 59, 59).toISOString();

  const { data: events, isLoading } = useCalendarEvents(start, end);

  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const firstDayOfWeek = new Date(year, month, 1).getDay();
  const monthName = currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });

  const eventsByDay = useMemo(() => {
    const map: Record<number, typeof events> = {};
    events?.forEach(e => {
      const day = new Date(e.startUtc).getDate();
      if (!map[day]) map[day] = [];
      map[day]!.push(e);
    });
    return map;
  }, [events]);

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Calendar" />
        <main className="p-8">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-between mb-6">
                <Button variant="ghost" onClick={() => setCurrentDate(new Date(year, month - 1))}><ChevronLeft className="w-5 h-5" /></Button>
                <h2 className="text-xl font-semibold">{monthName}</h2>
                <Button variant="ghost" onClick={() => setCurrentDate(new Date(year, month + 1))}><ChevronRight className="w-5 h-5" /></Button>
              </div>

              {isLoading ? <Loading /> : (
                <div className="grid grid-cols-7 gap-px bg-gray-200 rounded-lg overflow-hidden">
                  {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(d => (
                    <div key={d} className="bg-gray-50 p-2 text-center text-xs font-medium text-gray-500">{d}</div>
                  ))}
                  {Array.from({ length: firstDayOfWeek }).map((_, i) => (
                    <div key={`empty-${i}`} className="bg-white p-2 min-h-[100px]" />
                  ))}
                  {Array.from({ length: daysInMonth }).map((_, i) => {
                    const day = i + 1;
                    const dayEvents = eventsByDay[day] || [];
                    const isToday = new Date().getDate() === day && new Date().getMonth() === month && new Date().getFullYear() === year;
                    return (
                      <div key={day} className={`bg-white p-2 min-h-[100px] ${isToday ? 'ring-2 ring-inset ring-blue-500' : ''}`}>
                        <span className={`text-sm font-medium ${isToday ? 'text-blue-600' : 'text-gray-700'}`}>{day}</span>
                        <div className="mt-1 space-y-1">
                          {dayEvents.slice(0, 3).map(e => (
                            <Link key={e.id} href={`/events/${e.id}`} className="block text-xs p-1 rounded truncate hover:bg-gray-50" style={{ backgroundColor: (e.categoryColor || '#3B82F6') + '15', color: e.categoryColor || '#3B82F6' }}>
                              {e.title}
                            </Link>
                          ))}
                          {dayEvents.length > 3 && <p className="text-xs text-gray-400">+{dayEvents.length - 3} more</p>}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>
        </main>
      </div>
    </AuthGuard>
  );
}
