export interface Coupon {
  couponId: number;
  code: string;
  discountPercent: number;
  minOrderValue?: number;
  expiryDate?: string;
  usageLimit?: number;
  timesUsed: number;
  isActive: boolean;
}

export interface CreateCouponRequest {
  code: string;
  discountPercent: number;
  minOrderValue?: number;
  expiryDate?: string;
  usageLimit?: number;
  isActive: boolean;
}

export interface UpdateCouponRequest {
  code: string;
  discountPercent: number;
  minOrderValue?: number;
  expiryDate?: string;
  usageLimit?: number;
  isActive: boolean;
}

export interface ValidateCouponRequest {
  couponCode: string;
  orderValue: number;
}

export interface CouponValidationResponse {
  isValid: boolean;
  discountPercent: number;
  discountAmount: number;
  message: string;
}
