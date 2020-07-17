import { DataViewRequest, DataView, OrderBy, FilterOrGroup } from "./core";
export declare function process<T = any>(items: T[], req: DataViewRequest): DataView<T>;
export declare function skipAndTake<T = any>(items: T[], skip: number, take: number): T[];
export declare function orderBy<T = any>(items: T[], desc: OrderBy[]): T[];
export declare function filter<T = any>(items: T[], desc: FilterOrGroup): T[];
