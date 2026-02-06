'use client';

import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Loading } from '@/components/ui/loading';
import { useNotifications, useMarkNotificationRead } from '@/hooks/use-notifications';
import { formatTimeAgo } from '@/lib/utils';
import { Bell, Check } from 'lucide-react';

export default function NotificationsPage() {
  const { data: notifications, isLoading } = useNotifications();
  const markRead = useMarkNotificationRead();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Notifications" />
        <main className="p-8 max-w-3xl">
          {isLoading ? <Loading /> : notifications?.length === 0 ? (
            <Card><CardContent className="text-center py-12"><Bell className="w-12 h-12 text-gray-300 mx-auto mb-4" /><p className="text-gray-500">No notifications</p></CardContent></Card>
          ) : (
            <div className="space-y-3">
              {notifications?.map(notif => (
                <Card key={notif.id} className={notif.isRead ? 'opacity-60' : ''}>
                  <CardContent className="flex items-start justify-between py-4">
                    <div>
                      <p className="font-medium text-gray-900">{notif.title}</p>
                      <p className="text-sm text-gray-500 mt-1">{notif.message}</p>
                      <p className="text-xs text-gray-400 mt-2">{formatTimeAgo(notif.createdAtUtc)}</p>
                    </div>
                    {!notif.isRead && (
                      <Button variant="ghost" size="sm" onClick={() => markRead.mutate(notif.id)}><Check className="w-4 h-4" /></Button>
                    )}
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </main>
      </div>
    </AuthGuard>
  );
}
