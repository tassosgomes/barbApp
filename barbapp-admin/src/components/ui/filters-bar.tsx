import { useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Search, X } from 'lucide-react';

interface FilterField {
  key: string;
  label: string;
  type: 'text' | 'select';
  placeholder?: string;
  options?: { value: string; label: string }[];
}

interface FiltersBarProps {
  fields: FilterField[];
  onFiltersChange?: (filters: Record<string, string>) => void;
  className?: string;
}

export function FiltersBar({ fields, onFiltersChange, className }: FiltersBarProps) {
  const [searchParams, setSearchParams] = useSearchParams();

  const getFilterValue = (key: string) => searchParams.get(key) || '';

  const updateFilter = (key: string, value: string) => {
    const newSearchParams = new URLSearchParams(searchParams);

    if (value.trim()) {
      newSearchParams.set(key, value.trim());
    } else {
      newSearchParams.delete(key);
    }

    // Reset to page 1 when filters change
    if (key !== 'page') {
      newSearchParams.delete('page');
    }

    setSearchParams(newSearchParams);

    // Notify parent component
    if (onFiltersChange) {
      const filters: Record<string, string> = {};
      fields.forEach(field => {
        const filterValue = newSearchParams.get(field.key);
        if (filterValue) {
          filters[field.key] = filterValue;
        }
      });
      onFiltersChange(filters);
    }
  };

  const clearAllFilters = () => {
    const newSearchParams = new URLSearchParams();
    setSearchParams(newSearchParams);

    if (onFiltersChange) {
      onFiltersChange({});
    }
  };

  const hasActiveFilters = fields.some(field => getFilterValue(field.key));

  return (
    <div className={`flex flex-wrap gap-4 items-end ${className}`}>
      {fields.map(field => (
        <div key={field.key} className="flex flex-col gap-2 min-w-[200px]">
          <label className="text-sm font-medium text-gray-700">
            {field.label}
          </label>
          {field.type === 'text' ? (
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder={field.placeholder}
                value={getFilterValue(field.key)}
                onChange={(e) => updateFilter(field.key, e.target.value)}
                className="pl-10"
              />
            </div>
          ) : field.type === 'select' ? (
            <Select
              value={getFilterValue(field.key)}
              onValueChange={(value) => updateFilter(field.key, value)}
            >
              <SelectTrigger>
                <SelectValue placeholder={field.placeholder} />
              </SelectTrigger>
              <SelectContent>
                {field.options?.map(option => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          ) : null}
        </div>
      ))}

      {hasActiveFilters && (
        <Button
          variant="outline"
          onClick={clearAllFilters}
          className="flex items-center gap-2"
        >
          <X className="h-4 w-4" />
          Limpar filtros
        </Button>
      )}
    </div>
  );
}