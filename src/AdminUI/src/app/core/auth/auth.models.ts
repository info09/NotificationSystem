export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType?: string;
  expiresIn?: number;
}
