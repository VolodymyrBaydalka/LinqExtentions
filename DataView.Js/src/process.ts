import { DataViewRequest, DataView, OrderBy, FilterOrGroup, Filter, FilterOperator, Group } from "./core";
import { isFilterGroup } from "./functions";

export function process<T = any>(items: T[], req: DataViewRequest): DataView<T> {
    let result = {
        skiped: req.skip,
        taken: req.take,
        total: items.length,
        items: items   
    }

    result.items = filter(result.items, req.filter);
    result.items = orderBy(result.items, req.orderBy);
    result.items = skipAndTake(result.items, req.skip, req.take);

    return result;
}

export function skipAndTake<T = any>(items: T[], skip: number, take: number): T[] {
    if(skip == 0 && take == 0)
        return items;

    let start = skip ?? 0;

    if (start > items.length)
        return [];
    
    let end = (take == null || take == 0) ? undefined : Math.min(items.length, start + take);

    return items.slice(start, end);
}

export function orderBy<T = any>(items: T[], desc: OrderBy[]): T[] {
    if (desc == null || desc.length == 0)
        return items;

    return items.sort((x: any, y: any) => {
        for (let o of desc) {
            let res = x[o.field] - y[o.field];

            if (res != 0)
                return Math.sign(res) * (o.dir == "desc" ? -1 : 1);
        }

        return 0;
    })
}

export function filter<T = any>(items: T[], desc: FilterOrGroup): T[] {
    if(desc == null)
        return items;

    return items.filter(filterOrGroupFunc(desc));
}

function filterOrGroupFunc(filter: FilterOrGroup): (x: any) => boolean {
    return isFilterGroup(filter) ? groupFunc(filter) : filterFunc(filter);
}

function groupFunc(group: Group) : (x: any) => boolean {
    var funcs = group.filters.map(f => filterOrGroupFunc(f));

    return group.logic == "and" 
        ? x => funcs.every(f => f(x)) 
        : x => funcs.some(f => f(x)); 
}

function filterFunc(filter: Filter) : (x: any) => boolean {
    var op = operationFunc(filter.op, filter.value);
    return x => op(x[filter.field]);
}

function operationFunc(op: FilterOperator, val: any) : (x: any) => boolean {
    switch(op) {
        case "eq": return x => x == val;
        case "neq": return x => x != val;
        case "lt": return x => x < val;
        case "lte": return x => x <= val;
        case "gt": return x => x > val;
        case "gte": return x => x >= val;
        case "startsWith": return x => (x as string).indexOf(val) == 0;
        case "endsWith": return x => (x as string).indexOf(val) == x.length - val.length;
        case "contains": return x => (x as string).indexOf(val) != -1;
    }

    throw "unknown operator";
}