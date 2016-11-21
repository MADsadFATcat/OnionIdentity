using System;
using System.Data.Entity;
using OnionIdentity.Data.EF;

namespace OnionIdentity.Data.Infrastructure
{
    internal interface IDbFactory : IDisposable
    {
        DbContext Init();
    }

    internal class DbFactory : IDbFactory
    {
        private DbContext _db;

        public DbContext Init()
        {
            return _db ?? (_db = new ApplicationContext());
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }

                _disposed = true;
            }
        }
    }
}
