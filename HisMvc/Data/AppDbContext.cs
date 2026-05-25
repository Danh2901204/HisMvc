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
    
    // Pharmacy
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<PharmacyDispense> PharmacyDispenses => Set<PharmacyDispense>();
    public DbSet<DispenseItem> DispenseItems => Set<DispenseItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    
    // Inpatient
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<MedicalOrder> MedicalOrders => Set<MedicalOrder>();
    public DbSet<Surgery> Surgeries => Set<Surgery>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    
    // Insurance
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<InsuranceClaimItem> InsuranceClaimItems => Set<InsuranceClaimItem>();
    public DbSet<InsuranceConfig> InsuranceConfigs => Set<InsuranceConfig>();

    public DbSet<Icd10Catalog> Icd10Catalogs => Set<Icd10Catalog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Appointment>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Patient>().HasIndex(x => x.Phone);
        b.Entity<Patient>().HasIndex(x => x.IdentityNumber).IsUnique().HasFilter("[IdentityNumber] IS NOT NULL");
        b.Entity<Patient>().HasIndex(x => x.PatientCode).IsUnique().HasFilter("[PatientCode] IS NOT NULL AND [PatientCode] <> ''");

        b.Entity<Staff>()
            .HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Staff>().HasIndex(x => x.StaffCode).IsUnique().HasFilter("[StaffCode] IS NOT NULL AND [StaffCode] <> ''");

        b.Entity<Appointment>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Encounter: chuyển sang Restrict de bao toan HSBA (MoH yêu cầu luu tru lau dai)
        b.Entity<Encounter>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Encounter>()
            .HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Encounter>()
            .HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Encounter>()
            .HasIndex(x => x.EncounterCode)
            .IsUnique()
            .HasFilter("[EncounterCode] IS NOT NULL AND [EncounterCode] <> ''");

        b.Entity<Encounter>()
            .HasIndex(x => x.Status);

        // Order: chuyển sang Restrict de tranh mat HSBA
        b.Entity<Order>()
            .HasOne(x => x.Encounter)
            .WithMany()
            .HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasOne(x => x.OrderedByStaff)
            .WithMany()
            .HasForeignKey(x => x.OrderedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasIndex(x => x.OrderCode)
            .IsUnique()
            .HasFilter("[OrderCode] IS NOT NULL AND [OrderCode] <> ''");

        b.Entity<Service>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        b.Entity<Service>()
            .Property(x => x.BhytPrice)
            .HasPrecision(18, 2);

        b.Entity<Service>()
            .HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL AND [Code] <> ''");

        b.Entity<Order>()
          .HasOne(x => x.Service)
          .WithMany()
          .HasForeignKey(x => x.ServiceId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrderResult>()
          .HasIndex(x => x.OrderId)
          .IsUnique();

        b.Entity<OrderResult>()
          .HasOne(x => x.ResultedByStaff)
          .WithMany()
          .HasForeignKey(x => x.ResultedByStaffId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Invoice>()
          .HasOne(x => x.Encounter)
          .WithMany()
          .HasForeignKey(x => x.EncounterId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Invoice>()
          .HasOne(x => x.Admission)
          .WithMany()
          .HasForeignKey(x => x.AdmissionId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Invoice>()
          .HasOne(x => x.PaidByStaff)
          .WithMany()
          .HasForeignKey(x => x.PaidByStaffId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Invoice>()
          .HasIndex(x => x.InvoiceCode)
          .IsUnique();

        b.Entity<Invoice>()
          .Property(x => x.ExamFeeAmount).HasPrecision(18, 2);
        b.Entity<Invoice>()
          .Property(x => x.ServicesAmount).HasPrecision(18, 2);
        b.Entity<Invoice>()
          .Property(x => x.MedicineAmount).HasPrecision(18, 2);
        b.Entity<Invoice>()
          .Property(x => x.BedAmount).HasPrecision(18, 2);

        b.Entity<Invoice>()
          .Property(x => x.TotalAmount)
          .HasPrecision(18, 2);

        b.Entity<Invoice>()
          .Property(x => x.HasInsurance)
          .HasDefaultValue(false);

        b.Entity<Invoice>()
          .Property(x => x.InsuranceAmount)
          .HasPrecision(18, 2)
          .HasDefaultValue(0m);

        b.Entity<Invoice>()
          .Property(x => x.PatientAmount)
          .HasPrecision(18, 2)
          .HasDefaultValue(0m);

        // Pharmacy configurations
        b.Entity<Medicine>()
          .HasIndex(x => x.Code)
          .IsUnique();

        b.Entity<MedicineBatch>()
          .HasOne(x => x.Medicine)
          .WithMany()
          .HasForeignKey(x => x.MedicineId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<MedicineBatch>()
          .Property(x => x.UnitPrice)
          .HasPrecision(18, 2);

        b.Entity<Prescription>()
          .HasIndex(x => x.Code)
          .IsUnique();

        b.Entity<Prescription>()
          .HasOne(x => x.Encounter)
          .WithMany()
          .HasForeignKey(x => x.EncounterId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Prescription>()
          .HasOne(x => x.Admission)
          .WithMany()
          .HasForeignKey(x => x.AdmissionId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Prescription>()
          .HasOne(x => x.Doctor)
          .WithMany()
          .HasForeignKey(x => x.PrescribedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<MedicineBatch>()
          .HasIndex(x => new { x.MedicineId, x.BatchNumber })
          .IsUnique();

        b.Entity<InsuranceConfig>()
          .HasIndex(x => x.InsuranceType)
          .IsUnique();

        b.Entity<Icd10Catalog>().HasIndex(x => x.Name);

        b.Entity<Medicine>()
          .Property(x => x.BhytPrice)
          .HasPrecision(18, 2);

        b.Entity<PrescriptionItem>()
          .HasOne(x => x.Prescription)
          .WithMany(p => p.Items)
          .HasForeignKey(x => x.PrescriptionId)
          .OnDelete(DeleteBehavior.Cascade);

        b.Entity<PharmacyDispense>()
          .HasOne(x => x.Prescription)
          .WithMany()
          .HasForeignKey(x => x.PrescriptionId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<PharmacyDispense>()
          .HasOne(x => x.Pharmacist)
          .WithMany()
          .HasForeignKey(x => x.DispensedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<DispenseItem>()
          .HasOne(x => x.PharmacyDispense)
          .WithMany(d => d.Items)
          .HasForeignKey(x => x.PharmacyDispenseId)
          .OnDelete(DeleteBehavior.Cascade);

        b.Entity<DispenseItem>()
          .Property(x => x.UnitPrice)
          .HasPrecision(18, 2);

        b.Entity<DispenseItem>()
          .Property(x => x.TotalPrice)
          .HasPrecision(18, 2);

        b.Entity<InventoryTransaction>()
          .HasOne(x => x.MedicineBatch)
          .WithMany()
          .HasForeignKey(x => x.MedicineBatchId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<InventoryTransaction>()
          .HasOne(x => x.Staff)
          .WithMany()
          .HasForeignKey(x => x.StaffId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Admission>()
          .HasOne(x => x.DischargedByStaff)
          .WithMany()
          .HasForeignKey(x => x.DischargedBy)
          .OnDelete(DeleteBehavior.Restrict);

        // Inpatient configurations
        b.Entity<Ward>()
          .HasIndex(x => x.Code)
          .IsUnique();

        b.Entity<Ward>()
          .HasOne(x => x.Department)
          .WithMany()
          .HasForeignKey(x => x.DepartmentId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Bed>()
          .HasOne(x => x.Ward)
          .WithMany()
          .HasForeignKey(x => x.WardId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Bed>()
          .HasIndex(x => new { x.WardId, x.BedNumber })
          .IsUnique();

        b.Entity<Admission>()
          .HasIndex(x => x.AdmissionCode)
          .IsUnique();

        b.Entity<Admission>()
          .HasOne(x => x.Patient)
          .WithMany()
          .HasForeignKey(x => x.PatientId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Admission>()
          .HasOne(x => x.Bed)
          .WithMany()
          .HasForeignKey(x => x.BedId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Admission>()
          .HasOne(x => x.AttendingDoctor)
          .WithMany()
          .HasForeignKey(x => x.AttendingDoctorId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<MedicalOrder>()
          .HasOne(x => x.Admission)
          .WithMany(a => a.MedicalOrders)
          .HasForeignKey(x => x.AdmissionId)
          .OnDelete(DeleteBehavior.Cascade);

        b.Entity<MedicalOrder>()
          .HasOne(x => x.OrderedByStaff)
          .WithMany()
          .HasForeignKey(x => x.OrderedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<MedicalOrder>()
          .HasOne(x => x.ExecutedByStaff)
          .WithMany()
          .HasForeignKey(x => x.ExecutedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Surgery>()
          .HasIndex(x => x.SurgeryCode)
          .IsUnique();

        b.Entity<Surgery>()
          .HasOne(x => x.Admission)
          .WithMany()
          .HasForeignKey(x => x.AdmissionId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Surgery>()
          .HasOne(x => x.Surgeon)
          .WithMany()
          .HasForeignKey(x => x.SurgeonId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<VitalSign>()
          .HasOne(x => x.Admission)
          .WithMany()
          .HasForeignKey(x => x.AdmissionId)
          .OnDelete(DeleteBehavior.Cascade);

        b.Entity<VitalSign>()
          .HasOne(x => x.RecordedByStaff)
          .WithMany()
          .HasForeignKey(x => x.RecordedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<VitalSign>()
          .Property(x => x.Temperature)
          .HasPrecision(4, 1);

        b.Entity<VitalSign>()
          .Property(x => x.OxygenSaturation)
          .HasPrecision(5, 2);

        b.Entity<VitalSign>()
          .Property(x => x.Weight)
          .HasPrecision(5, 2);

        b.Entity<VitalSign>()
          .Property(x => x.Height)
          .HasPrecision(5, 2);

        b.Entity<Allergy>()
          .HasOne(x => x.Patient)
          .WithMany()
          .HasForeignKey(x => x.PatientId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<MedicalHistory>()
          .HasOne(x => x.Patient)
          .WithMany()
          .HasForeignKey(x => x.PatientId)
          .OnDelete(DeleteBehavior.Restrict);

        // Insurance configurations
        b.Entity<InsuranceClaim>()
          .HasIndex(x => x.ClaimCode)
          .IsUnique();

        b.Entity<InsuranceClaim>()
          .HasOne(x => x.Patient)
          .WithMany()
          .HasForeignKey(x => x.PatientId)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<InsuranceClaim>()
          .HasOne(x => x.ApprovedByStaff)
          .WithMany()
          .HasForeignKey(x => x.ApprovedBy)
          .OnDelete(DeleteBehavior.Restrict);

        b.Entity<InsuranceClaim>()
          .Property(x => x.CoveragePercent)
          .HasPrecision(5, 2);

        b.Entity<InsuranceClaim>()
          .Property(x => x.TotalAmount)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaim>()
          .Property(x => x.InsuranceCovered)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaim>()
          .Property(x => x.PatientPayment)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaimItem>()
          .HasOne(x => x.InsuranceClaim)
          .WithMany()
          .HasForeignKey(x => x.InsuranceClaimId)
          .OnDelete(DeleteBehavior.Cascade);

        b.Entity<InsuranceClaimItem>()
          .Property(x => x.UnitPrice)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaimItem>()
          .Property(x => x.TotalPrice)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaimItem>()
          .Property(x => x.InsurancePaid)
          .HasPrecision(18, 2);

        b.Entity<InsuranceClaimItem>()
          .Property(x => x.PatientPaid)
          .HasPrecision(18, 2);

        b.Entity<InsuranceConfig>()
          .Property(x => x.DefaultCoveragePercent)
          .HasPrecision(5, 2);

        b.Entity<Patient>()
          .Property(x => x.InsuranceCoveragePercent)
          .HasPrecision(5, 2)
          .HasDefaultValue(0m);

        b.Entity<PrescriptionItem>()
          .Property(x => x.Duration)
          .HasDefaultValue(1);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeEntities()
    {
        foreach (var entry in ChangeTracker.Entries<Patient>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.Phone = (entry.Entity.Phone ?? "").Trim();
                entry.Entity.FullName = (entry.Entity.FullName ?? "").Trim();
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Medicine>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.ActiveIngredient = string.IsNullOrWhiteSpace(entry.Entity.ActiveIngredient)
                    ? entry.Entity.Name
                    : entry.Entity.ActiveIngredient.Trim();
                entry.Entity.Unit = string.IsNullOrWhiteSpace(entry.Entity.Unit)
                    ? "Vien"
                    : entry.Entity.Unit.Trim();
            }
        }

        // BO: không tu set EndAt = CheckInAt nua. EndAt chi set khi Completed.
        foreach (var entry in ChangeTracker.Entries<Encounter>())
        {
            if (entry.State == EntityState.Added && entry.Entity.EndAt == default)
                entry.Entity.EndAt = DateTime.UtcNow; // placeholder, sẽ overwrite khi Close
        }

        foreach (var entry in ChangeTracker.Entries<InventoryTransaction>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                if (entry.Entity.StaffId.HasValue)
                    entry.Entity.CreatedBy ??= entry.Entity.StaffId;
                else if (entry.Entity.CreatedBy.HasValue)
                    entry.Entity.StaffId ??= entry.Entity.CreatedBy;
            }
        }

        // BO auto-fill PatientAmount - de service tinh chinh xac, tranh ghi de
        foreach (var entry in ChangeTracker.Entries<MedicalOrder>())
        {
            if (entry.State == EntityState.Added && entry.Entity.ScheduledAt == default)
                entry.Entity.ScheduledAt = entry.Entity.OrderedAt;
        }
    }
}
