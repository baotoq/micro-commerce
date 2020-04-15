import { httpClient } from "./http-client";

const data = [
  {
    id: 1,
    name: "adsasd3124"
  },
  {
    id: 2,
    name: "12412414"
  }
] as CatalogResponse[];

export interface CatalogResponse {
  id: number;
  name: string;
}

export class CatalogService {
  public async find(id: number): Promise<CatalogResponse> {
    return new Promise((resolve, reject) => {
      resolve(data.find(x => x.id === id));
    });
  }

  public async findAll(): Promise<CatalogResponse[]> {
    return new Promise((resolve, reject) => {
      resolve(data);
    });
  }
}
