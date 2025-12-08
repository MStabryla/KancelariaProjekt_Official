import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MainComponent } from './main/main.component';
import { AuthGuard } from './shared/guards/auth.guard';
import { AccessDeniedComponent } from './errors/access-denied/access-denied.component';
import { invoiceTableComponent } from './invoices/invoiceTable/invoiceTable.component';
import { CompanyComponent } from './company/company/company.component';
import { invoiceSendAll } from './invoices/invoiceSendAll/invoiceSendAll.component';
import { PaymentsTable } from './payments/paymentsTable/paymentsTable.component';
import { UserpanelComponent } from './user/userpanel/userpanel.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { CompaniesComponent } from './company/companies/companies.component';
import { PaymentMethodsComponent } from './company/payment-methods/payment-methods.component';
import { DepartmentsComponent } from './department/departments/departments.component';
import { UsersComponent } from './admin/users/users.component';
import { FtpViewComponent } from './ftp/ftp-view/ftp-view.component';
import { ContractorsTable } from './contractors/contractorsTable/contractorsTable.component';
import { DocumentsComponent } from './document/documents/documents.component';
import { LettersComponent } from './letters/letters/letters.component';
import { MainSubpageComponent } from './main-subpage/main-subpage.component';
import { MessagesComponent } from './message/messages/messages.component';
import { FtpAdminComponent } from './ftp/ftp-admin/ftp-admin.component';
import { RegisterUserComponent } from './authentication/register-user/register-user.component';
import { LoginComponent } from './authentication/login/login.component';
import { LogoutComponent } from './authentication/logout/logout.component';
import { ForgotPasswordComponent } from './authentication/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './authentication/reset-password/reset-password.component';
import { EmailConfirmationComponent } from './authentication/email-confirmation/email-confirmation.component';



//Jak chcesz połączyć jakiś widok z url to pod elementem <router-outlet> pojawi się komponent przypisany do danego adres
const routes: Routes = [
  { path: "", component: MainComponent },
  { path: 'mainsubpage/:id', component: MainSubpageComponent },
  //{ path: 'authentication', loadChildren: () => AuthenticationModule },
  { path: 'authentication/register', component: RegisterUserComponent },
  { path: 'authentication/login', component: LoginComponent },
  { path: 'authentication/logout', component: LogoutComponent },
  { path: 'authentication/forgotpassword', component: ForgotPasswordComponent },
  { path: 'authentication/resetpassword', component: ResetPasswordComponent },
  { path: 'authentication/emailconfirmation', component: EmailConfirmationComponent },
  { path: 'accessdenied', component: AccessDeniedComponent },
  { path: 'user', component: UserpanelComponent, canActivate: [AuthGuard] },
  { path: 'user/emailchange', component: UserpanelComponent, canActivate: [AuthGuard] },
  { path: 'invoice', component: invoiceTableComponent, canActivate: [AuthGuard] },
  { path: 'companies', component: CompaniesComponent, canActivate: [AuthGuard] /*, data: { getUrl: "api/companypanel", details: "company", objectTitle: "Companies", type: Company, sortActive: "id" }*/ },
  { path: 'company/:id', component: CompanyComponent, canActivate: [AuthGuard] },
  { path: 'company/:id/paymentMethods', component: PaymentMethodsComponent, canActivate: [AuthGuard] },
  { path: 'company/:id/departments', component: DepartmentsComponent, canActivate: [AuthGuard] },
  { path: 'sendedinvoices', component: invoiceSendAll, canActivate: [AuthGuard] },
  { path: 'payments', component: PaymentsTable, canActivate: [AuthGuard] },
  { path: 'contractors', component: ContractorsTable, canActivate: [AuthGuard] },
  { path: 'adminpanel', component: AdminPanelComponent, canActivate: [AuthGuard] },
  { path: 'adminpanel/users', component: UsersComponent, canActivate: [AuthGuard] },
  { path: 'adminpanel/files', component: FtpAdminComponent, canActivate: [AuthGuard] },
  { path: 'files', component: FtpViewComponent, canActivate: [AuthGuard] },
  { path: 'documents', component: DocumentsComponent, canActivate: [AuthGuard] },
  { path: 'letters', component: LettersComponent, canActivate: [AuthGuard] },
  { path: 'messages', component: MessagesComponent, canActivate: [AuthGuard] },
  {
    path: '**', pathMatch: 'full',
    component: MainComponent
  },
  //{ path: 'company', loadChildren: () => import('./company/company.module').then(m => m.CompanyModule), canActivate: const [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
