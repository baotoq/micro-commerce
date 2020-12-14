import { createHttpClient } from "./http-client";
import { CursorPaged, ReviewStatus, CursorPagedQuery, OffsetPaged, OffsetPagedQuery, Review } from "../models/index";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/reviews";

export interface FindReviewCursorQuery extends CursorPagedQuery<Date> {
  reviewStatus: ReviewStatus;
  productId: number;
}

export interface FindCategoriesOffsetQuery extends OffsetPagedQuery {
  queryString?: string;
}

export interface CreateReviewCommand {
  title: string;
  comment: string;
  rating: number;
  productId: number;
}

class ReviewService {
  public async findCursorAsync(query: FindReviewCursorQuery) {
    const { data } = await httpClient.get<CursorPaged<Review, Date>>(`${resource}/cursor`, { params: query });
    return data;
  }
  public async findOffsetAsync(query: FindCategoriesOffsetQuery) {
    const { data } = await httpClient.get<OffsetPaged<Review>>(`${resource}/offset`, { params: query });
    return data;
  }
  public async createAsync(command: CreateReviewCommand) {
    const { data } = await httpClient.post(resource, command);
    return data;
  }
  public async changeStatusAsync(id: number, reviewStatus: ReviewStatus) {
    await httpClient.post(`${resource}/${id}/change-review-status`, { reviewStatus });
  }
  public async deleteAsync(id: number) {
    await httpClient.delete(`${resource}/${id}`);
  }
}

export default new ReviewService();
