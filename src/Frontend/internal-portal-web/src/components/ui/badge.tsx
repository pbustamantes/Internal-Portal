import { cn, getStatusColor } from '@/lib/utils';
import type { HTMLAttributes } from 'react';

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  status?: string;
}

export function Badge({ className, status, children, ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        status ? getStatusColor(status) : 'bg-gray-100 text-gray-700',
        className
      )}
      {...props}
    >
      {children ?? status}
    </span>
  );
}
