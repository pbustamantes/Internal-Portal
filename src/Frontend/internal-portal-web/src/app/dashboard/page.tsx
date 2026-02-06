'use client';

import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Loading } from '@/components/ui/loading';
import { useUpcomingEvents } from '@/hooks/use-events';
import { useNotifications } from '@/hooks/use-notifications';
import { formatDateTime, formatTimeAgo } from '@/lib/utils';
import Link from 'next/link';
import { Calendar, Users, Bell, Ticket } from 'lucide-react';

export default function DashboardPage() {
  const { data: events, isLoading: eventsLoading } = useUpcomingEvents(5);
  const { data: notifications, isLoading: notifLoading } = useNotifications();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Dashboard" />
        <main className="p-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            {[
              { label: 'Upcoming Events', value: events?.length ?? 0, icon: Calendar, color: 'text-blue-600 bg-blue-50' },
              { label: 'Notifications', value: notifications?.filter(n => !n.isRead).length ?? 0, icon: Bell, color: 'text-yellow-600 bg-yellow-50' },
            ].map(stat => (
              <Card key={stat.label}>
                <CardContent className="flex items-center gap-4 py-4">
                  <div className={`p-3 rounded-lg ${stat.color}`}><stat.icon className="w-6 h-6" /></div>
                  <div>
                    <p className="text-2xl font-bold">{stat.value}</p>
                    <p className="text-sm text-gray-500">{stat.label}</p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <Card>
              <CardHeader><CardTitle>Upcoming Events</CardTitle></CardHeader>
              <CardContent>
                {eventsLoading ? <Loading /> : events?.length === 0 ? (
                  <p className="text-gray-500 text-sm">No upcoming events</p>
                ) : (
                  <div className="space-y-3">
                    {events?.map(event => (
                      <Link key={event.id} href={`/events/${event.id}`} className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-50 transition-colors">
                        <div>
                          <p className="font-medium text-gray-900">{event.title}</p>
                          <p className="text-sm text-gray-500">{formatDateTime(event.startUtc)}</p>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="text-sm text-gray-500 flex items-center gap-1">
                            <Users className="w-4 h-4" /> {event.currentAttendees}/{event.maxAttendees}
                          </span>
                          <Badge status={event.status}>{event.status}</Badge>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader><CardTitle>Recent Notifications</CardTitle></CardHeader>
              <CardContent>
                {notifLoading ? <Loading /> : notifications?.length === 0 ? (
                  <p className="text-gray-500 text-sm">No notifications</p>
                ) : (
                  <div className="space-y-3">
                    {notifications?.slice(0, 5).map(notif => (
                      <div key={notif.id} className={`p-3 rounded-lg ${notif.isRead ? 'bg-white' : 'bg-blue-50'}`}>
                        <p className="font-medium text-sm">{notif.title}</p>
                        <p className="text-sm text-gray-500">{notif.message}</p>
                        <p className="text-xs text-gray-400 mt-1">{formatTimeAgo(notif.createdAtUtc)}</p>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
