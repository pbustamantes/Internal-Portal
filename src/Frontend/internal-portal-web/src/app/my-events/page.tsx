'use client';

import Link from 'next/link';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Loading } from '@/components/ui/loading';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table';
import { useMyRegistrations } from '@/hooks/use-registrations';
import { useCancelRegistration } from '@/hooks/use-registrations';
import { formatDateTime } from '@/lib/utils';
import { toast } from 'sonner';

export default function MyEventsPage() {
  const { data: registrations, isLoading } = useMyRegistrations();
  const cancelReg = useCancelRegistration();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="My Events" />
        <main className="p-8">
          <Card>
            <CardContent className="pt-6">
              {isLoading ? <Loading /> : registrations?.length === 0 ? (
                <div className="text-center py-12">
                  <p className="text-gray-500 mb-4">You haven&apos;t registered for any events yet.</p>
                  <Link href="/events"><Button>Browse Events</Button></Link>
                </div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Event</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Registered</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {registrations?.map(reg => (
                      <TableRow key={reg.id}>
                        <TableCell><Link href={`/events/${reg.eventId}`} className="font-medium text-blue-600 hover:underline">{reg.eventTitle}</Link></TableCell>
                        <TableCell><Badge status={reg.status}>{reg.status}</Badge></TableCell>
                        <TableCell>{formatDateTime(reg.registeredAtUtc)}</TableCell>
                        <TableCell>
                          {reg.status !== 'Cancelled' && (
                            <Button variant="ghost" size="sm" onClick={() => cancelReg.mutate(reg.eventId, { onSuccess: () => toast.success('Registration cancelled') })}>Cancel</Button>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </main>
      </div>
    </AuthGuard>
  );
}
