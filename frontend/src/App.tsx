import { useDashboard } from './hooks/useDashboard';
import { StatCard } from './components/StatCard';
import { LogVolumeChart } from './components/LogVolumeChart';
import { ErrorRateChart } from './components/ErrorRateChart';
import { IncidentsTable } from './components/IncidentsTable';
import { AlertsFeed } from './components/AlertsFeed';
import './index.css';

export default function App() {
  const { stats, incidents, alerts, loading, error, lastUpdated } = useDashboard();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top nav */}
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div>
            <h1 className="text-lg font-semibold text-gray-900">
              Incident Monitor
            </h1>
            <p className="text-xs text-gray-400 mt-0.5">
              Real-time log analysis and anomaly detection
            </p>
          </div>
          <div className="flex items-center gap-3">
            {/* Live indicator */}
            <span className="flex items-center gap-1.5 text-xs text-gray-400">
              <span className="h-1.5 w-1.5 rounded-full bg-green-400 animate-pulse" />
              Live
            </span>
            {lastUpdated && (
              <span className="text-xs text-gray-400">
                Updated {lastUpdated.toLocaleTimeString()}
              </span>
            )}
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-6 py-6 space-y-6">

        {/* Error banner */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {/* Stat cards */}
        {loading && !stats ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="h-24 bg-white rounded-lg shadow-sm animate-pulse" />
            ))}
          </div>
        ) : stats ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <StatCard
              label="Total logs"
              value={stats.totalLogs.toLocaleString()}
              accent="blue"
            />
            <StatCard
              label="Active incidents"
              value={stats.activeIncidents}
              accent="red"
            />
            <StatCard
              label="Total alerts"
              value={stats.totalAlerts.toLocaleString()}
              accent="yellow"
            />
            <StatCard
              label="Total incidents"
              value={stats.totalIncidents.toLocaleString()}
              accent="green"
            />
          </div>
        ) : null}

        {/* Charts row */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-lg shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-4">
              Log volume — last hour
            </h2>
            <LogVolumeChart data={stats?.volumeOverTime ?? []} />
          </div>

          <div className="bg-white rounded-lg shadow-sm p-5">
            <h2 className="text-sm font-semibold text-gray-700 mb-4">
              Error rate by service
            </h2>
            <ErrorRateChart data={stats?.serviceStats ?? []} />
          </div>
        </div>

        {/* Incidents + alerts row */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-lg shadow-sm p-5">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-sm font-semibold text-gray-700">
                Open incidents
              </h2>
              <span className="text-xs bg-red-100 text-red-600 px-2 py-0.5 rounded-full font-medium">
                {incidents.length} open
              </span>
            </div>
            <IncidentsTable incidents={incidents} />
          </div>

          <div className="bg-white rounded-lg shadow-sm p-5">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-sm font-semibold text-gray-700">
                Recent alerts
              </h2>
              <span className="text-xs text-gray-400">{alerts.length} shown</span>
            </div>
            <div className="max-h-96 overflow-y-auto pr-1">
              <AlertsFeed alerts={alerts} />
            </div>
          </div>
        </div>

      </main>
    </div>
  );
}