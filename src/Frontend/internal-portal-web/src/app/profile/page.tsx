'use client';

import { useState, useEffect, useRef } from 'react';
import { Sidebar } from '@/components/layout/sidebar';
import { Header } from '@/components/layout/header';
import { AuthGuard } from '@/components/layout/auth-guard';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/lib/auth-context';
import { PasswordRequirements } from '@/components/ui/password-requirements';
import { validatePassword } from '@/lib/password-validation';
import api from '@/lib/api';
import { toast } from 'sonner';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';

export default function ProfilePage() {
  const { user, refreshUser } = useAuth();
  const [form, setForm] = useState({ firstName: '', lastName: '', department: '' });
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [changingPassword, setChangingPassword] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (user) setForm({ firstName: user.firstName, lastName: user.lastName, department: user.department || '' });
  }, [user]);

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.put('/users/me', form);
      toast.success('Profile updated');
    } catch {
      toast.error('Failed to update profile');
    } finally {
      setSaving(false);
    }
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      await api.post('/users/me/profile-picture', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      await refreshUser();
      toast.success('Profile picture updated');
    } catch {
      toast.error('Failed to upload profile picture');
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleRemovePicture = async () => {
    setUploading(true);
    try {
      await api.delete('/users/me/profile-picture');
      await refreshUser();
      toast.success('Profile picture removed');
    } catch {
      toast.error('Failed to remove profile picture');
    } finally {
      setUploading(false);
    }
  };

  const updatePassword = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => setPasswordForm(f => ({ ...f, [field]: e.target.value }));

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      toast.error('New passwords do not match');
      return;
    }
    const passwordError = validatePassword(passwordForm.newPassword);
    if (passwordError) {
      toast.error(`Password requirement not met: ${passwordError}`);
      return;
    }
    setChangingPassword(true);
    try {
      await api.post('/auth/change-password', {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      });
      toast.success('Password changed successfully');
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch {
      toast.error('Failed to change password. Check your current password.');
    } finally {
      setChangingPassword(false);
    }
  };

  const initials = user ? `${user.firstName[0] || ''}${user.lastName[0] || ''}`.toUpperCase() : '';
  const pictureUrl = user?.profilePictureUrl ? `${API_BASE_URL}${user.profilePictureUrl}` : null;

  return (
    <AuthGuard>
      <Sidebar />
      <div className="ml-64">
        <Header title="Profile" />
        <main className="p-8 max-w-2xl">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Profile Information</CardTitle>
                <Badge>{user?.role}</Badge>
              </div>
            </CardHeader>
            <CardContent>
              <div className="flex flex-col items-center mb-6">
                <div className="relative">
                  {pictureUrl ? (
                    <img
                      src={pictureUrl}
                      alt="Profile"
                      className="w-24 h-24 rounded-full object-cover border-2 border-gray-200"
                    />
                  ) : (
                    <div className="w-24 h-24 rounded-full bg-blue-600 flex items-center justify-center text-white text-2xl font-semibold border-2 border-gray-200">
                      {initials}
                    </div>
                  )}
                </div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".jpg,.jpeg,.png,.gif,.webp"
                  className="hidden"
                  onChange={handleFileSelect}
                />
                <div className="flex gap-2 mt-3">
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    disabled={uploading}
                    onClick={() => fileInputRef.current?.click()}
                  >
                    {uploading ? 'Uploading...' : 'Change Photo'}
                  </Button>
                  {pictureUrl && (
                    <Button
                      type="button"
                      variant="secondary"
                      size="sm"
                      disabled={uploading}
                      onClick={handleRemovePicture}
                    >
                      Remove
                    </Button>
                  )}
                </div>
              </div>
              <form onSubmit={handleSubmit} className="space-y-4">
                <Input label="Email" value={user?.email || ''} disabled />
                <div className="grid grid-cols-2 gap-4">
                  <Input label="First Name" id="firstName" value={form.firstName} onChange={update('firstName')} required />
                  <Input label="Last Name" id="lastName" value={form.lastName} onChange={update('lastName')} required />
                </div>
                <Input label="Department" id="department" value={form.department} onChange={update('department')} />
                <Button type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save Changes'}</Button>
              </form>
            </CardContent>
          </Card>

          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Change Password</CardTitle>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleChangePassword} className="space-y-4">
                <Input label="Current Password" id="currentPassword" type="password" value={passwordForm.currentPassword} onChange={updatePassword('currentPassword')} required />
                <div>
                  <Input label="New Password" id="newPassword" type="password" value={passwordForm.newPassword} onChange={updatePassword('newPassword')} required />
                  <PasswordRequirements password={passwordForm.newPassword} />
                </div>
                <Input label="Confirm New Password" id="confirmPassword" type="password" value={passwordForm.confirmPassword} onChange={updatePassword('confirmPassword')} required />
                <Button type="submit" disabled={changingPassword}>{changingPassword ? 'Changing...' : 'Change Password'}</Button>
              </form>
            </CardContent>
          </Card>
        </main>
      </div>
    </AuthGuard>
  );
}
