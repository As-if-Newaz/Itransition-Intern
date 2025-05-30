using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UserManagement.DAL.EntityFramework
{
    public class Repository<T> : IRepository<T> where T : class
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

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Any(predicate);
        }

        public IEnumerable<T> Get()
        {
            return dbSet.ToList();
        }

        public T? Get(int id)
        {
            return dbSet.Find(id);
        }

        public bool Update(T entity)
        {
            dbSet.Update(entity);
            return db.SaveChanges() > 0;
        }

        public bool Delete(T entity)
        {

            db.Entry(entity).State = EntityState.Deleted;
            dbSet.Remove(entity);
            return db.SaveChanges() > 0;
        }
    }
}