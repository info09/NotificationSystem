export interface EndUserProfile {
  id: string;
  externalUserId: string;
  displayName: string;
  email?: string | null;
  phoneNumber?: string | null;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface GetEndUserProfilesResponse {
  items: EndUserProfile[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateEndUserProfileRequest {
  externalUserId: string;
  displayName: string;
  email?: string | null;
  phoneNumber?: string | null;
}

export interface UpdateEndUserProfileRequest {
  displayName: string;
  email?: string | null;
  phoneNumber?: string | null;
}
