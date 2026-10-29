import { apiFetch } from "@/lib/api";

export type DeliveryAddress = {
  recipientName: string;
  phone: string;
  street: string;
  ward: string;
  province: string;
};

export type CheckoutLineProblem = {
  variantId: string;
  reason: string;
  available: number | null;
};

export type Checkout = {
  id: string;
  status: "AwaitingPayment" | "Expired";
  placedAt: string;
  expiresAt: string;
  deliveryAddress: DeliveryAddress;
  itemsTotal: number;
  shippingTotal: number;
  total: number;
  merchants: { merchantId: string; subtotal: number; shippingFee: number }[];
  lines: {
    variantId: string;
    merchantId: string;
    productId: string;
    productName: string;
    optionValues: string[];
    unitPrice: number;
    quantity: number;
  }[];
};

export type CheckoutConflict = {
  detail: string;
  lines: CheckoutLineProblem[];
  existingCheckoutId: string | null;
};

export type CheckoutPage = {
  items: Checkout[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export const provinceNames = [
  "An Giang",
  "Bắc Ninh",
  "Cà Mau",
  "Cao Bằng",
  "Cần Thơ",
  "Đà Nẵng",
  "Đắk Lắk",
  "Điện Biên",
  "Đồng Nai",
  "Đồng Tháp",
  "Gia Lai",
  "Hà Nội",
  "Hà Tĩnh",
  "Hải Phòng",
  "Hồ Chí Minh",
  "Huế",
  "Hưng Yên",
  "Khánh Hòa",
  "Lai Châu",
  "Lạng Sơn",
  "Lào Cai",
  "Lâm Đồng",
  "Nghệ An",
  "Ninh Bình",
  "Phú Thọ",
  "Quảng Ngãi",
  "Quảng Ninh",
  "Quảng Trị",
  "Sơn La",
  "Tây Ninh",
  "Thái Nguyên",
  "Thanh Hóa",
  "Tuyên Quang",
  "Vĩnh Long",
] as const;

export function placeCheckout(deliveryAddress: DeliveryAddress, itemsTotal: number, accessToken: string) {
  return apiFetch<Checkout>("/checkout", {
    method: "POST",
    headers: { Authorization: "Bearer " + accessToken },
    body: JSON.stringify({ deliveryAddress, itemsTotal }),
  });
}

export function getCheckouts(accessToken: string) {
  return apiFetch<CheckoutPage>("/checkouts?page=1&pageSize=20", {
    headers: { Authorization: "Bearer " + accessToken },
  });
}
