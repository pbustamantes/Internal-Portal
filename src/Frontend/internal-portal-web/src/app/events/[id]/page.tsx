'use client';

import { use } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loading } from '@/components/ui/loading';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table';
import { useEvent, useEventAttendees, usePublishEvent, useCancelEvent, useDeleteEvent } from '@/hooks/use-events';
import { useRegisterForEvent, useCancelRegistration } from '@/hooks/use-registrations';
import { useAuth } from '@/lib/auth-context';
import { formatDateTime } from '@/lib/utils';
import { toast } from 'sonner';
import { MapPin, Clock, Users, Edit, Trash2 } from 'lucide-react';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';

export default function EventDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const { user } = useAuth();
  const { data: event, isLoading } = useEvent(id);
  const { data: attendees } = useEventAttendees(id);
  const publishEvent = usePublishEvent();
  const cancelEvent = useCancelEvent();
  const deleteEvent = useDeleteEvent();
  const registerForEvent = useRegisterForEvent();
  const cancelRegistration = useCancelRegistration();

  if (isLoading) return <AuthGuard><Sidebar /><div className="ml-64"><Loading /></div></AuthGuard>;
  if (!event) return null;

  const isOwner = user?.id === event.organizerId || user?.role === 'Admin';
  const isRegistered = attendees?.some(a => a.userId === user?.id) ?? false;
  const isPast = event.status === 'Completed' || new Date(event.endUtc) < new Date();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title={event.title} />
        <main className="p-8">
          <div className="flex gap-3 mb-6">
            {isOwner && event.status === 'Draft' && !isPast && (
              <Button onClick={() => publishEvent.mutate(id, { onSuccess: () => toast.success('Event published') })}>Publish</Button>
            )}
            {isOwner && event.status !== 'Cancelled' && !isPast && (
              <>
                <Link href={`/events/${id}/edit`}><Button variant="secondary"><Edit className="w-4 h-4 mr-2" /> Edit</Button></Link>
                <Button variant="danger" onClick={() => cancelEvent.mutate(id, { onSuccess: () => toast.success('Event cancelled') })}>Cancel Event</Button>
                <Button variant="ghost" onClick={() => deleteEvent.mutate(id, { onSuccess: () => { toast.success('Event deleted'); router.push('/events'); } })}><Trash2 className="w-4 h-4" /></Button>
              </>
            )}
            {event.status === 'Published' && !isOwner && !isPast && (
              <Button disabled={isRegistered} onClick={() => registerForEvent.mutate(id, { onSuccess: () => toast.success('Registered!'), onError: () => toast.error('Registration failed') })}>{isRegistered ? 'Already Registered' : 'Register for Event'}</Button>
            )}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div className="lg:col-span-2 space-y-6">
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-4">
                    <Badge status={event.status}>{event.status}</Badge>
                    {event.categoryName && <span className="text-sm text-gray-500">{event.categoryName}</span>}
                  </div>
                  {event.description && <p className="text-gray-700 mb-6">{event.description}</p>}
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div className="flex items-center gap-2 text-gray-600"><Clock className="w-4 h-4" /> {formatDateTime(event.startUtc)} - {formatDateTime(event.endUtc)}</div>
                    {event.locationCity && <div className="flex items-center gap-2 text-gray-600"><MapPin className="w-4 h-4" /> {event.locationCity}, {event.locationState}</div>}
                    <div className="flex items-center gap-2 text-gray-600"><Users className="w-4 h-4" /> {event.currentAttendees}/{event.maxAttendees} attendees</div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader><CardTitle>Attendees</CardTitle></CardHeader>
                <CardContent>
                  {attendees?.length === 0 ? <p className="text-gray-500 text-sm">No attendees yet</p> : (
                    <Table>
                      <TableHeader><TableRow><TableHead>Name</TableHead><TableHead>Email</TableHead><TableHead>Status</TableHead><TableHead>Registered</TableHead></TableRow></TableHeader>
                      <TableBody>
                        {attendees?.map(a => (
                          <TableRow key={a.userId}>
                            <TableCell>
                              <div className="flex items-center gap-3">
                                {a.profilePictureUrl ? (
                                  <img src={`${API_BASE_URL}${a.profilePictureUrl}`} alt="" className="w-8 h-8 rounded-full object-cover shrink-0" />
                                ) : (
                                  <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-semibold shrink-0">
                                    {a.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)}
                                  </div>
                                )}
                                <span className="font-medium">{a.fullName}</span>
                              </div>
                            </TableCell>
                            <TableCell>{a.email}</TableCell>
                            <TableCell><Badge status={a.registrationStatus}>{a.registrationStatus}</Badge></TableCell>
                            <TableCell>{formatDateTime(a.registeredAtUtc)}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  )}
                </CardContent>
              </Card>
            </div>

            <div>
              <Card>
                <CardHeader><CardTitle>Details</CardTitle></CardHeader>
                <CardContent className="space-y-3 text-sm">
                  <div><span className="text-gray-500">Organizer:</span> <span className="font-medium">{event.organizerName}</span></div>
                  <div><span className="text-gray-500">Recurrence:</span> <span className="font-medium">{event.recurrence}</span></div>
                  {event.venueName && <div><span className="text-gray-500">Venue:</span> <span className="font-medium">{event.venueName}</span></div>}
                  {event.locationBuilding && <div><span className="text-gray-500">Building:</span> <span className="font-medium">{event.locationBuilding}</span></div>}
                  {event.locationRoom && <div><span className="text-gray-500">Room:</span> <span className="font-medium">{event.locationRoom}</span></div>}
                </CardContent>
              </Card>
            </div>
          </div>
        </main>
      </div>
    </AuthGuard>
  );
}
