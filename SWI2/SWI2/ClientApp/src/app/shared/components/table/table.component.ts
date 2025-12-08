import { HttpParams } from '@angular/common/http';
import { Type } from '@angular/compiler/src/core';
import { Component, Output, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { merge, Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, startWith, switchMap } from 'rxjs/operators';
import { type } from 'os';
import { ApiService } from '../../services/api.service';
import { AuthenticationService } from '../../services/authentication.service';
import { FilterModel, TableParamsModel } from '../../../models/tableParams.model';
import { FormModel } from '../../../models/Form.model';
import { Input } from '@angular/core';
import { isMoment } from 'moment';
import { Adress } from '../adressMatInput/adressMatInput.component';
import { CodeCoutry } from '../coutryCodeMatInput/codeCoutryMatInput.component';
import { SelectionModel } from '@angular/cdk/collections';
import { EventEmitter } from '@angular/core';
@Component({
  selector: 'swi-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
/** list component*/
export class TableComponent {

  @Input() sortActive: string;
  @Input() requestUrl: string;
  @Input() headerRow: FormModel[];
  @Input() validators: Validators[];
  @Input() displayFilters: boolean;
  @Input() displaySelections: boolean;

  @Input() refreshTable: Observable<void>;
  @Input() selection: SelectionModel<any>;

  @Output() elementDblClick = new EventEmitter();

  displayedColumns: string[] = [];
  totalElementsLength = 0;
  dataSource = new MatTableDataSource<any>();
  @ViewChild(MatPaginator) paginator: MatPaginator;
  filter: FormGroup;
  @ViewChild(MatSort) sort: MatSort;
  isLoadingResults = true;
  currentCompanyId: number;

  constructor(private _api: ApiService,
    private _route: ActivatedRoute,
    public _authService: AuthenticationService,
    public dialog: MatDialog,
    private formBuilder: FormBuilder,
    private _router: Router) {
    this._authService.currentSelectedCompany.subscribe(company => this.currentCompanyId = company['id']);
  }
  ngOnInit() {
    if (this.displayFilters) {
      this.filter = this.generateForm();
    }

    this.displayedColumns = this.headerRow.map(hr => { return hr.name });
    this.displayedColumns = Array.from(new Set(this.displayedColumns));
    if (this.displaySelections) {
      this.displayedColumns.unshift('select');
    }
  }

  ngAfterViewInit() {
    merge(this.sort.sortChange, this.paginator.page, this.filter.valueChanges, this.refreshTable)
      .pipe(
        debounceTime(600),
        startWith({}),
        switchMap(() => {
          this.isLoadingResults = true;
          return this.loadData();
        }),
        map(data => {
          this.isLoadingResults = false;
          this.totalElementsLength = data.totalCount;
          return data.elements;
        }),
        catchError((err) => {
          this.isLoadingResults = false;
          return of([err]);
        })
      ).subscribe(data => {
        let tabledata = data;
        if (this.headerRow.find(x => x.name === 'adress-input') != null) {

          tabledata.forEach(td => td['adress-input'] = td.street + ' ' + td.houseNumber + (td.apartamentNumber ? ('/' + td.apartamentNumber) : '') + ', ' + td.city);
        }
        if (this.headerRow.find(x => x.name === 'codeCoutry-input') != null) {
          tabledata.forEach(td => td['codeCoutry-input'] = td.postalcode + ' ' + td.postoffice + ', ' + td.country);
        }

        this.dataSource.data = tabledata;
        if (this.displaySelections) {
          this.selection.clear();
        }
        });
  }
  loadData() {
    if (this._route.snapshot.params.id)
      this.requestUrl = this.requestUrl.replace('{id}', this._route.snapshot.params.id);
    else {
      if (this.currentCompanyId != null) {
        this.requestUrl = this.requestUrl.replace('{id}', this.currentCompanyId.toString());//podmiana na id firmy
      } else {
        this.requestUrl = this.requestUrl.replace('{id}', '');//podmiana na id firmy
      }
    }

    const tableParams = { pageNumber: this.paginator.pageIndex, pageSize: this.paginator.pageSize, sort: '' , filters: [] } as TableParamsModel;
    if (this.sort.active === 'adress-input') {
      tableParams.sort = 'street' + " " + this.sort.direction;
    }
    else if (this.sort.active === 'codeCoutry-input') {
      tableParams.sort = 'postalcode'+ " " + this.sort.direction;
    }
    else {
      tableParams.sort = this.sort.active + " " + this.sort.direction;
    }
    if (this.displayFilters) {
      let counter = 0;
      let ifisRangeField = false;
      Object.entries(this.filter.value).forEach(([k, v]) => {
        if (v) {
          if (typeof (v) === 'object') {
            Object.entries(v).forEach(([ak, av]) => {
              if (av) {
                let name = ak;
                if (typeof this.headerRow[counter].type == 'object') {
                  Object.entries(this.headerRow[counter].type).forEach(([hk,hv]) => name =name +'.'+hv);
                }
                tableParams.filters.push({ name:name, value: av, type: this.typesMap(this.headerRow[counter].type) } as FilterModel);
              }
            });
          } else {
            let name = k;
            if (typeof this.headerRow[counter].type == 'object') {
              Object.entries(this.headerRow[counter].type).forEach(([hk, hv]) => name = name + '.' + hv);
            }
            tableParams.filters.push({ name: name, value: v, type: this.typesMap(this.headerRow[counter].type) } as FilterModel);
          }
        }

        if (this.headerRow[counter].ifRange && !ifisRangeField) {
          ifisRangeField = true;
        } else {
          counter++;
          ifisRangeField = false;
        };
      });
    }
    const test =  this._api.get<{ elements: any[], totalCount: number }>(this.requestUrl, { query: btoa(JSON.stringify(tableParams)) });
    return test;
  }


  getElement(row) {/*
    delete row['adress-input'];
    delete row['codeCoutry-input'];*/
    this.elementDblClick.emit(row);
  }

  //helpers
  generateForm(): FormGroup {

    const form = {};
    const count = 0;
    this.headerRow.forEach(hr => {
      if (hr.ifRange) {
        form[hr.name + " >"] = [null, this.validators[count]];
        form[hr.name + " <"] = [null, this.validators[count]];
      } else if (hr.name === 'codeCoutry-input') {
        form[hr.name] = [new CodeCoutry(), this.validators[count]];
      } else if (hr.name === 'adress-input') {
        form[hr.name] = [new Adress(), this.validators[count]];
      } else {
        form[hr.name] = [null, this.validators[count]];
      }
    });
    const formGroup = this.formBuilder.group(form);
    return formGroup;
  }
  //select rows
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }
  //helpers
  typesMap(type: any): string {
    if (typeof type == 'object') {
      return 'enum'
    }
    let rtype = '';
    switch (type) {
      case 'boolean':
        rtype = "string";
        break;
      case 'text':
        rtype = 'string';
        break;
      case 'email':
        rtype = 'string';
        break;
      case 'date':
        rtype = 'date';
        break;
      case 'number':
        rtype = 'int';
        break;
      case 'array':
        rtype = 'array';
        break;
      case 'arrayofobjects':
        rtype = 'array';
        break;
      default:
        rtype = 'string';
        break;
    }
    return rtype;
  }

  toISOSDateString(date: string) {
    return date.slice(0, 10);
  }
}

