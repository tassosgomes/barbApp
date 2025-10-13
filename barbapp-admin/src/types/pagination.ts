/**
 * Generic paginated response wrapper from API
 * @template T - The type of items in the paginated list
 */
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Generic API response wrapper (for non-paginated responses)
 * @template T - The type of data in the response
 */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}
