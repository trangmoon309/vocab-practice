SKILL: EF Core Fluent API configuration
- Always use IEntityTypeConfiguration<T> per entity, never OnModelCreating directly
- Guid PKs: builder.HasKey(e => e.Id); builder.Property(e => e.Id).ValueGeneratedNever();
- Required strings: .IsRequired().HasMaxLength(N)
- Optional strings: .HasMaxLength(N) (no IsRequired)
- Enums stored as strings: .HasConversion<string>()
- UTC datetimes: .HasDefaultValueSql("NOW()") on CreatedAt
- Cascade deletes: .OnDelete(DeleteBehavior.Cascade) explicitly on all FKs
- Unique index: builder.HasIndex(e => e.Email).IsUnique()
- Never use data annotations on entity classes — Fluent API only
