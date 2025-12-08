import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DepartmentViewComponent } from '../../department/department-view/department-view.component';
import { Department } from '../../department/department.model';
import { SWIError } from '../../errors/error';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ApiService } from '../../shared/services/api.service';
import { CompanyEditComponent } from '../company-edit/company-edit.component';
import { Company } from '../company.model';
import { PaymentMethod } from '../payment-method';
import { PaymentMethodViewComponent } from '../payment-method-view/payment-method-view.component';
import { PaymentMethodCreateComponent } from '../payment-method-create/payment-method-create.component';
import { DepartmentCreateComponent } from '../../department/department-create/department-create.component';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { TableViewModel } from '../../models/tableViewModel';


@Component({
  selector: 'app-company',
  templateUrl: './company.component.html',
  styleUrls: ['./company.component.css'],
  changeDetection: ChangeDetectionStrategy.Default
})
/** company component*/
export class CompanyComponent {

  public userRole: string;

  get companyUrl() { return this.companyId !== "" ? "/api/companypanel/" + this.companyId : "/api/companypanel/" };
  get departmentUrl() {
    return this.companyId !== "" ? "/api/departmentpanel/company/" + this.companyId : "/api/departmentpanel/" };
  get paymentMethodUrl() {
    return this.companyId !== "" ? "/api/companypanel/" + this.companyId + "/paymentmethod/" : "/api/companypanel/paymentmethod/";
  };

  companyId: "";
  company: Company = new Company({ name: "wait" });
  departments: Department[] = [];
  paymentMethods: PaymentMethod[] = [];
  apiService: ApiService

  /** company ctor */
  constructor(private _api: ApiService,
    public dialog: MatDialog,
    private _auth: AuthenticationService,
    private _route: ActivatedRoute,
    private _router: Router,
    private errorD: ErrorDialogService,
    private router: Router,
    ) {
    this.apiService = _api;
    this._auth.currentUserRole.subscribe(x => {
      this.userRole = x;
      this.companyId = _route.snapshot.params.id;
      this.refresh("")
    })
    /*this._auth.currentUserRole.toPromise().then(x => {
      this.userRole = x;
      this.companyId = _route.snapshot.params.id;
      this.refresh("");
    }).catch(x => { console.error(x)})*/
  }

  refresh(resource: string) {
    if (resource === "departments")
      this.apiService.get(this.departmentUrl).toPromise().then(x => this.setDepartments(x as TableViewModel)).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      })
    else if (resource === "payment-methods") {
      this.apiService.get(this.paymentMethodUrl).toPromise().then(x => this.setPaymentMethods(x as TableViewModel)).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      })
    }
    else {
      this.apiService.get(this.companyUrl).toPromise().then(x => this.showCompany(x)).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      })
      this.apiService.get(this.departmentUrl).toPromise().then(x => this.setDepartments(x as TableViewModel)).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      })
      this.apiService.get(this.paymentMethodUrl).toPromise().then(x => this.setPaymentMethods(x as TableViewModel)).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      })
    }
    
  }

  showCompany(x: unknown) {
    this.company = new Company(x);
  }
  setDepartments(list: TableViewModel)
  {
    this.departments = list.elements.map(x => new Department(x));
  }
  setPaymentMethods(list: TableViewModel) {
    this.paymentMethods = list.elements.map(x => new PaymentMethod(x));
  }
  openPay(payment: PaymentMethod) {
    this.dialog.open(PaymentMethodViewComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: payment
    }).afterClosed().subscribe(res => {
      if (res && res.op === "remove") {
        this.refresh("payment-methods");
      }
    })
  }
  createPay() {
    this.dialog.open(PaymentMethodCreateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: this.companyId
    }).afterClosed().subscribe(res => {
      if (res && res.op === "create")
        this.refresh("payment-methods");
    })
  }
  openPayList() {
    this._router.navigateByUrl("/company/" + this.company.id + "/paymentMethods");
    //this._router.navigate(['paymentMethods'])
  }
  openDep(department: Department) {
    this.dialog.open(DepartmentViewComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: department
    }).afterClosed().subscribe(res => {
      if (res && res.op === "remove") {
        this.refresh("departments");
      }
    })
  }
  createDep() {
    this.dialog.open(DepartmentCreateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: this.companyId
    }).afterClosed().subscribe(res => {
      if (res && res.op === "create")
        this.refresh("departments");
    })
  }
  openDepList() {
    this._router.navigateByUrl("company/" + this.company.id + "/departments");
  }

  editCompany() {
    this.dialog.open(CompanyEditComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: this.company
    }).afterClosed().subscribe(x => {
      if(x && x.op === "edit")
        this.company = x.data;
    })
  }
  goToContractors() {
    this.router.navigateByUrl("contractors/" + this.company.id)
  }
  goToPayments() {
    this.router.navigateByUrl("payments/" + this.company.id)
  }
  goToInvoices() {
    this.router.navigateByUrl("invoice/" + this.company.id)
  }


  forceError() {
    this._api.delete("/api/companypanel/-1").toPromise().then().catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
  }
}


