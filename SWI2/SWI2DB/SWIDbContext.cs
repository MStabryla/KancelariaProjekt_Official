using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SWI2DB.Configuration;
using SWI2DB.Models.Account;
using SWI2DB.Models.Administrator;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Client;
using SWI2DB.Models.Company;
using SWI2DB.Models.Contractor;
using SWI2DB.Models.Department;
using SWI2DB.Models.Employee;
using SWI2DB.Models.Entries;
using SWI2DB.Models.Invoice;
using SWI2DB.Models.Messages;
using SWI2DB.Models.Payment;

namespace SWI2DB
{
    public class SWIDbContext : IdentityDbContext<User>
    {
        //private static string _testDbConnectionString = /*"Data Source=SWI2;Initial Catalog=SWI2;Integrated Security=False;User Id = swi2; Password=zaq1@WSX;"*/ "Data Source=.\\SQLEXPRESS;Initial Catalog=SWI2;Integrated Security=True";
        public SWIDbContext()
        {
        }
        public SWIDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Message> Message { get; set; }
        public DbSet<MessageReceiver> MessagesReceiver { get; set; }
        public DbSet<MessageSender> MessagesSender { get; set; }
        public DbSet<UserMessageTemplate> UserMessageTemplate { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Company> Companys { get; set; }
        public DbSet<ClientCompany> ClientCompany { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<DocumentType> DocumentType { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<UserEmail> UserEmail { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<PaymentMethodDictionary> PaymentMethodDictionary { get; set; }
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Letter> Letter { get; set; }
        public DbSet<LetterRecipent> LetterRecipents { get; set; }
        public DbSet<EntryDictionary> EntryDictionary { get; set; }
        public DbSet<InvoiceEntry> InvoiceEntry { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceContractor> InvoiceContractor { get; set; }
        public DbSet<InvoiceIssuer> InvoiceIssuer { get; set; }
        public DbSet<InvoiceSended> InvoiceSended { get; set; }
        public DbSet<SellDateName> SellDateName { get; set; }
        public DbSet<InvoiceMailTemplate> InvoiceMailTemplate { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ContractorBankAccount> ContractorBankAccount { get; set; }
        public DbSet<PaymentForInvoice> PaymentForInvoice { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            //builder/*.UseLazyLoadingProxies()*/.UseSqlServer(_testDbConnectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new SellDateNameConfiguration());

            modelBuilder.Entity<InvoiceSended>(entity =>
            {
                entity.HasOne(c => c.User).WithMany(u => u.InvoiceSendeds);
                entity.HasOne(c => c.Invoice).WithMany(u => u.InvoiceSendeds).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<InvoiceEntry>(entity =>
            {
                entity.HasOne(c => c.Invoice).WithMany(u => u.InvoiceEntries).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ContractorBankAccount>(entity =>
            {
                entity.HasOne(c => c.Contractor).WithMany(u => u.ContractorBankAccounts).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(c => c.ContractorBankAccount).WithMany(u => u.Payments).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<PaymentForInvoice>(entity =>
            {
                entity.HasOne(c => c.Payment).WithMany(u => u.PaymentsForInvoices).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Invoice).WithMany(u => u.PaymentsForInvoices).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<InvoiceContractor>(entity =>
            {
                entity.HasOne(c => c.Contractor).WithMany(u => u.InvoiceContractors).OnDelete(DeleteBehavior.SetNull);
            });
            /*        modelBuilder.Entity<User>(entity =>
                    {
                      entity.HasOne(u => u.UserDetails).WithOne(m => m.User).HasForeignKey<UserDetails>(c => c.Id);
                      entity.HasMany(u => u.SendedMessages).WithOne(m => m.User);
                      entity.HasMany(u => u.ReceivedMessages).WithOne(m => m.User);
                      entity.HasMany(u => u.UserEmails).WithOne(m => m.User);
                      entity.HasMany(u => u.InvoiceSendeds).WithOne(m => m.User);

                      entity.HasMany(u => u.Documents).WithOne(m => m.User);
                      entity.HasMany(u => u.UserMessageTemplates).WithMany(x => x.Users);
                      entity.HasOne(c => c.Client).WithOne(u => u.User).HasForeignKey<Client>(c => c.Id);
                      entity.HasOne(e => e.Employee).WithOne(u => u.User).HasForeignKey<Employee>(c => c.Id);


                    });
                    modelBuilder.Entity<UserDetails>(entity =>
                    {
                      entity.HasOne(c => c.User).WithOne(u => u.UserDetails).HasForeignKey<User>(c => c.UserDetailsId);
                    });
                    modelBuilder.Entity<UserEmail>(entity =>
                    {
                      entity.HasOne(c => c.User).WithMany(u => u.UserEmails);
                    });
                    modelBuilder.Entity<Document>(entity =>
                    {
                      entity.HasOne(u => u.DocumentType).WithMany(m => m.Documents);
                      entity.HasOne(u => u.User).WithMany(m => m.Documents);
                      entity.HasOne(u => u.Company).WithMany(m => m.Documents);
                    });
                    modelBuilder.Entity<DocumentType>(entity =>
                    {
                      entity.HasMany(c => c.Documents).WithOne(u => u.DocumentType);
                    });
                    modelBuilder.Entity<Client>(entity =>
                    {
                      entity.HasOne(c => c.User).WithOne(u => u.Client).HasForeignKey<User>(c => c.ClientId);
                      entity.HasMany(c => c.Invoices).WithOne(u => u.Client);
                      entity.HasMany(c => c.ClientCompany).WithOne(u => u.Client);
                      entity.HasOne(c => c.Department).WithMany(u => u.Clients);
                    });
                    modelBuilder.Entity<Employee>(entity =>
                    {
                      entity.HasOne(e => e.User).WithOne(u => u.Employee).HasForeignKey<User>(c => c.EmployeeId);
                      entity.HasMany(e => e.Letters).WithOne(u => u.Employee);
                      entity.HasMany(e => e.Departments).WithMany(u => u.Employees);
                    });
                    modelBuilder.Entity<Letter>(entity =>
                    {
                      entity.HasOne(e => e.LetterRecipent).WithMany(u => u.Letters);
                      entity.HasOne(e => e.Employee).WithMany(u => u.Letters);
                    });
                    modelBuilder.Entity<LetterRecipent>(entity =>
                    {
                      entity.HasMany(e => e.Letters).WithOne(u => u.LetterRecipent);
                    });
                    modelBuilder.Entity<Message>(entity =>
                    {
                      entity.HasOne(x => x.MessageReceiver).WithOne(x => x.Message).HasForeignKey<Message>(x => x.MessageReceiverId);
                      entity.HasOne(x => x.MessageSender).WithOne(x => x.Message).HasForeignKey<Message>(x => x.MessageSenderId);
                    });

                    modelBuilder.Entity<MessageSender>(entity =>
                    {
                      entity.HasOne(u => u.Message).WithOne(m => m.MessageSender).HasForeignKey<Message>(c => c.MessageSenderId);
                      entity.HasOne(u => u.User).WithMany(m => m.SendedMessages);
                    });

                    modelBuilder.Entity<MessageReceiver>(entity =>
                    {
                      entity.HasOne(u => u.Message).WithOne(m => m.MessageReceiver).HasForeignKey<Message>(c => c.MessageReceiverId);
                      entity.HasOne(u => u.User).WithMany(m => m.ReceivedMessages);

                    });
                    modelBuilder.Entity<UserMessageTemplate>(entity =>
                    {
                      entity.HasMany(u => u.Users).WithMany(x => x.UserMessageTemplates);
                    });
                    modelBuilder.Entity<Company>(entity =>
                    {
                      entity.HasMany(u => u.PaymentMethodsDictionary).WithOne(x => x.Company);
                      entity.HasMany(u => u.Contractors).WithOne(x => x.Company);
                      entity.HasMany(u => u.Invoices).WithOne(x => x.Company);
                      entity.HasMany(u => u.InvoiceIssuers).WithOne(x => x.Company);
                      entity.HasMany(u => u.InvoiceMailTemplates).WithOne(x => x.Company);
                      entity.HasMany(u => u.EntriesDictionary).WithOne(x => x.Company);
                      entity.HasMany(u => u.ClientCompany).WithOne(x => x.Company);
                      entity.HasMany(u => u.Documents).WithOne(m => m.Company);
                      entity.HasMany(u => u.Departments).WithOne(m => m.Company);
                    });
                    modelBuilder.Entity<PaymentMethodDictionary>(entity =>
                    {
                      entity.HasOne(u => u.Company).WithMany(x => x.PaymentMethodsDictionary);
                    });
                    modelBuilder.Entity<InvoiceIssuer>(entity =>
                    {
                      entity.HasOne(u => u.Company).WithMany(x => x.InvoiceIssuers);
                      entity.HasMany(u => u.Invoices).WithOne(x => x.InvoiceIssuer);

                    });
                    modelBuilder.Entity<InvoiceMailTemplate>(entity =>
                    {
                      entity.HasOne(u => u.Company).WithMany(x => x.InvoiceMailTemplates);
                    });
                    modelBuilder.Entity<EntryDictionary>(entity =>
                    {
                      entity.HasOne(u => u.Company).WithMany(x => x.EntriesDictionary);
                    });
                    modelBuilder.Entity<Contractor>(entity =>
                    {
                      entity.HasMany(u => u.InvoiceContractors).WithOne(x => x.Contractor);
                      entity.HasMany(u => u.ContractorBankAccounts).WithOne(x => x.Contractor);
                      entity.HasOne(u => u.Company).WithMany(x => x.Contractors);
                    });
                    modelBuilder.Entity<ContractorBankAccount>(entity =>
                    {
                      entity.HasOne(u => u.Contractor).WithMany(x => x.ContractorBankAccounts);
                      entity.HasMany(u => u.Payments).WithOne(x => x.ContractorBankAccount);

                    });
                    modelBuilder.Entity<Department>(entity =>
                    {
                      entity.HasMany(u => u.Clients).WithOne(x => x.Department);
                      entity.HasOne(u => u.Company).WithMany(x => x.Departments);
                      entity.HasMany(u => u.Employees).WithMany(x => x.Departments);
                    });
                    modelBuilder.Entity<Invoice>(entity =>
                    {
                      entity.HasOne(u => u.InvoiceContractor).WithMany().HasForeignKey(i => i.InvoiceContractorId);
                      entity.HasOne(u => u.InvoiceIssuer).WithMany(x => x.Invoices);
                      entity.HasOne(u => u.Client).WithMany(x => x.Invoices);
                      entity.HasOne(u => u.Company).WithMany(x => x.Invoices);
                      entity.HasMany(u => u.Payments).WithMany(x => x.Invoices);
                      entity.HasMany(u => u.InvoiceEntries).WithOne(x => x.Invoice).OnDelete(DeleteBehavior.Cascade);
                      entity.HasMany(u => u.InvoiceSendeds).WithOne(x => x.Invoice).OnDelete(DeleteBehavior.Cascade);
                    });
                    modelBuilder.Entity<InvoiceSended>(entity =>
                      {
                        entity.HasOne(c => c.User).WithMany(u => u.InvoiceSendeds);
                        entity.HasOne(c => c.Invoice).WithMany(u => u.InvoiceSendeds).OnDelete(DeleteBehavior.Cascade);
                      });
                    modelBuilder.Entity<InvoiceEntry>(entity =>
                    {
                      entity.HasOne(c => c.Invoice).WithMany(u => u.InvoiceEntries).OnDelete(DeleteBehavior.Cascade);
                    });
                    modelBuilder.Entity<InvoiceContractor>(entity =>
                    {
                      entity.HasMany(u => u.Invoices).WithOne(x => x.InvoiceContractor);
                      entity.HasOne(u => u.Contractor).WithMany(x => x.InvoiceContractors);

                    });
                    modelBuilder.Entity<Payment>(entity =>
                    {
                      entity.HasMany(u => u.Invoices).WithMany(x => x.Payments);
                      entity.HasOne(u => u.ContractorBankAccount).WithMany(x => x.Payments);
                    });*/
        }
    }
}
