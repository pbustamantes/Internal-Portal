'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useCreateEvent } from '@/hooks/use-events';
import { useVenues } from '@/hooks/use-venues';
import { toast } from 'sonner';

export default function CreateEventPage() {
  const router = useRouter();
  const createEvent = useCreateEvent();
  const { data: venues } = useVenues();
  const [form, setForm] = useState({
    title: '', description: '', startUtc: '', endUtc: '',
    minAttendees: 0, maxAttendees: 50,
    street: '', city: '', state: '', zipCode: '', building: '', room: '',
    venueId: '',
  });

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await createEvent.mutateAsync({
        ...form,
        minAttendees: Number(form.minAttendees),
        maxAttendees: Number(form.maxAttendees),
        recurrence: 0,
        venueId: form.venueId || undefined,
      });
      toast.success('Event created!');
      router.push(`/events/${result.id}`);
    } catch {
      toast.error('Failed to create event');
    }
  };

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Create Event" />
        <main className="p-8 max-w-3xl">
          <Card>
            <CardContent className="pt-6">
              <form onSubmit={handleSubmit} className="space-y-6">
                <Input label="Title" id="title" value={form.title} onChange={update('title')} required />
                <div>
                  <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <textarea id="description" value={form.description} onChange={update('description')} rows={4}
                    className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <Input label="Start Date & Time" id="startUtc" type="datetime-local" value={form.startUtc} onChange={update('startUtc')} required />
                  <Input label="End Date & Time" id="endUtc" type="datetime-local" value={form.endUtc} onChange={update('endUtc')} required />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <Input label="Min Attendees" id="minAttendees" type="number" value={form.minAttendees} onChange={update('minAttendees')} min={0} />
                  <Input label="Max Attendees" id="maxAttendees" type="number" value={form.maxAttendees} onChange={update('maxAttendees')} min={1} required />
                </div>
                <div className="border-t pt-4">
                  <h3 className="text-sm font-medium text-gray-700 mb-3">Location (optional)</h3>
                  <div className="mb-4">
                    <label htmlFor="venueId" className="block text-sm font-medium text-gray-700 mb-1">Venue</label>
                    <select id="venueId" value={form.venueId}
                      onChange={e => setForm(f => ({ ...f, venueId: e.target.value }))}
                      className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                      <option value="">No venue</option>
                      {venues?.map(v => (
                        <option key={v.id} value={v.id}>{v.name} (capacity: {v.capacity})</option>
                      ))}
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <Input label="Street" id="street" value={form.street} onChange={update('street')} />
                    <Input label="City" id="city" value={form.city} onChange={update('city')} />
                    <Input label="State" id="state" value={form.state} onChange={update('state')} />
                    <Input label="Zip Code" id="zipCode" value={form.zipCode} onChange={update('zipCode')} />
                    <Input label="Building" id="building" value={form.building} onChange={update('building')} />
                    <Input label="Room" id="room" value={form.room} onChange={update('room')} />
                  </div>
                </div>
                <div className="flex gap-3">
                  <Button type="submit" disabled={createEvent.isPending}>{createEvent.isPending ? 'Creating...' : 'Create Event'}</Button>
                  <Button type="button" variant="secondary" onClick={() => router.back()}>Cancel</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </main>
      </div>
    </AuthGuard>
  );
}
