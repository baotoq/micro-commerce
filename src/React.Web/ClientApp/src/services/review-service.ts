import { httpClient } from "./http-client";
import { CursorPaged, ReviewStatus } from "../models/index";

export interface ReviewResponse {
  id: number;
  title: string;
  comment: number;
  rating: number;
  createdDate: Date;
}

export interface FindReviewQuery {
  pageToken: Date;
  reviewStatus: ReviewStatus;
  productId: number;
}

class ReviewService {
  public async findAsync(query: FindReviewQuery) {
    const params = query;
    const { data } = await httpClient.get<CursorPaged<ReviewResponse>>("/api/reviews", { params });
    return data;
  }
}

export default new ReviewService();
