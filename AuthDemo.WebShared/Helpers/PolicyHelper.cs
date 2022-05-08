using AuthDemo.WebShared.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthDemo.WebShared.Helpers
{
    public static class PolicyHelper
    {
        public static IEnumerable<string> GetAllDbSets(this ApplicationDbContext db)
        {
            var dbSetProperties = new List<PropertyInfo>();
            var properties = db.GetType().GetProperties();

            foreach (var property in properties)
            {
                var setType = property.PropertyType;

                var isDbSet = setType.IsGenericType && (typeof(DbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()) || setType.GetInterface(typeof(DbSet<>).FullName) != null);

                if (isDbSet)
                {
                    dbSetProperties.Add(property);
                }
            }

            return dbSetProperties.Select(d => d.Name).OrderBy(d => d);
        }

        public static List<string> GetAllCRUDPolicies(this ApplicationDbContext db)
        {
            IEnumerable<string> dbSets = db.GetAllDbSets();

            List<string> policies = new List<string>();

            foreach (string dbSet in dbSets)
            {
                List<string> cRud = new List<string>
                {
                    $"Create {dbSet}",
                    $"Read {dbSet}",
                    $"Update {dbSet}",
                    $"Delete {dbSet}"
                };

                policies.AddRange(cRud);
            }

            return policies.OrderBy(p => p).ToList();
        }

        public static List<string> GetCRUDPoliciesByDbSet(this ApplicationDbContext db, string dbSet)
        {
            List<string> policies = db.GetAllCRUDPolicies();
            policies = policies.Where(p => p.Contains($" {dbSet}")).OrderBy(p => p).ToList();
            return policies;
        }
    }
}
