using System;
using System.Linq.Expressions;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Specifications
{
    public static class UserPredicates
    {
        // Expresión traducible por EF Core: DeletedAt == null (no eliminado)
        public static readonly Expression<Func<User, bool>> NotDeleted = u => u.DeletedAt == null;
    }
}