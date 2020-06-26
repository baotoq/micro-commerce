export interface Product {
  id: number;
  name: string;
  price: number;
  description: string;
  imageUri: string;
  categories?: [{ id: number, name: string }] | undefined
}
