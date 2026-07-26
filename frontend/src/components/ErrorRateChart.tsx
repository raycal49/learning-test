import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, Cell,
} from 'recharts';
import type { ServiceStats } from '../types/dashboard';

interface Props {
  data: ServiceStats[];
}

// Color by error rate severity
function barColor(rate: number) {
  if (rate >= 30) return '#ef4444'; // high
  if (rate >= 10) return '#f59e0b'; // medium
  return '#22c55e';                 // healthy
}

export function ErrorRateChart({ data }: Props) {
  if (!data.length) {
    return (
      <div className="flex items-center justify-center h-48 text-gray-400 text-sm">
        No service data in the last hour
      </div>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={240}>
      <BarChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis
          dataKey="serviceName"
          tick={{ fontSize: 11, fill: '#9ca3af' }}
        />
        <YAxis
          unit="%"
          domain={[0, 100]}
          tick={{ fontSize: 11, fill: '#9ca3af' }}
        />
        <Tooltip
        formatter={(value) => {
            const v = typeof value === 'number' ? value : 0;
            return [`${v}%`, 'Error rate'];
        }}
        contentStyle={{ fontSize: 12, borderRadius: 6 }}
        />
        <Bar dataKey="errorRate" name="Error rate" radius={[4, 4, 0, 0]}>
          {data.map((entry, i) => (
            <Cell key={i} fill={barColor(entry.errorRate)} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}