export type PaginationView = {
  page: number;
  totalPages: number;
  hasPrev: boolean;
  hasNext: boolean;
  prevPage: number | null;
  nextPage: number | null;
  window: number[];
};

export function paginate(
  currentPage: number,
  total: number,
  pageSize: number,
  windowSize = 3,
): PaginationView {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const page = Math.min(Math.max(1, currentPage), totalPages);
  const span = Math.min(windowSize, totalPages);
  const start = Math.min(
    Math.max(1, page - Math.floor(span / 2)),
    Math.max(1, totalPages - span + 1),
  );
  return {
    page,
    totalPages,
    hasPrev: page > 1,
    hasNext: page < totalPages,
    prevPage: page > 1 ? page - 1 : null,
    nextPage: page < totalPages ? page + 1 : null,
    window: Array.from({ length: span }, (_, i) => start + i),
  };
}

export function pageHref(page: number, base = "?"): string {
  if (page === 1) return base;
  return `${base.includes("?") ? base : `${base}?`}page=${page}`;
}
