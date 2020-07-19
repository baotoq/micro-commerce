export interface Product {
  id: number;
  name: string;
  price: number;
  description: string;
  imageUri: string;
  ratingAverage: number;
  categories?: [{ id: number, name: string }] | undefined
}
