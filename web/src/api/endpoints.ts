import { api } from "./api";
import type { AuthUser } from "../features/auth/authSlice";

export interface PrivateDataItem {
  id: number;
  name: string;
  status: string;
}

export interface PrivateDataResponse {
  user: string | null;
  authenticated: boolean;
  roles: string[];
  data: PrivateDataItem[];
}

const injected = api.injectEndpoints({
  endpoints: (build) => ({
    login: build.mutation<
      { token: string; refreshToken?: string; user: AuthUser },
      { userName: string; password: string }
    >({
      query: (body) => ({ url: "/auth/login", method: "POST", body }),
    }),
    me: build.query<AuthUser, void>({ query: () => "/auth/me" }),
    privateData: build.query<PrivateDataResponse, void>({
      query: () => "/data/private",
      providesTags: ["PrivateData"],
    }),
  }),
});

export const { useLoginMutation, useMeQuery, usePrivateDataQuery } = injected;
