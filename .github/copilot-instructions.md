- Always use Tactical DDD. Entities, Aggregates, ValueObjects, Domain Services, Application Services

- Use xUnit for writing tests, don't using namespace "using Xunit;" and "using VerifyXunit;"
- Use "VerifyXunit" snapshot for Assert, don't put [UsesVerify] attribute, use "Verify" instead of "Verifier.Verify"
- Always use `Arrange`, `Act`, `Assert` comments in tests
- Always cover functionality with tests
