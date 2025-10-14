import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';

interface ReadOnlyFieldProps {
  label: string;
  value: string;
  className?: string;
}

export function ReadOnlyField({ label, value, className = '' }: ReadOnlyFieldProps) {
  return (
    <div className={`space-y-2 ${className}`}>
      <Label>{label}</Label>
      <Input
        value={value}
        readOnly
        className="bg-gray-50 cursor-not-allowed"
      />
    </div>
  );
}