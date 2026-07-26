interface Props {
  label: string;
  value: number | string;
  // Optional accent color for the left border
  accent?: 'blue' | 'red' | 'yellow' | 'green';
}

const accentMap = {
  blue:   'border-blue-500',
  red:    'border-red-500',
  yellow: 'border-yellow-400',
  green:  'border-green-500',
};

export function StatCard({ label, value, accent = 'blue' }: Props) {
  return (
    <div className={`bg-white rounded-lg shadow-sm border-l-4 ${accentMap[accent]} p-5`}>
      <p className="text-sm text-gray-500 uppercase tracking-wide">{label}</p>
      <p className="text-3xl font-semibold text-gray-800 mt-1">{value}</p>
    </div>
  );
}