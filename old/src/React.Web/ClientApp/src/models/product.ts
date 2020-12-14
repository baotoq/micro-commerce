export interface Product {
  id: number;
  name: string;
  price: number;
  cartMaxQuantity: number;
  stockQuantity: number;
  description: string;
  imageUri: string;
  ratingAverage: number;
  categories?: [{ id: number, name: string }] | undefined
}
