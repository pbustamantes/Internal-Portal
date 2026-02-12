'use client';

import { PASSWORD_REQUIREMENTS } from '@/lib/password-validation';

interface PasswordRequirementsProps {
  password: string;
}

export function PasswordRequirements({ password }: PasswordRequirementsProps) {
  const hasInput = password.length > 0;

  return (
    <ul className="text-sm space-y-1 mt-1">
      {PASSWORD_REQUIREMENTS.map((req) => {
        const passed = req.test(password);
        let className = 'text-gray-400';
        let icon = '\u2022';

        if (hasInput) {
          if (passed) {
            className = 'text-green-600';
            icon = '\u2713';
          } else {
            className = 'text-red-500';
            icon = '\u2717';
          }
        }

        return (
          <li key={req.label} className={className}>
            {icon} {req.label}
          </li>
        );
      })}
    </ul>
  );
}
