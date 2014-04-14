namespace KymdanMM.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory _databaseFactory;
        private KymdanMMEntities _dataContext;

        public UnitOfWork(IDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }
        protected KymdanMMEntities DataContext
        {
            get { return _dataContext ?? (_dataContext = _databaseFactory.Get()); }
        }
        public void Commit()
        {
            DataContext.Commit();
        }
    }
}
