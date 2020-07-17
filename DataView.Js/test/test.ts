import { expect } from 'chai';
import { OrderBy, isOrderedBy, toggleOrderBy, Filter, filterToString, DataViewRequest, requestToQueryString } from '../src/index';

it("isOrderedBy", () => {
  var data: OrderBy[] = [
    { field: "field1", dir: "asc" },
    { field: "field2", dir: "desc" },
  ];

  expect(isOrderedBy(data, "field1")).eq("asc");
  expect(isOrderedBy(data, "field2")).eq("desc");
  expect(isOrderedBy(data, "field3")).eq(undefined);
});

it("toggleOrderBy", () => {
  var data: OrderBy[] = [];

  data = toggleOrderBy([], "field1");

  expect(data.length).eq(1);
  expect(data[0].field).eq("field1");
  expect(data[0].dir).eq("asc");

  data = toggleOrderBy(data, "field1");

  expect(data.length).eq(1);
  expect(data[0].field).eq("field1");
  expect(data[0].dir).eq("desc");

  data = toggleOrderBy(data, "field2");

  expect(data.length).eq(2);
  expect(data[1].field).eq("field2");
  expect(data[1].dir).eq("asc");
});

describe("toString", () => {
  it("should be valid", () => {
    var filter1: Filter = { field: "field", op: "eq", value: "test" };
    var filter2: Filter = { field: "field", op: "eq", value: new Date(Date.UTC(2020, 0, 1)) };
    var filter3: Filter = { field: "field", op: "eq", value: 132 };
    var req: DataViewRequest = {
      skip: 10,
      take: 20,
      filter: {
        logic: "or",
        filters: [filter1, filter3]
      },
      orderBy: [{ field: "field", dir: "asc" }]
    }

    expect(filterToString(filter1)).eq('field eq "test"');
    expect(filterToString(filter2)).eq('field eq "2020-01-01T00:00:00.000Z"');
    expect(filterToString(filter3)).eq('field eq 132');
    expect(requestToQueryString(req)).eq(`skip=10&take=20&filter=(field%20eq%20%22test%22)or(field%20eq%20132)&orderBy=field%20asc`);
  })
})