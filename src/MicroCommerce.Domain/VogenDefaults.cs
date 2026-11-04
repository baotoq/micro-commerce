using Vogen;

// Value objects convert to their primitive in JSON and through TypeConverter (route and query binding).
// EF Core conversion is declared in the ApiService, so the domain needs no EF Core reference (research R2).
[assembly: VogenDefaults(conversions: Conversions.SystemTextJson | Conversions.TypeConverter)]
