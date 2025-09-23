using CollabSphere.Application;
using CollabSphere.Domain.UserModule.Repos;
using CollabSphere.Infrastructure.PostgreDbContext;
using CollabSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly collab_sphereContext _context;
        private IDbContextTransaction? _transaction;

        #region Register_Repo
        public IUserRepository UserRepo { get; }
        


        #endregion

        public UnitOfWork(collab_sphereContext context)
        {
            _context = context;

            #region Register_Repo
            UserRepo = new UserRepository(_context);
            

            #endregion
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
