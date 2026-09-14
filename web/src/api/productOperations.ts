import { api } from "./api";

export interface ProductOperation { productVersion: number; reason: string }
export interface IdentifierWrite extends ProductOperation { type: string; value: string }
export interface DemandWrite extends ProductOperation {
  demandVersion?: number; constructionTaktId: number; earliestDeliveryDate: string; requiredDeliveryDate: string;
  priorityLevel: string; priorityOrder: number; status: string; comment?: string | null;
}
export interface DemandRevision {
  id: number; revisionNumber: number; constructionTaktId: number; taktName: string;
  requiredProductionStartDate: string; requiredProductionEndDate: string; earliestDeliveryDate: string; requiredDeliveryDate: string;
  priorityLevel: string; priorityOrder: number; changeReason: string | null; comment: string | null; createdAt: string; createdByUserId: string | null;
}
export interface ProductDemand { id: number; version: number; status: string; currentRevisionId: number | null; hasProductionAssignments: boolean; revisions: DemandRevision[] }
export interface DemandTakt { id: number; name: string; productionStartDate: string; productionEndDate: string }
const operations = api.injectEndpoints({ endpoints: (build) => ({
  productIdentifierTypes: build.query<string[], string>({ query: (id) => `/products/${id}/identifiers/types` }),
  saveProductIdentifier: build.mutation<void, { id: string; identifierId?: number; body: IdentifierWrite }>({
    query: ({ id, identifierId, body }) => ({ url: `/products/${id}/identifiers${identifierId ? `/${identifierId}` : ""}`, method: identifierId ? "PUT" : "POST", body }),
    invalidatesTags: ["Catalog"],
  }),
  revokeProductIdentifier: build.mutation<void, { id: string; identifierId: number; body: ProductOperation }>({
    query: ({ id, identifierId, body }) => ({ url: `/products/${id}/identifiers/${identifierId}/revoke`, method: "POST", body }), invalidatesTags: ["Catalog"],
  }),
  productDemands: build.query<ProductDemand[], string>({ query: (id) => `/products/${id}/demands`, providesTags: ["Catalog"] }),
  productDemandTakts: build.query<DemandTakt[], string>({ query: (id) => `/products/${id}/demands/takts`, providesTags: ["Catalog"] }),
  saveProductDemand: build.mutation<void, { id: string; demandId?: number; body: DemandWrite }>({
    query: ({ id, demandId, body }) => ({ url: `/products/${id}/demands${demandId ? `/${demandId}` : ""}`, method: demandId ? "PUT" : "POST", body }), invalidatesTags: ["Catalog"],
  }),
  cancelProductDemand: build.mutation<void, { id: string; demandId: number; body: ProductOperation & { demandVersion: number } }>({
    query: ({ id, demandId, body }) => ({ url: `/products/${id}/demands/${demandId}/cancel`, method: "POST", body }), invalidatesTags: ["Catalog"],
  }),
}) });
export const { useProductIdentifierTypesQuery, useSaveProductIdentifierMutation, useRevokeProductIdentifierMutation,
  useProductDemandsQuery, useProductDemandTaktsQuery, useSaveProductDemandMutation, useCancelProductDemandMutation } = operations;
