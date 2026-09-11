import { api } from "./api";
import type { AuthUser } from "../features/auth/authSlice";
import type { RootState } from "../store";

export interface Paged<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface LookupItem {
  id: string;
  code?: string | null;
  name: string;
}

export interface ImportResult {
  created: number;
  updated: number;
  errors: string[];
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

    list: build.query<
      Paged<Record<string, unknown>>,
      { path: string; page?: number; search?: string; extra?: string }
    >({
      query: ({ path, page = 1, search, extra }) => {
        const params = new URLSearchParams({
          page: String(page),
          pageSize: "20",
        });
        if (search) params.set("search", search);
        if (extra)
          extra.split("&").forEach((p) => {
            const [k, v] = p.split("=");
            if (k && v) params.set(k, decodeURIComponent(v));
          });
        return `/${path}?${params}`;
      },
      providesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
    lookup: build.query<LookupItem[], string>({
      query: (entity) => `/lookups/${entity}`,
    }),
    create: build.mutation<unknown, { path: string; body: unknown }>({
      query: ({ path, body }) => ({ url: `/${path}`, method: "POST", body }),
      invalidatesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
    update: build.mutation<unknown, { path: string; id: string; body: unknown }>({
      query: ({ path, id, body }) => ({
        url: `/${path}/${id}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
    remove: build.mutation<void, { path: string; id: string }>({
      query: ({ path, id }) => ({ url: `/${path}/${id}`, method: "DELETE" }),
      invalidatesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
    activate: build.mutation<void, { path: string; id: string }>({
      query: ({ path, id }) => ({
        url: `/${path}/${id}/activate`,
        method: "POST",
      }),
      invalidatesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
    importExcel: build.mutation<ImportResult, { path: string; file: File }>({
      query: ({ path, file }) => {
        const body = new FormData();
        body.append("file", file);
        return { url: `/${path}/import`, method: "POST", body };
      },
      invalidatesTags: (_r, _e, arg) => [{ type: "Catalog", id: arg.path }],
    }),
  }),
});

export const {
  useLoginMutation,
  useMeQuery,
  useListQuery,
  useLookupQuery,
  useCreateMutation,
  useUpdateMutation,
  useRemoveMutation,
  useActivateMutation,
  useImportExcelMutation,
} = injected;

/** Download Excel export with current auth token. */
export async function exportCatalog(path: string, getState: () => RootState) {
  const token = getState().auth.token;
  const res = await fetch(`/api/${path}/export`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) throw new Error("Ошибка экспорта");
  const blob = await res.blob();
  const dispo = res.headers.get("Content-Disposition") ?? "";
  const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(dispo);
  const fileName = match
    ? match[1].replace(/['"]/g, "")
    : `${path}.xlsx`;
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  a.click();
  URL.revokeObjectURL(url);
}
