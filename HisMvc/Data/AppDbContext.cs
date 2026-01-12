using HisMvc.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Staff> Staffs => Set<Staff>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderResult> OrderResults => Set<OrderResult>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Appointment>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Patient>().HasIndex(x => x.Phone);

        b.Entity<Staff>()
            .HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Appointment>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.Entity<Order>()
            .HasOne(x => x.Encounter)
            .WithMany()
            .HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        b.Entity<Service>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        b.Entity<Order>()
          .HasOne(x => x.Service)
          .WithMany()
          .HasForeignKey(x => x.ServiceId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrderResult>()
          .HasIndex(x => x.OrderId)
          .IsUnique();

        b.Entity<Invoice>()
          .HasOne(x => x.Encounter)
          .WithMany()
          .HasForeignKey(x => x.EncounterId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Invoice>()
          .HasIndex(x => x.InvoiceCode)
          .IsUnique();

        b.Entity<Invoice>()
          .Property(x => x.TotalAmount)
          .HasPrecision(18, 2);
    }
}
