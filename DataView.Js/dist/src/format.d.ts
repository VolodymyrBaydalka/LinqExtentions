import { FilterOrGroup, OrderBy, DataViewRequest } from "./core";
export declare function filterToString(filter: FilterOrGroup, format?: (v: any) => string): string | null;
export declare function orderByToString(orderBy: OrderBy[]): string | null;
export declare function requestToQueryString(request: DataViewRequest, prefix?: string): string;
