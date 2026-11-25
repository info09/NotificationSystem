export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType?: string;
  expiresIn?: number;
}

export interface RequestPasswordResetRequest {
  identifier: string;
}

export interface RequestPasswordResetResponse {
  accepted: boolean;
}

export interface ConfirmPasswordResetRequest {
  token: string;
  newPassword: string;
}

export interface ConfirmPasswordResetResponse {
  reset: boolean;
}
