using System.Collections.Generic;
using System.Linq;

using Realms;

namespace DailyPrayer.Models
{
    public interface FeastsRepository<T> where T : class
    {
        void Insert(T entity);
        void Update(T entity);
        //void Delete(T entity);
        //IEnumerable<T> SearchFor(Expression<Func<T, bool>> predicate);
        IEnumerable<T> List();
        T GetById(int id);
    }

    public class FeastsRepository : FeastsRepository<FeastInfoObject>
    {
        private Realm _dbContext;

        public void Repository(Realm dbContext)
        {
            _dbContext = dbContext;
        }

        public FeastInfoObject GetById(int id)
        {
            return _dbContext.Find<FeastInfoObject>(id);
        }

        public IEnumerable<FeastInfoObject> List()
        {
            IList<FeastInfoObject> feasts = _dbContext.All<FeastInfoObject>().ToList();
            return _dbContext.All<FeastInfoObject>().ToList();
        }

        //public IEnumerable<FeastInfoObject> SearchFor(Expression<Func<FeastInfoObject, bool>> predicate)
        //{
        //    return _dbContext<FeastInfoObject>.Where(predicate);
        //}

        public IEnumerable<FeastInfoObject> GetAll()
        {
            return _dbContext.All<FeastInfoObject>();
        }

        //public virtual IEnumerable<FeastInfoObject> List(System.Linq.Expressions.Expression<Func<FeastInfoObject, bool>> predicate)
        //{
        //    return _dbContext.Set<FeastInfoObject>()
        //           .Where(predicate)
        //           .AsEnumerable();
        //}

        public void Insert(FeastInfoObject entity)
        {
            _dbContext.Add<FeastInfoObject>(entity);
        }

        public void Update(FeastInfoObject entity)
        {
            //_dbContext.Entry(entity).State = EntityState.Modified;
            //_dbContext.WriteAsync();
        }

        //public void Delete(T entity)
        //{
        //    _dbContext.Set<T>().Remove(entity);
        //    _dbContext.SaveChanges();
        //}

    }
}
