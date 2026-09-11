import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { RootState } from "../store";
import { loggedOut, tokenReceived } from "../features/auth/authSlice";

export const TAGS = ["PrivateData"] as const;

const rawBaseQuery = fetchBaseQuery({
  baseUrl: "/api",
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.token;
    if (token) headers.set("Authorization", `Bearer ${token}`);
    return headers;
  },
});

const baseQueryWithAuthGuard: typeof rawBaseQuery = async (
  args,
  apiCtx,
  extraOptions,
) => {
  let result = await rawBaseQuery(args, apiCtx, extraOptions);
  if (result.error && result.error.status === 401) {
    const refresh = (apiCtx.getState() as RootState).auth.refreshToken;
    if (refresh) {
      const refreshed = await rawBaseQuery(
        {
          url: "/auth/refresh",
          method: "POST",
          body: { refreshToken: refresh },
        },
        apiCtx,
        extraOptions,
      );
      if (refreshed.data && typeof refreshed.data === "object") {
        const data = refreshed.data as { token: string; refreshToken?: string };
        apiCtx.dispatch(
          tokenReceived({ token: data.token, refreshToken: data.refreshToken }),
        );
        result = await rawBaseQuery(args, apiCtx, extraOptions);
        return result;
      }
    }
    apiCtx.dispatch(loggedOut());
  }
  return result;
};

export const api = createApi({
  reducerPath: "api",
  baseQuery: baseQueryWithAuthGuard,
  tagTypes: TAGS,
  endpoints: () => ({}),
});
