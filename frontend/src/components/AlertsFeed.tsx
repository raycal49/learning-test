import type { Alert } from '../types/dashboard';

interface Props {
  alerts: Alert[];
}

function formatTime(ts: string) {
  return new Date(ts).toLocaleString([], {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function AlertsFeed({ alerts }: Props) {
  if (!alerts.length) {
    return (
      <p className="text-gray-400 text-sm py-4 text-center">No recent alerts</p>
    );
  }

  return (
    <ul className="space-y-2">
      {alerts.map(alert => (
        <li
          key={alert.id}
          className="flex items-start gap-3 p-3 rounded-lg bg-red-50 border border-red-100"
        >
          {/* Simple dot indicator */}
          <span className="mt-1 h-2 w-2 rounded-full bg-red-400 shrink-0" />
          <div className="min-w-0">
            <p className="text-xs font-semibold text-red-700">
              {alert.serviceName}
            </p>
            <p className="text-xs text-red-600 mt-0.5 leading-snug">
              {alert.description}
            </p>
            <p className="text-xs text-red-400 mt-1">
              {formatTime(alert.triggeredAt)}
            </p>
          </div>
        </li>
      ))}
    </ul>
  );
}