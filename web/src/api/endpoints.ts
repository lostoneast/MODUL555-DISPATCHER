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
      providesTags: (_r, _e, entity) => [{ type: "Lookup", id: entity }],
      // После создания связанной сущности (завод → линия) нужен свежий список.
      keepUnusedDataFor: 0,
    }),
    create: build.mutation<unknown, { path: string; body: unknown }>({
      query: ({ path, body }) => ({ url: `/${path}`, method: "POST", body }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    update: build.mutation<unknown, { path: string; id: string; body: unknown }>({
      query: ({ path, id, body }) => ({
        url: `/${path}/${id}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    remove: build.mutation<void, { path: string; id: string }>({
      query: ({ path, id }) => ({ url: `/${path}/${id}`, method: "DELETE" }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    activate: build.mutation<void, { path: string; id: string }>({
      query: ({ path, id }) => ({
        url: `/${path}/${id}/activate`,
        method: "POST",
      }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    importExcel: build.mutation<ImportResult, { path: string; file: File }>({
      query: ({ path, file }) => {
        const body = new FormData();
        body.append("file", file);
        return { url: `/${path}/import`, method: "POST", body };
      },
      invalidatesTags: ["Catalog", "Lookup"],
    }),

    adminOverview: build.query<
      {
        products: number;
        productTypes: number;
        plants: number;
        productionLines: number;
        constructionObjects: number;
        constructionTakts: number;
        vehicles: number;
        storageAreas: number;
        trips: number;
        auditEvents: number;
      },
      void
    >({
      query: () => "/admin/overview",
    }),
    adminRoles: build.query<{ value: string; label: string }[], void>({
      query: () => "/admin/roles",
    }),
    adminAudit: build.query<
      Paged<{
        id: number;
        timestamp: string;
        userId?: string;
        userName?: string;
        entityType: string;
        entityId: string;
        action: string;
        comment?: string;
        source: string;
        correlationId?: string;
      }>,
      { page?: number; search?: string }
    >({
      query: ({ page = 1, search }) => {
        const p = new URLSearchParams({ page: String(page), pageSize: "20" });
        if (search) p.set("search", search);
        return `/admin/audit?${p}`;
      },
    }),
    clearDemo: build.mutation<{ message: string; affected: number }, void>({
      query: () => ({ url: "/admin/demo/clear", method: "POST" }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    seedDemo: build.mutation<{ message: string; affected: number }, void>({
      query: () => ({ url: "/admin/demo/seed", method: "POST" }),
      invalidatesTags: ["Catalog", "Lookup"],
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
  useAdminOverviewQuery,
  useAdminRolesQuery,
  useAdminAuditQuery,
  useClearDemoMutation,
  useSeedDemoMutation,
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
