﻿using IntelBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntelBot.Database
{
    public interface IRepository<T> where T : ModelBase
    {
        T Delete( T item );
        Task<T> DeleteAsync( T item );
        T Delete( string id );
        Task<T> DeleteAsync( string id );
        void DeleteMany( T[] items );
        Task DeleteManyAsync( T[] items );
        void DeleteMany( string[] itemIds );
        Task DeleteManyAsync( string[] itemIds );

        T Replace( T item, bool upsert = false );
        Task<T> ReplaceAsync( T item, Expression<Func<T, bool>> customPrimaryKeyPredicate = null, bool upsert = false );
        void ReplaceMany( T[] items, bool upsert = false );
        Task ReplaceManyAsync( T[] items, bool upsert = false );

        T Insert( T item );
        Task<T> InsertAsync( T item );
        void InsertMany( T[] items );
        Task InsertManyAsync( T[] items );

        /*T Update( T item, UpdateAction[] actions = null );
        Task<T> UpdateAsync( T item, UpdateAction[] actions = null );*/

        IEnumerable<T> Query();
    }
}
