export class TableParamsModel {
  constructor() { }
  pageNumber: number;
  pageSize: number;
  sort: string;
  filters: FilterModel[];
}

export class FilterModel {
  constructor() { }
  name: string;
  value: string;
  type: string;
}
