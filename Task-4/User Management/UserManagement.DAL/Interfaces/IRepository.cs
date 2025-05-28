using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UserManagement.DAL.EntityFramework
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> Get();
        T? Get(int id);
        //IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        bool Create(T entity);
        bool Update(T entity);

        bool Delete(T entity);
        bool Exists(Expression<Func<T, bool>> predicate);
    }
}