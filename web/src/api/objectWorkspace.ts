import { api } from "./api";
import type { Paged } from "./endpoints";

export const productStatuses = [
  { value: "Created", label: "Создано" },
  { value: "InProduction", label: "В производстве" },
  { value: "InStorage", label: "На складе" },
  { value: "AssignedToTrip", label: "Назначено в рейс" },
  { value: "InTransit", label: "В пути" },
  { value: "Delivered", label: "Доставлено" },
  { value: "Cancelled", label: "Отменено" },
];

export const dimensions = [
  ["widthMm", "Ширина, мм"],
  ["heightMm", "Высота, мм"],
  ["thicknessMm", "Толщина, мм"],
  ["corniceWidthIncreaseMm", "Припуск карниза, мм"],
  ["totalWidthWithCorniceMm", "Ширина с карнизом, мм"],
  ["thicknessIncreaseMm", "Припуск толщины, мм"],
  ["rightBendMm", "Правый загиб, мм"],
  ["leftBendMm", "Левый загиб, мм"],
  ["claddingWidthWithBendsMm", "Обшивка с загибами, мм"],
] as const;
export type Dimension = (typeof dimensions)[number][0];
export type ObjectProduct = Record<Dimension, number> & {
  id: number;
  productCode: string;
  productTypeId: number;
  productTypeName?: string;
  mark: string;
  weightKg: number | null;
  status: string;
  additionalInfo?: string | null;
  version: number;
  buildingSectionId: number | null;
  buildingSectionName?: string | null;
  floorId: number | null;
  floorName?: string | null;
  installationNumber?: string | null;
  bindingSource: string;
  createdAt: string;
  updatedAt: string;
};
export interface ProductsQuery {
  objectId: string;
  page: number;
  pageSize: number;
  search?: string;
  productTypeId?: number;
  status?: string;
  buildingSectionId?: number;
  floorId?: number;
  sortBy?: string;
  descending?: boolean;
}
export interface DeletionPreview {
  name: string;
  previewToken: string;
  productsToDelete: number;
  productsToKeep: number;
  records: Record<string, number>;
}
function queryString(query: ProductsQuery) {
  const params = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (key !== "objectId" && value !== undefined && value !== "")
      params.set(key, String(value));
  });
  return params.toString();
}
export function errorMessage(error: unknown): string {
  if (error && typeof error === "object" && "data" in error) {
    const data = error.data as
      { message?: string; errors?: Record<string, string[]> } | undefined;
    if (data?.message) return data.message;
    if (data?.errors) return Object.values(data.errors).flat().join(" ");
  }
  return "Операция не выполнена. Проверьте соединение и повторите попытку.";
}

const workspace = api.injectEndpoints({
  endpoints: (build) => ({
    importObjectProducts: build.mutation<
      { created: number; updated: number; errors: string[] },
      { objectId: string; file: File; suffix: string; startNumber: number }
    >({
      query: ({ objectId, file, suffix, startNumber }) => {
        const body = new FormData();
        body.append("file", file);
        body.append("suffix", suffix);
        body.append("startNumber", String(startNumber));
        return { url: `/construction-objects/${objectId}/products/import`, method: "POST", body };
      },
      invalidatesTags: (result) => result?.created ? ["Catalog", "Lookup"] : [],
    }),
    objectImportExample: build.mutation<{ downloaded: boolean }, string>({
      query: (objectId) => ({
        url: `/construction-objects/${objectId}/products/import-example`,
        responseHandler: async (response) => {
          if (!response.ok) return response.json();
          const url = URL.createObjectURL(await response.blob());
          const link = document.createElement("a");
          link.href = url;
          link.download = "products-import-example.xlsx";
          link.click();
          setTimeout(() => URL.revokeObjectURL(url), 1000);
          return { downloaded: true };
        },
      }),
    }),
    objectProducts: build.query<Paged<ObjectProduct>, ProductsQuery>({
      query: (q) => `/construction-objects/${q.objectId}/products?${queryString(q)}`,
      providesTags: ["Catalog"],
    }),
    objectStructure: build.query<
      {
        sections: { id: number; name: string }[];
        floors: { id: number; buildingSectionId: number; name: string }[];
      },
      string
    >({
      query: (id) => `/construction-objects/${id}/products/structure`,
      providesTags: ["Catalog"],
    }),
    saveObjectProduct: build.mutation<
      ObjectProduct,
      { objectId: string; productId?: number; body: Partial<ObjectProduct> }
    >({
      query: ({ objectId, productId, body }) => ({
        url: `/construction-objects/${objectId}/products${productId === undefined ? "" : `/${productId}`}`,
        method: productId === undefined ? "POST" : "PUT",
        body,
      }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    objectDeletionPreview: build.query<DeletionPreview, string>({
      query: (id) => `/construction-objects/${id}/deletion-preview`,
      keepUnusedDataFor: 0,
    }),
    deleteConstructionObject: build.mutation<
      void,
      { id: string; confirmationName: string; previewToken: string }
    >({
      query: ({ id, ...body }) => ({
        url: `/construction-objects/${id}`,
        method: "DELETE",
        body,
      }),
      invalidatesTags: ["Catalog", "Lookup"],
    }),
    exportObjectProducts: build.mutation<{ downloaded: boolean }, ProductsQuery>({
      query: (q) => ({
        url: `/construction-objects/${q.objectId}/products/export?${queryString(q)}`,
        responseHandler: async (response) => {
          if (!response.ok) return response.json();
          const url = URL.createObjectURL(await response.blob());
          const link = document.createElement("a");
          link.href = url;
          link.download = `object-${q.objectId}-products.xlsx`;
          link.click();
          setTimeout(() => URL.revokeObjectURL(url), 1000);
          return { downloaded: true };
        },
      }),
    }),
  }),
});
export const {
  useImportObjectProductsMutation,
  useObjectImportExampleMutation,
  useObjectProductsQuery,
  useObjectStructureQuery,
  useSaveObjectProductMutation,
  useObjectDeletionPreviewQuery,
  useDeleteConstructionObjectMutation,
  useExportObjectProductsMutation,
} = workspace;
