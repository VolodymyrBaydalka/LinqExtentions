import { OrderBy, SortDirection, FilterOrGroup, Group } from "./core";
export declare function isOrderedBy(orderBy: OrderBy[], field: string): SortDirection | undefined;
export declare function toggleOrderBy(orderBy: OrderBy[], field: string, mode?: "replace" | "append"): OrderBy[];
export declare function isFilterGroup(filter: FilterOrGroup): filter is Group;
export declare function reduceFilter(filter: FilterOrGroup): FilterOrGroup | null;
