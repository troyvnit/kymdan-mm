using System;

namespace KymdanMM.Data.Infrastructure
{
    public interface IDatabaseFactory : IDisposable
    {
        KymdanMMEntities Get();
    }
}
