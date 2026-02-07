'use client';

import { useState, useEffect, use } from 'react';
import { useRouter } from 'next/navigation';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Loading } from '@/components/ui/loading';
import { useVenue, useUpdateVenue } from '@/hooks/use-venues';
import { toast } from 'sonner';

export default function EditVenuePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const { data: venue, isLoading } = useVenue(id);
  const updateVenue = useUpdateVenue();
  const [form, setForm] = useState({
    name: '', capacity: 0,
    street: '', city: '', state: '', zipCode: '',
    building: '', room: '',
  });

  useEffect(() => {
    if (venue) {
      setForm({
        name: venue.name,
        capacity: venue.capacity,
        street: venue.street,
        city: venue.city,
        state: venue.state,
        zipCode: venue.zipCode,
        building: venue.building || '',
        room: venue.room || '',
      });
    }
  }, [venue]);

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await updateVenue.mutateAsync({
        id,
        ...form,
        capacity: Number(form.capacity),
        building: form.building || undefined,
        room: form.room || undefined,
      });
      toast.success('Venue updated!');
      router.push('/admin/venues');
    } catch {
      toast.error('Failed to update venue');
    }
  };

  if (isLoading) return <AuthGuard><Sidebar /><div className="ml-64"><Header title="Edit Venue" /><main className="p-8"><Loading /></main></div></AuthGuard>;

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Edit Venue" />
        <main className="p-8 max-w-3xl">
          <Card>
            <CardContent className="pt-6">
              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <Input label="Name" id="name" value={form.name} onChange={update('name')} required />
                  <Input label="Capacity" id="capacity" type="number" value={form.capacity} onChange={update('capacity')} min={1} required />
                </div>
                <Input label="Street" id="street" value={form.street} onChange={update('street')} required />
                <div className="grid grid-cols-2 gap-4">
                  <Input label="City" id="city" value={form.city} onChange={update('city')} required />
                  <Input label="State" id="state" value={form.state} onChange={update('state')} required />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <Input label="Zip Code" id="zipCode" value={form.zipCode} onChange={update('zipCode')} required />
                  <Input label="Building" id="building" value={form.building} onChange={update('building')} />
                </div>
                <Input label="Room" id="room" value={form.room} onChange={update('room')} />
                <div className="flex gap-3">
                  <Button type="submit" disabled={updateVenue.isPending}>{updateVenue.isPending ? 'Saving...' : 'Save Changes'}</Button>
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
