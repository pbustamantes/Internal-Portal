'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Loading } from '@/components/ui/loading';
import { useEvents } from '@/hooks/use-events';
import { useAuth } from '@/lib/auth-context';
import { formatDateTime } from '@/lib/utils';
import { Plus, Users, Search, ArrowUpDown } from 'lucide-react';

export default function EventsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const { data, isLoading } = useEvents(page, 10, search || undefined, undefined, 'date', sortOrder);
  const { user } = useAuth();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Events" />
        <main className="p-8">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <div className="relative w-80">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <Input className="pl-10" placeholder="Search events..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} />
              </div>
              <Button variant="secondary" size="sm" onClick={() => { setSortOrder(o => o === 'asc' ? 'desc' : 'asc'); setPage(1); }} title={sortOrder === 'asc' ? 'Showing earliest first' : 'Showing latest first'}>
                <ArrowUpDown className="w-4 h-4 mr-2" />
                {sortOrder === 'asc' ? 'Earliest first' : 'Latest first'}
              </Button>
            </div>
            {(user?.role === 'Organizer' || user?.role === 'Admin') && (
              <Link href="/events/create"><Button><Plus className="w-4 h-4 mr-2" /> Create Event</Button></Link>
            )}
          </div>

          {isLoading ? <Loading /> : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {data?.items.map(event => (
                  <Link key={event.id} href={`/events/${event.id}`}>
                    <Card className="hover:shadow-md transition-shadow cursor-pointer h-full">
                      <CardContent className="pt-5">
                        <div className="flex items-start justify-between mb-3">
                          <div className="flex items-center gap-2">
                            {event.categoryName && (
                              <span className="text-xs px-2 py-0.5 rounded-full" style={{ backgroundColor: event.categoryColor + '20', color: event.categoryColor ?? undefined }}>{event.categoryName}</span>
                            )}
                            <Badge status={event.status}>{event.status}</Badge>
                          </div>
                        </div>
                        <h3 className="font-semibold text-gray-900 mb-2">{event.title}</h3>
                        <p className="text-sm text-gray-500 mb-3">{formatDateTime(event.startUtc)}</p>
                        <div className="flex items-center justify-between text-sm text-gray-500">
                          <span className="flex items-center gap-1"><Users className="w-4 h-4" /> {event.currentAttendees}/{event.maxAttendees}</span>
                          <span>by {event.organizerName}</span>
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                ))}
              </div>

              {data && data.totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 mt-8">
                  <Button variant="secondary" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage(p => p - 1)}>Previous</Button>
                  <span className="text-sm text-gray-500">Page {data.pageNumber} of {data.totalPages}</span>
                  <Button variant="secondary" size="sm" disabled={!data.hasNextPage} onClick={() => setPage(p => p + 1)}>Next</Button>
                </div>
              )}
            </>
          )}
        </main>
      </div>
    </AuthGuard>
  );
}
