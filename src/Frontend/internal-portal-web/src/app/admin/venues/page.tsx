'use client';

import Link from 'next/link';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Loading } from '@/components/ui/loading';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/table';
import { useVenues, useDeleteVenue } from '@/hooks/use-venues';
import { toast } from 'sonner';
import { Plus, Trash2, Edit } from 'lucide-react';

export default function AdminVenuesPage() {
  const { data: venues, isLoading } = useVenues();
  const deleteVenue = useDeleteVenue();

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Manage Venues" />
        <main className="p-8">
          <div className="flex justify-end mb-4">
            <Link href="/admin/venues/create"><Button><Plus className="w-4 h-4 mr-2" /> Create Venue</Button></Link>
          </div>
          <Card>
            <CardContent className="pt-6">
              {isLoading ? <Loading /> : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Name</TableHead>
                      <TableHead>Capacity</TableHead>
                      <TableHead>City</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {venues?.map(venue => (
                      <TableRow key={venue.id}>
                        <TableCell className="font-medium">{venue.name}</TableCell>
                        <TableCell>{venue.capacity}</TableCell>
                        <TableCell>{venue.city}, {venue.state}</TableCell>
                        <TableCell className="flex gap-1">
                          <Link href={`/admin/venues/${venue.id}/edit`}><Button variant="ghost" size="sm"><Edit className="w-4 h-4" /></Button></Link>
                          <Button variant="ghost" size="sm" onClick={() => deleteVenue.mutate(venue.id, { onSuccess: () => toast.success('Venue deleted') })}><Trash2 className="w-4 h-4 text-red-500" /></Button>
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
