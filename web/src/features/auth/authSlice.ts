import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export interface AuthUser {
  id: string;
  userName: string;
  fullName: string;
  position: string;
  roles: string[];
}

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
}

const TOKEN_KEY = "dispatcher.token";
const REFRESH_KEY = "dispatcher.refresh";
const USER_KEY = "dispatcher.user";

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}

const initialState: AuthState = {
  token: localStorage.getItem(TOKEN_KEY),
  refreshToken: localStorage.getItem(REFRESH_KEY),
  user: readStoredUser(),
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    credentialsReceived(
      state,
      action: PayloadAction<{
        token: string;
        refreshToken?: string | null;
        user: AuthUser;
      }>,
    ) {
      state.token = action.payload.token;
      state.refreshToken = action.payload.refreshToken ?? null;
      state.user = action.payload.user;
      localStorage.setItem(TOKEN_KEY, action.payload.token);
      if (action.payload.refreshToken)
        localStorage.setItem(REFRESH_KEY, action.payload.refreshToken);
      else localStorage.removeItem(REFRESH_KEY);
      localStorage.setItem(USER_KEY, JSON.stringify(action.payload.user));
    },
    tokenReceived(
      state,
      action: PayloadAction<{ token: string; refreshToken?: string | null }>,
    ) {
      state.token = action.payload.token;
      localStorage.setItem(TOKEN_KEY, action.payload.token);
      if (action.payload.refreshToken) {
        state.refreshToken = action.payload.refreshToken;
        localStorage.setItem(REFRESH_KEY, action.payload.refreshToken);
      }
    },
    loggedOut(state) {
      state.token = null;
      state.refreshToken = null;
      state.user = null;
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_KEY);
      localStorage.removeItem(USER_KEY);
    },
  },
});

export const { credentialsReceived, tokenReceived, loggedOut } = authSlice.actions;
export default authSlice.reducer;
