'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/lib/auth-context';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import { PasswordRequirements } from '@/components/ui/password-requirements';
import { validatePassword } from '@/lib/password-validation';
import { toast } from 'sonner';

export default function RegisterPage() {
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '', department: '' });
  const [isLoading, setIsLoading] = useState(false);
  const { register } = useAuth();
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const passwordError = validatePassword(form.password);
    if (passwordError) {
      toast.error(`Password requirement not met: ${passwordError}`);
      return;
    }
    setIsLoading(true);
    try {
      await register(form.email, form.password, form.firstName, form.lastName, form.department || undefined);
      router.push('/dashboard');
    } catch {
      toast.error('Registration failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, [field]: e.target.value }));

  return (
    <div className="min-h-screen flex items-center justify-center px-4">
      <Card className="w-full max-w-md">
        <CardContent className="pt-8 pb-6">
          <div className="text-center mb-8">
            <h1 className="text-2xl font-bold text-gray-900">Create Account</h1>
            <p className="text-gray-500 mt-1">Join the Internal Portal</p>
          </div>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input label="First Name" id="firstName" value={form.firstName} onChange={update('firstName')} required />
              <Input label="Last Name" id="lastName" value={form.lastName} onChange={update('lastName')} required />
            </div>
            <Input label="Email" id="email" type="email" value={form.email} onChange={update('email')} required />
            <div>
              <Input label="Password" id="password" type="password" value={form.password} onChange={update('password')} required />
              <PasswordRequirements password={form.password} />
            </div>
            <Input label="Department" id="department" value={form.department} onChange={update('department')} />
            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? 'Creating account...' : 'Create Account'}
            </Button>
          </form>
          <p className="text-center text-sm text-gray-500 mt-6">
            Already have an account? <Link href="/login" className="text-blue-600 hover:underline">Sign in</Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
