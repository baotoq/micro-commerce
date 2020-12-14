export * from "./category";
export * from "./product";
export * from "./user";
export * from "./review";
export * from "./role";
export * from "./cart";
export * from "./order";

export interface OffsetPaged<T> {
  data: T[];
  currentPage: number;
  nextPage: number;
  totalPages: number;
  totalCount: number;
}

export interface CursorPaged<T, TToken> {
  data: T[];
  previousPageToken: TToken;
  nextPageToken: TToken;
}

export interface OffsetPagedQuery {
  page: number;
  pageSize: number;
}

export interface CursorPagedQuery<TToken> {
  pageToken: TToken;
  pageSize?: number;
}
