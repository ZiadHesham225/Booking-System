export interface PaginatedResponse<T> {
  items: T[];
  currentPage: number;
  totalPages: number;
  totalItems: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}
