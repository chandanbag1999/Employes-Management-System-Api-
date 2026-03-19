using EMS.Infrastructure.Persistence;

namespace EMS.Infrastructure.UnitOfWork;

public class UnitOfWork : IDisposable
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // Ek baar mein sab save — atomicity
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}