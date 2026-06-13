SKILL: Supabase PostgreSQL connection from .NET
- Use Npgsql EF Core provider: Npgsql.EntityFrameworkCore.PostgreSQL
- Connection string format: Host=db.[ref].supabase.co;Port=5432;Database=postgres;
  Username=postgres;Password=[pw];SSL Mode=Require;Trust Server Certificate=true
- For Railway: use Transaction mode port 6543 to avoid connection limits
- Enable connection pooling: Minimum Pool Size=1;Maximum Pool Size=20
- Run migrations at startup: app.Services.CreateScope() → dbContext.Database.MigrateAsync()
- Never store connection string in source — always env var
