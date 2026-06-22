using Xunit;

// Los tests de integración arrancan un host completo (WebApplicationFactory<Program>).
// FastEndpoints comparte un JsonSerializerOptions global que se copia/muta durante
// UseFastEndpoints (MapFastEndpoints). Dos arranques concurrentes corrompen la lista
// de converters → "Destination array was not long enough" y cae toda la colección.
// La colección "Integration" corre en serie, pero DomainEventsIntegrationTests vive en
// su propia colección y arrancaba en paralelo. Serializar todas las colecciones del
// assembly elimina el race (en producción solo hay un host, no aplica).
[assembly: CollectionBehavior(DisableTestParallelization = true)]
