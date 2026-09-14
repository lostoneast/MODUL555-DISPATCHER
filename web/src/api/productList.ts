import { api } from "./api";
import type { Paged } from "./endpoints";

export type FilterOperator = "Eq" | "NotEq" | "Contains" | "Gte" | "Lte" | "IsEmpty" | "IsNotEmpty";
export interface ProductFilter { field: string; operator: FilterOperator; value?: string }
export interface ProductField {
  field: string; type: "text" | "number" | "date" | "datetime" | "enum" | "boolean";
  nullable: boolean; sortable: boolean; operators: FilterOperator[]; values: string[];
}
export interface ProductIdentifierRow {
  id: number; type: string; value: string; isActive: boolean; assignedAt: string; revokedAt: string | null;
}
export interface ProductRow {
  [field: string]: string | number | boolean | null | ProductIdentifierRow[];
  id: number; productCode: string; mark: string; status: string; identifiers: ProductIdentifierRow[];
}
export interface ProductListRequest {
  page: number; pageSize: number; search: string; sortBy: string; descending: boolean; filters: ProductFilter[];
}
const productList = api.injectEndpoints({
  endpoints: (build) => ({
    productList: build.query<Paged<ProductRow>, ProductListRequest>({
      query: ({ filters, ...query }) => {
        const params = new URLSearchParams();
        Object.entries(query).forEach(([key, value]) => params.set(key, String(value)));
        filters.forEach((filter, index) => {
          params.set(`filters[${index}].field`, filter.field);
          params.set(`filters[${index}].operator`, filter.operator);
          if (filter.value !== undefined) params.set(`filters[${index}].value`, filter.value);
        });
        return `/products?${params}`;
      },
      providesTags: ["Catalog"],
    }),
    productFields: build.query<ProductField[], void>({ query: () => "/products/fields" }),
  }),
});
export const { useProductListQuery, useProductFieldsQuery } = productList;
