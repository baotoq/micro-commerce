import { createHttpClient } from "./http-client";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/statistics";

class StatisticService {
  public async getOrdersAsync() {
    const { data } = await httpClient.get<[]>(`${resource}/orders`);
    return data;
  }
  public async getReviewsAsync() {
    const { data } = await httpClient.get<[]>(`${resource}/reviews`);
    return data;
  }
}

export default new StatisticService();
