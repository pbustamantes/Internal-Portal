'use client';

import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Loading } from '@/components/ui/loading';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table';
import { useAttendanceReport, useMonthlyReport, usePopularEvents } from '@/hooks/use-reports';
import { formatDate } from '@/lib/utils';

export default function ReportsPage() {
  const { data: attendance, isLoading: attLoading } = useAttendanceReport();
  const { data: monthly, isLoading: monLoading } = useMonthlyReport();
  const { data: popular, isLoading: popLoading } = usePopularEvents();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Reports" />
        <main className="p-8 space-y-8">
          <Card>
            <CardHeader><CardTitle>Popular Events</CardTitle></CardHeader>
            <CardContent>
              {popLoading ? <Loading /> : (
                <Table>
                  <TableHeader><TableRow><TableHead>Event</TableHead><TableHead>Category</TableHead><TableHead>Registrations</TableHead><TableHead>Capacity</TableHead><TableHead>Fill Rate</TableHead></TableRow></TableHeader>
                  <TableBody>
                    {popular?.map(e => (
                      <TableRow key={e.eventId}><TableCell className="font-medium">{e.title}</TableCell><TableCell>{e.categoryName || '-'}</TableCell><TableCell>{e.registrationCount}</TableCell><TableCell>{e.maxAttendees}</TableCell><TableCell>{e.fillRate}%</TableCell></TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle>Event Attendance</CardTitle></CardHeader>
            <CardContent>
              {attLoading ? <Loading /> : (
                <Table>
                  <TableHeader><TableRow><TableHead>Event</TableHead><TableHead>Date</TableHead><TableHead>Confirmed</TableHead><TableHead>Cancelled</TableHead><TableHead>Waitlisted</TableHead><TableHead>Rate</TableHead></TableRow></TableHeader>
                  <TableBody>
                    {attendance?.map(e => (
                      <TableRow key={e.eventId}><TableCell className="font-medium">{e.eventTitle}</TableCell><TableCell>{formatDate(e.startUtc)}</TableCell><TableCell>{e.confirmedCount}</TableCell><TableCell>{e.cancelledCount}</TableCell><TableCell>{e.waitlistedCount}</TableCell><TableCell>{e.attendanceRate}%</TableCell></TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle>Monthly Summary</CardTitle></CardHeader>
            <CardContent>
              {monLoading ? <Loading /> : (
                <Table>
                  <TableHeader><TableRow><TableHead>Month</TableHead><TableHead>Total Events</TableHead><TableHead>Total Registrations</TableHead><TableHead>Avg Attendees</TableHead></TableRow></TableHeader>
                  <TableBody>
                    {monthly?.map(m => (
                      <TableRow key={m.month}><TableCell>{new Date(m.year, m.month - 1).toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}</TableCell><TableCell>{m.totalEvents}</TableCell><TableCell>{m.totalRegistrations}</TableCell><TableCell>{m.averageAttendees}</TableCell></TableRow>
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
