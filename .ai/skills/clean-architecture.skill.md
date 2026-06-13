SKILL: Clean Architecture for .NET
When implementing a new feature, follow this checklist:
1. Define entity in Domain/Entities/ — no EF or external refs
2. Define repository interface in Domain/Interfaces/Repositories/
3. Define DTO in Application/{Feature}/DTOs/
4. Write Command or Query + Handler in Application/{Feature}/Commands or Queries/
5. Implement repository in Infrastructure/Persistence/Repositories/
6. Add EF config in Infrastructure/Configurations/
7. Add thin controller method in API/Controllers/ — call handler, return DTO
8. Register in DI: Application.AddApplication(), Infrastructure.AddInfrastructure()

LAYER VIOLATION CHECKS:
- Domain must not reference Application, Infrastructure, or API namespaces
- Application must not reference Infrastructure or API namespaces
- Infrastructure may reference Application (for interfaces) and Domain only
- API may reference Application and Infrastructure (for DI wiring only)
