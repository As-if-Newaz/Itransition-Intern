using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Repositories
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDBContext db;
        protected readonly DbSet<T> dbSet;
        public Repository(ApplicationDBContext db)
        {
            this.db = db;
            this.dbSet = db.Set<T>();
        }

        public bool Create(T entity)
        {
            dbSet.Add(entity);
            return db.SaveChanges() > 0;
        }

        public bool Delete(T entity)
        {
            db.Entry(entity).State = EntityState.Deleted;
            dbSet.Remove(entity);
            return db.SaveChanges() > 0;
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T? Get(int id)
        {
            return dbSet.Find(id);
        }

        public IEnumerable<T> GetAll()
        {
            return dbSet.AsNoTracking().ToList();
        }

        public bool Update(T entity)
        {
            dbSet.Update(entity);
            return db.SaveChanges() > 0;
        }

        public int Count(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return dbSet.Count();
            return dbSet.Count(predicate);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Where(predicate).AsNoTracking().ToList();
        }
    }
}
