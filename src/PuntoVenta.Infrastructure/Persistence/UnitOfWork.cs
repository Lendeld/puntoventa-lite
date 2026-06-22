using Microsoft.EntityFrameworkCore.Storage;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Persistence;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork, IScopedService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new TransactionScope(transaction);
    }

    private sealed class TransactionScope(IDbContextTransaction transaction) : ITransactionScope
    {
        private readonly IDbContextTransaction _transaction = transaction;

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => _transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
