import type { Incident } from '../types/dashboard';

interface Props {
  incidents: Incident[];
}

function timeAgo(ts: string) {
  const diff = Date.now() - new Date(ts).getTime();
  const mins = Math.floor(diff / 60_000);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}

// Color badge by service name — consistent across refreshes
function serviceBadgeColor(name: string) {
  const colors: Record<string, string> = {
    'payment-service': 'bg-purple-100 text-purple-700',
    'auth-service':    'bg-blue-100 text-blue-700',
    'order-service':   'bg-orange-100 text-orange-700',
  };
  return colors[name] ?? 'bg-gray-100 text-gray-700';
}

export function IncidentsTable({ incidents }: Props) {
  if (!incidents.length) {
    return (
      <p className="text-gray-400 text-sm py-4 text-center">No open incidents</p>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-gray-400 border-b border-gray-100">
            <th className="pb-2 font-medium">Service</th>
            <th className="pb-2 font-medium">Pattern</th>
            <th className="pb-2 font-medium text-right">Occurrences</th>
            <th className="pb-2 font-medium text-right">Last seen</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {incidents.slice(0, 10).map(incident => (
            <tr key={incident.id} className="hover:bg-gray-50 transition-colors">
              <td className="py-2.5 pr-4">
                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${serviceBadgeColor(incident.serviceName)}`}>
                  {incident.serviceName}
                </span>
              </td>
              <td className="py-2.5 pr-4 text-gray-600 font-mono text-xs max-w-xs truncate">
                {incident.messagePattern}
              </td>
              <td className="py-2.5 pr-4 text-right font-semibold text-gray-700">
                {incident.occurrenceCount.toLocaleString()}
              </td>
              <td className="py-2.5 text-right text-gray-400">
                {timeAgo(incident.lastSeen)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}