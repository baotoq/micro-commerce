export interface Review {
  id: number;
  title: string;
  comment: number;
  rating: number;
  reviewStatus: ReviewStatus;
  productName?: string;
  createdById: number;
  createdByUserName: string;
  createdDate: Date;
}

export enum ReviewStatus {
  Pending,
  Approved,
  NotApproved,
}
