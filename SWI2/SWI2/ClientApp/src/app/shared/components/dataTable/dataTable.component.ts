import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'swi-dataTable',
  templateUrl: './dataTable.component.html',
  styleUrls: ['./dataTable.component.css']
})

export class DataTable {
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild('viewTable') viewTable;

  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<any>();
  descriptions: {};
  @Input() data: { [key: string]: any }[];
  @Output() selectRow = new EventEmitter();
  lastSelectedTarget = null;

  constructor() {

  }
  ngAfterViewInit() {

    if (this.data[0]) {
      this.displayedColumns = Object.keys(this.data[0]).filter(s => !(s.includes('id') || s.includes('Id')));
      this.descriptions = this.data[0].getDescriptions();

      this.dataSource.paginator = this.paginator;
      this.dataSource.data = this.data;

/*      Object.defineProperty(this.dataSource.data, "push", {
        enumerable: false, // hide from for...in
        configurable: false, // prevent further meddling...
        writable: false, // see above ^
        value: function () {
          this.dataSource._updateChangeSubscription();
          for (var i = 0, n = this.length, l = arguments.length; i < l; i++, n++) {
            *//*RaiseMyEvent(this, n, this[n] = arguments[i]); // assign/raise your event*/
           /* this.dataSource._updateChangeSubscription();*//*

          }
          return n;
        }
      });*/

    }
  }

  markElement(data: any, rowElement) {
    rowElement.parentElement.style.background = '#777777';
    if (this.lastSelectedTarget)this.lastSelectedTarget.parentElement.style.background = 'none';
    this.lastSelectedTarget = rowElement;
    this.selectRow.emit(data);
  }
  //helpers
  toISOSDateString(date: string) {
    return date.slice(0, 10);
  }
}
