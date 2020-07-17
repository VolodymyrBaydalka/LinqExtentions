import { OrderBy, SortDirection, FilterOrGroup, Group } from "./core";

export function isOrderedBy(orderBy: OrderBy[], field: string) : SortDirection | undefined {
    return orderBy?.find(x => x.field == field)?.dir;
}

export function toggleOrderBy(orderBy: OrderBy[], field: string, mode: "replace" | "append" = "append"): OrderBy[] {
    var oldDir = isOrderedBy(orderBy, field);
    var desc: OrderBy = { field, dir: oldDir == "asc" ? "desc" : "asc" };

    if (mode == "replace")
        return [desc];

    var res = orderBy.filter(x => x.field != field);
    res.push(desc);

    return res;
}

export function isFilterGroup(filter: FilterOrGroup) : filter is Group {
    return (filter as Group).filters != null;
}

export function reduceFilter(filter: FilterOrGroup): FilterOrGroup | null {
    if (!isFilterGroup(filter))
        return filter;

    filter.filters = filter.filters.map(f => reduceFilter(f)).filter(x => x != null) as FilterOrGroup[];

    if (filter.filters.length == 0)
        return null;

    if (filter.filters.length == 1)
        return filter.filters[0];

    return filter;
}