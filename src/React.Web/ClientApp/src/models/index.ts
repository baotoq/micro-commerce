export * from "./category";
export * from "./product";
export * from "./user";

export interface OffsetPaged<T> {
  data: T[];
  totalPages: number;
  totalCount: number;
}

export interface CursorPaged<T> {
  data: T[];
  previousPageToken: number;
  nextPageToken: number;
}
