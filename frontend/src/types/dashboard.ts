export interface ServiceStats {
  serviceName: string;
  totalLogs: number;
  errorCount: number;
  warnCount: number;
  infoCount: number;
  errorRate: number;
}

export interface VolumePoint {
  timestamp: string;
  total: number;
  errors: number;
  warnings: number;
  info: number;
}

export interface DashboardStats {
  totalLogs: number;
  totalIncidents: number;
  totalAlerts: number;
  activeIncidents: number;
  serviceStats: ServiceStats[];
  volumeOverTime: VolumePoint[];
}

export interface Incident {
  id: string;
  serviceName: string;
  messagePattern: string;
  occurrenceCount: number;
  firstSeen: string;
  lastSeen: string;
  isResolved: boolean;
}

export interface Alert {
  id: string;
  serviceName: string;
  description: string;
  triggeredAt: string;
  isAcknowledged: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LogEntry {
  id: string;
  serviceName: string;
  level: string;
  message: string;
  timestamp: string;
}