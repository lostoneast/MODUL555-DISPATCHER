import { api } from "./api";
import type { ObjectProduct } from "./objectWorkspace";
import type { ProductRow } from "./productList";

export interface ProductHistoryEntry { kind: string; id: number; date: string; endDate: string | null; description: string; comment: string | null }
export interface ProductDetail { product: ProductRow; history: ProductHistoryEntry[] }
export interface ProductDetailWrite extends Omit<ObjectProduct, "bindingSource" | "createdAt" | "updatedAt" | "id"> {
  constructionObjectId: number | null;
  tripId: number | null;
}
export interface ProductTripOption { id: number; tripNumber: string; status: string; plantName: string; vehicle: string; plannedArrivalAt: string | null }
const detailApi = api.injectEndpoints({ endpoints: (build) => ({
  productDetail: build.query<ProductDetail, string>({
    query: (id) => `/products/${encodeURIComponent(id)}`,
    providesTags: ["Catalog"],
  }),
  productAvailableTrips: build.query<ProductTripOption[], { id: string; objectId?: number | null }>({
    query: ({ id, objectId }) => `/products/${encodeURIComponent(id)}/available-trips${objectId ? `?objectId=${objectId}` : ""}`,
    providesTags: ["Catalog"],
  }),
  saveProductDetail: build.mutation<ProductDetail, { id: string; body: ProductDetailWrite }>({
    query: ({ id, body }) => ({ url: `/products/${encodeURIComponent(id)}/details`, method: "PUT", body }),
    invalidatesTags: ["Catalog", "Lookup"],
  }),
}) });
export const { useProductDetailQuery, useProductAvailableTripsQuery, useSaveProductDetailMutation } = detailApi;
