'use client';

import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react';
import api from '@/lib/api';
import { getAccessToken, setAccessToken } from '@/lib/token-store';
import { queryClient } from '@/lib/query-provider';
import { useNotificationStore } from '@/lib/notification-store';
import type { User, AuthResponse } from '@/types';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, firstName: string, lastName: string, department?: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadUser = useCallback(async () => {
    try {
      // Try to refresh the session via cookie — this recovers the access token
      // after a page reload since the in-memory token is lost
      const { data: refreshData } = await api.post<AuthResponse>('/auth/refresh', {});
      setAccessToken(refreshData.accessToken);

      const { data } = await api.get<User>('/users/me');
      setUser(data);
    } catch {
      setAccessToken(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const refreshUser = useCallback(async () => {
    try {
      const { data } = await api.get<User>('/users/me');
      setUser(data);
    } catch { /* ignore */ }
  }, []);

  useEffect(() => { loadUser(); }, [loadUser]);

  const login = async (email: string, password: string) => {
    const { data } = await api.post<AuthResponse>('/auth/login', { email, password });
    setAccessToken(data.accessToken);
    setUser(data.user);
  };

  const register = async (email: string, password: string, firstName: string, lastName: string, department?: string) => {
    const { data } = await api.post<AuthResponse>('/auth/register', { email, password, firstName, lastName, department });
    setAccessToken(data.accessToken);
    setUser(data.user);
  };

  const logout = async () => {
    try {
      await api.post('/auth/logout');
    } finally {
      setAccessToken(null);
      setUser(null);
      queryClient.clear();
      useNotificationStore.getState().clearStore();
    }
  };

  return (
    <AuthContext.Provider value={{ user, isLoading, isAuthenticated: !!user, login, register, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
