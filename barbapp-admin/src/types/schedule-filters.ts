/**
 * Tipos para filtros de navegação de agenda por data
 */

export interface ScheduleFilters {
  date: string; // ISO 8601 date string (YYYY-MM-DD)
}

export interface DateNavigation {
  currentDate: string;
  canGoPrevious: boolean;
  canGoNext: boolean;
}

export type ScheduleViewMode = 'day' | 'week' | 'month';