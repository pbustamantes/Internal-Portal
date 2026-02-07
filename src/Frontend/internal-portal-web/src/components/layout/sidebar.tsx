'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';
import { cn } from '@/lib/utils';
import { Calendar, LayoutDashboard, Bell, User, Ticket, CalendarDays, Settings, BarChart3, Users, LogOut } from 'lucide-react';

const mainNav = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/events', label: 'Events', icon: Ticket },
  { href: '/calendar', label: 'Calendar', icon: CalendarDays },
  { href: '/my-events', label: 'My Events', icon: Calendar },
  { href: '/notifications', label: 'Notifications', icon: Bell },
  { href: '/profile', label: 'Profile', icon: User },
];

const adminNav = [
  { href: '/admin/events', label: 'Manage Events', icon: Settings },
  { href: '/admin/users', label: 'Manage Users', icon: Users },
  { href: '/admin/reports', label: 'Reports', icon: BarChart3 },
];

export function Sidebar() {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  return (
    <aside className="fixed left-0 top-0 h-full w-64 bg-white border-r border-gray-200 flex flex-col z-40">
      <div className="px-6 py-5 border-b border-gray-100">
        <h1 className="text-xl font-bold text-blue-600">Internal Portal</h1>
        <p className="text-xs text-gray-500 mt-1">Event Management</p>
      </div>

      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        {mainNav.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              pathname === item.href || pathname.startsWith(item.href + '/')
                ? 'bg-blue-50 text-blue-700'
                : 'text-gray-600 hover:bg-gray-50'
            )}
          >
            <item.icon className="w-5 h-5" />
            {item.label}
          </Link>
        ))}

        {user?.role === 'Admin' && (
          <>
            <div className="pt-4 pb-2 px-3"><p className="text-xs font-semibold text-gray-400 uppercase">Admin</p></div>
            {adminNav.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  'flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                  pathname.startsWith(item.href) ? 'bg-blue-50 text-blue-700' : 'text-gray-600 hover:bg-gray-50'
                )}
              >
                <item.icon className="w-5 h-5" />
                {item.label}
              </Link>
            ))}
          </>
        )}
      </nav>

      <div className="px-3 py-4 border-t border-gray-100">
        <div className="flex items-center gap-3 px-3 py-2 mb-2">
          {user?.profilePictureUrl ? (
            <img
              src={`${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'}${user.profilePictureUrl}`}
              alt=""
              className="w-9 h-9 rounded-full object-cover shrink-0"
            />
          ) : (
            <div className="w-9 h-9 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-semibold shrink-0">
              {user ? `${user.firstName[0] || ''}${user.lastName[0] || ''}`.toUpperCase() : ''}
            </div>
          )}
          <div className="min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">{user?.firstName} {user?.lastName}</p>
            <p className="text-xs text-gray-500 truncate">{user?.email}</p>
          </div>
        </div>
        <button
          onClick={logout}
          className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium text-gray-600 hover:bg-gray-50 w-full transition-colors"
        >
          <LogOut className="w-5 h-5" />
          Logout
        </button>
      </div>
    </aside>
  );
}
