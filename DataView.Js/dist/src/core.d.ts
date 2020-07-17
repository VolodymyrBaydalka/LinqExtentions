export declare type SortDirection = 'asc' | 'desc';
export declare type FilterOperator = 'eq' | 'neq' | 'lt' | 'lte' | 'gt' | 'gte' | 'startsWith' | 'endsWith' | 'contains';
export declare type FilterLogic = 'or' | 'and';
export interface Filter {
    field: string;
    op: FilterOperator;
    value: any;
}
export interface Group {
    logic: FilterLogic;
    filters: FilterOrGroup[];
}
export declare type FilterOrGroup = Filter | Group;
export interface OrderBy {
    field: string;
    dir: SortDirection;
}
export interface DataViewRequest {
    take: number;
    skip: number;
    orderBy: OrderBy[];
    filter: FilterOrGroup;
}
export interface DataView<T = any> {
    taken: number;
    skiped: number;
    total: number;
    items: T[];
}
