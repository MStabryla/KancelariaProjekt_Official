import { BrowserModule } from '@angular/platform-browser';
import { ErrorHandler, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MainComponent } from './main/main.component'
import { ApiService } from './shared/services/api.service';
import { JwtModule } from '@auth0/angular-jwt';
import { MenuComponent } from './menu/menu.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './imports/material.module';
import { CommonModule, DatePipe } from '@angular/common';
import { AccessDeniedComponent } from './errors/access-denied/access-denied.component';
import { invoiceTableComponent } from './invoices/invoiceTable/invoiceTable.component';
import { InvoiceEditComponent } from './invoices/invoiceEdit/invoiceEdit.component';
import { AdressMatInput } from './shared/components/adressMatInput/adressMatInput.component';
import { CompanyEditComponent } from './company/company-edit/company-edit.component';
import { PaymentMethodViewComponent } from './company/payment-method-view/payment-method-view.component';
import { DepartmentViewComponent } from './department/department-view/department-view.component';
import { DepartmentEditComponent } from './department/department-edit/department-edit.component';
import { PaymentMethodEditComponent } from './company/payment-method-edit/payment-method-edit.component';
import { ErrorDialogComponent } from './errors/error-dialog/error-dialog.component';
import { PaymentMethodCreateComponent } from './company/payment-method-create/payment-method-create.component';
import { DepartmentCreateComponent } from './department/department-create/department-create.component';
import { AskIfDialog } from './shared/components/askIfDialog/askIfDialog.component';
import { coutryCodeMatInput } from './shared/components/coutryCodeMatInput/codeCoutryMatInput.component';
import { AskForSomethingDialog } from './shared/components/askForSomethingDialog/askForSomethingDialog.component';
import { CustomFormDialog } from './shared/components/customFormDialog/customFormDialog.component';
import { TypeofPipe, SplitPipe, InstanceofDatePipe, MapOne, CurrencySymbolPipe } from './shared/helpers/custom.pipe';
import { CompaniesComponent } from './company/companies/companies.component';
import { CompanyComponent } from './company/company/company.component';
import { ErrorDialogService } from './shared/services/error-dialog-service';
import { UserpanelComponent } from './user/userpanel/userpanel.component';
import { UserEditComponent } from './user/user-edit/user-edit.component';
import { UserEmailChangeComponent } from './user/user-email-change/user-email-change.component';
import { UserChangePasswordComponent } from './user/user-change-password/user-change-password.component';
import { AlertComponent } from './alert/alert.component';
import { InvoicePdf } from './invoices/invoicePdf/invoicePdf.component';
import { InvoiceSend } from './invoices/InvoiceSend/invoiceSend.component';
import { TableComponent } from './shared/components/table/table.component';
import { invoiceSendAll } from './invoices/invoiceSendAll/invoiceSendAll.component';
import { PaymentsTable } from './payments/paymentsTable/paymentsTable.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { PaymentMethodsComponent } from './company/payment-methods/payment-methods.component';
import { DepartmentsComponent } from './department/departments/departments.component';
import { UsersComponent } from './admin/users/users.component';
import { UserViewComponent } from './admin/user-view/user-view.component';
import { GiveAccessClientComponent } from './admin/give-access-client/give-access-client.component';
import { GiveAccessEmployeeComponent } from './admin/give-access-employee/give-access-employee.component';
import { RemoveAccessClientComponent } from './admin/remove-access-client/remove-access-client.component';
import { RemoveAccessEmployeeComponent } from './admin/remove-access-employee/remove-access-employee.component';
import { FtpViewComponent } from './ftp/ftp-view/ftp-view.component';
import { FtpDirectoryComponent } from './ftp/ftp-directory/ftp-directory.component';
import { FtpFileComponent } from './ftp/ftp-file/ftp-file.component';
import { FtpFileViewComponent } from './ftp/ftp-file-view/ftp-file-view.component';
import { FtpUploadComponent } from './ftp/ftp-upload/ftp-upload.component';
import { ChangeRoleComponent } from './admin/change-role/change-role.component';
import { ContractorsTable } from './contractors/contractorsTable/contractorsTable.component';
import { DataTable } from './shared/components/dataTable/dataTable.component';
import { ContractorBankAccountTable } from './contractors/contractorBankAccountTable/contractorBankAccountTable.component';
import { paymentsEdit } from './payments/paymentsEdit/paymentsEdit.component';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from '@angular/material-moment-adapter';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { DocumentsComponent } from './document/documents/documents.component';
import { DocumentViewComponent } from './document/document-view/document-view.component';
import { DocumentCreateComponent } from './document/document-create/document-create.component';
import { LettersComponent } from './letters/letters/letters.component';
import { LettersViewComponent } from './letters/letters-view/letters-view.component';
import { LettersCreateComponent } from './letters/letters-create/letters-create.component';
import { LetterRecipientCreateComponent } from './letters/letter-recipient-create/letter-recipient-create.component';
import { FileUploadComponent } from './shared/components/file-upload/file-upload.component';
import { paymentsCsvSettelment } from './payments/paymentsCsvSettelment/paymentsCsvSettelment.component';
import { SvgIconComponent } from './shared/components/svg-icon/svg-icon.component';
import { MainSubpageComponent } from './main-subpage/main-subpage.component';
import { MessagesComponent } from './message/messages/messages.component';
import { MessageViewComponent } from './message/message-view/message-view.component';
import { MessagesCreateComponent } from './message/messages-create/messages-create.component';
import "./shared/helpers/model-helper";
import { MessageTempalteComponent } from './message/message-tempalte/message-tempalte.component';
import { InvoiceMailTemplateComponent } from './invoices/invoice-mail-template/invoice-mail-template.component'
import { FtpAdminComponent } from './ftp/ftp-admin/ftp-admin.component';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { RegisterUserComponent } from './authentication/register-user/register-user.component';
import { LoginComponent } from './authentication/login/login.component';
import { LogoutComponent } from './authentication/logout/logout.component';
import { ForgotPasswordComponent } from './authentication/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './authentication/reset-password/reset-password.component';
import { EmailConfirmationComponent } from './authentication/email-confirmation/email-confirmation.component';
import { UserAddComponent } from './admin/user-add/user-add.component';
import { CompanyCreateComponent } from './company/company-create/company-create.component';
import { CustomErrorHandlerService } from './shared/services/customErrorHandler.service';
export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'YYYY',
  },
};

export function createTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}


function tokenGetter() {
  return localStorage.getItem("jwt");
}

@NgModule({
  declarations: [
    AppComponent,
    RegisterUserComponent, LoginComponent, ForgotPasswordComponent, ResetPasswordComponent, EmailConfirmationComponent, LogoutComponent,
    MainComponent,
    MenuComponent,
    AccessDeniedComponent,
    invoiceTableComponent,
    InvoiceEditComponent,
    CompaniesComponent,
    TableComponent,
    CompanyComponent,
    CompanyEditComponent,
    CompanyCreateComponent,
    PaymentMethodsComponent,
    PaymentMethodViewComponent,
    PaymentMethodCreateComponent,
    PaymentMethodEditComponent,
    DepartmentsComponent,
    DepartmentViewComponent,
    DepartmentCreateComponent,
    DepartmentEditComponent,
    ErrorDialogComponent,
    UsersComponent,
    UserViewComponent,
    UserpanelComponent,
    UserEditComponent,
    UserEmailChangeComponent,
    UserChangePasswordComponent,
    ChangeRoleComponent,
    AdminPanelComponent,
    MessagesComponent,
    MessageViewComponent,
    MessagesCreateComponent,
    FtpViewComponent,
    FtpDirectoryComponent,
    FtpFileComponent,
    FtpFileViewComponent,
    FtpUploadComponent,
    DocumentsComponent,
    DocumentViewComponent,
    DocumentCreateComponent,
    LettersComponent,
    LettersViewComponent,
    LettersCreateComponent,
    LetterRecipientCreateComponent,
    FtpAdminComponent,
    AdressMatInput,
    coutryCodeMatInput,
    AskIfDialog,
    AlertComponent,
    AskForSomethingDialog,
    CustomFormDialog,
    TypeofPipe,
    SplitPipe,
    InstanceofDatePipe,
    InvoicePdf,
    InvoiceSend,
    invoiceSendAll,
    PaymentsTable,
    ContractorsTable,
    GiveAccessClientComponent,
    RemoveAccessClientComponent,
    GiveAccessEmployeeComponent,
    RemoveAccessEmployeeComponent,
    ContractorBankAccountTable,
    DataTable,
    paymentsEdit,
    MapOne,
    FileUploadComponent,
    paymentsCsvSettelment,
    SvgIconComponent,
    MainSubpageComponent,
    MessageTempalteComponent,
    InvoiceMailTemplateComponent,
    UserAddComponent,
    CurrencySymbolPipe
  ],
  entryComponents: [
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MaterialModule,
    HttpClientModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter
      }
    }),
    BrowserAnimationsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: (createTranslateLoader),
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    ApiService,
    { provide: ErrorHandler, useClass: CustomErrorHandlerService },
    ErrorDialogService,
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'fill' } },
    DatePipe,
    { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: true } },
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'fill' } },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS },
    { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_LOCALE, useValue: 'pl-PL' } //this.dateAdapter.setLocale('en-EN');
    //{ provide: HTTP_INTERCEPTORS, useClass: AntiForgeryService, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
