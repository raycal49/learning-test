import { useState, useEffect, useCallback } from 'react';
import { dashboardApi } from '../api/dashboardApi';
import type { DashboardStats, Incident, Alert } from '../types/dashboard';

// How often we refresh — 10s is frequent enough to feel live
// without hammering the API
const POLL_INTERVAL_MS = 10_000;

interface DashboardData {
  stats: DashboardStats | null;
  incidents: Incident[];
  alerts: Alert[];
  loading: boolean;
  error: string | null;
  lastUpdated: Date | null;
}

export function useDashboard(): DashboardData {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      // Fetch everything in parallel — no point waiting sequentially
      const [statsData, incidentsData, alertsData] = await Promise.all([
        dashboardApi.getStats(),
        dashboardApi.getIncidents(),
        dashboardApi.getAlerts(20),
      ]);

      setStats(statsData);
      setIncidents(incidentsData);
      setAlerts(alertsData);
      setError(null);
      setLastUpdated(new Date());
    } catch (err) {
      setError('Failed to fetch dashboard data. Is the API running?');
      console.error('Dashboard fetch error:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAll();
    const interval = setInterval(fetchAll, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [fetchAll]);

  return { stats, incidents, alerts, loading, error, lastUpdated };
}