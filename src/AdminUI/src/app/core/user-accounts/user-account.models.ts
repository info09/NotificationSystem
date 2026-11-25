export interface UserAccount {
  id: string;
  userName: string;
  email: string;
  displayName: string;
  role: string;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GetUserAccountsResponse {
  items: UserAccount[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateUserAccountRequest {
  userName: string;
  email: string;
  displayName: string;
  role: string;
  password?: string | null;
}

export interface UpdateUserAccountRequest {
  userName: string;
  email: string;
  displayName: string;
  role: string;
  password?: string | null;
}

export interface IntegrationClientCredential {
  id: string;
  userAccountId: string;
  clientId: string;
  status: string;
  createdAtUtc: string;
  expiresAtUtc?: string | null;
  revokedAtUtc?: string | null;
  lastRotatedAtUtc?: string | null;
}

export interface CreateIntegrationClientCredentialRequest {
  clientId?: string | null;
  expiresAtUtc?: string | null;
}

export interface CreatedIntegrationClientCredentialResponse {
  credential: IntegrationClientCredential;
  clientSecret: string;
}

export interface RotatedIntegrationClientCredentialResponse {
  credential: IntegrationClientCredential;
  clientSecret: string;
}
