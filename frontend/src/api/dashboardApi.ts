import axios from 'axios';
import type { DashboardStats, Incident, Alert, PagedResult, LogEntry } from '../types/dashboard';

// Point at your running API — vite proxy handles this in dev
const client = axios.create({
  baseURL: 'http://localhost:5153/api',
  timeout: 10_000,
});

export const dashboardApi = {
  getStats: () =>
    client.get<DashboardStats>('/dashboard/stats').then(r => r.data),

  getIncidents: () =>
    client.get<Incident[]>('/dashboard/incidents').then(r => r.data),

  getAlerts: (count = 20) =>
    client.get<Alert[]>(`/dashboard/alerts?count=${count}`).then(r => r.data),

  getLogs: (page = 1, pageSize = 20, service?: string, level?: string) => {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
      ...(service && { service }),
      ...(level && { level }),
    });
    return client
      .get<PagedResult<LogEntry>>(`/dashboard/logs?${params}`)
      .then(r => r.data);
  },
};