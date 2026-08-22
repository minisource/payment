/** Cursor-based pagination response from Payment backend */
export interface CursorPage<T> {
  items: T[];
  next_cursor?: string | null;
  has_more: boolean;
}

/** Offset-based pagination response from Payment backend */
export interface OffsetPage<T> {
  items: T[];
  total: number;
  skip: number;
  take: number;
}

/** Common pagination params for offset-based endpoints */
export interface OffsetPaginationParams {
  skip?: number;
  take?: number;
}

/** Common pagination params for cursor-based endpoints */
export interface CursorPaginationParams {
  cursor?: string;
  limit?: number;
}

/** Date range filter */
export interface DateRangeParams {
  dateFrom?: string;
  dateTo?: string;
}

/** Generic list params combining pagination + filters */
export interface ListParams extends OffsetPaginationParams, DateRangeParams {
  query?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
