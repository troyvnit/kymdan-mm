namespace KymdanMM.Data.Infrastructure
{
    public class DatabaseFactory : Disposable, IDatabaseFactory
    {
        private KymdanMMEntities _dataContext;
        public KymdanMMEntities Get()
        {
            return _dataContext ?? (_dataContext = new KymdanMMEntities());
        }
        protected override void DisposeCore()
        {
            if (_dataContext != null)
                _dataContext.Dispose();
        }
    }
}
