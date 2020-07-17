import { FilterOrGroup, OrderBy, DataViewRequest } from "./core";
import { isFilterGroup } from "./functions";

export function filterToString(filter: FilterOrGroup, format?: (v: any) => string): string | null {
    if (filter == null)
        return null;

    if (isFilterGroup(filter))
        return filter.filters.map(f => `(${filterToString(f, format)})`).join(filter.logic);

    const val = (format ?? formatValue)(filter.value);

    return `${filter.field} ${filter.op} ${val}`;
}

function formatValue(value: any): string {
    if (value instanceof Date)
        return `"${value.toISOString()}"`;

    if (typeof(value) == 'string')
        return `"${value}"`;

    return value;
}

export function orderByToString(orderBy: OrderBy[]): string | null {
    if(orderBy == null)
        return null;
    
    return orderBy.map(x => `${x.field} ${x.dir}`).join(",")
}

export function requestToQueryString(request: DataViewRequest, prefix: string = "") : string {
    var result = `${prefix}skip=${request.skip}&${prefix}take=${request.take}`;

    var filterString = filterToString(request.filter);
    var orderByString = orderByToString(request.orderBy);
    
    if(filterString != null)
        result += `&${prefix}filter=${encodeURIComponent(filterString)}`;

    if(orderByString != null)
        result += `&${prefix}orderBy=${encodeURIComponent(orderByString)}`;

    return result;
}