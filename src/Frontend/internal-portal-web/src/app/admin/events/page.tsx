'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loading } from '@/components/ui/loading';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table';
import { useEvents, useDeleteEvent } from '@/hooks/use-events';
import { formatDateTime } from '@/lib/utils';
import { toast } from 'sonner';
import { Plus, Trash2, Edit } from 'lucide-react';

export default function AdminEventsPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useEvents(page, 20);
  const deleteEvent = useDeleteEvent();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Manage Events" />
        <main className="p-8">
          <div className="flex justify-end mb-4">
            <Link href="/events/create"><Button><Plus className="w-4 h-4 mr-2" /> Create Event</Button></Link>
          </div>
          <Card>
            <CardContent className="pt-6">
              {isLoading ? <Loading /> : (
                <Table>
                  <TableHeader><TableRow><TableHead>Title</TableHead><TableHead>Date</TableHead><TableHead>Status</TableHead><TableHead>Attendees</TableHead><TableHead>Organizer</TableHead><TableHead>Actions</TableHead></TableRow></TableHeader>
                  <TableBody>
                    {data?.items.map(event => (
                      <TableRow key={event.id}>
                        <TableCell className="font-medium">{event.title}</TableCell>
                        <TableCell>{formatDateTime(event.startUtc)}</TableCell>
                        <TableCell><Badge status={event.status}>{event.status}</Badge></TableCell>
                        <TableCell>{event.currentAttendees}/{event.maxAttendees}</TableCell>
                        <TableCell>{event.organizerName}</TableCell>
                        <TableCell className="flex gap-1">
                          {event.status !== 'Completed' && (
                            <>
                              <Link href={`/events/${event.id}/edit`}><Button variant="ghost" size="sm"><Edit className="w-4 h-4" /></Button></Link>
                              <Button variant="ghost" size="sm" onClick={() => deleteEvent.mutate(event.id, { onSuccess: () => toast.success('Event deleted') })}><Trash2 className="w-4 h-4 text-red-500" /></Button>
                            </>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
              {data && data.totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 mt-4">
                  <Button variant="secondary" size="sm" disabled={!data.hasPreviousPage} onClick={() => setPage(p => p - 1)}>Previous</Button>
                  <span className="text-sm text-gray-500">Page {data.pageNumber} of {data.totalPages}</span>
                  <Button variant="secondary" size="sm" disabled={!data.hasNextPage} onClick={() => setPage(p => p + 1)}>Next</Button>
                </div>
              )}
            </CardContent>
          </Card>
        </main>
      </div>
    </AuthGuard>
  );
}
