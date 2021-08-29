using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SpeedUpImportingProcess.BenchMark.Extensions.Enumeration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions
{
    public static class EfExtensions
    {
        public static Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> query)
        {
            return query.ContinueWith((result) => result?.Result?.ToList());
        }

        /// <summary>
        /// Execute a query asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="spName"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAsync<T>(this DbContext dbContext, string spName, object parameter, CommandType commandType = CommandType.StoredProcedure) where T : class
        {
            using (var cn = new SqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                await cn.OpenAsync();
                return await cn.QueryAsync<T>(spName, parameter, commandType: commandType);
            }
        }

        public static async Task<DataSet> QueryAsync(this DbContext dbContext, string spName, object parameter, int timedOutInsecond)
        {
            using (var cn = new SqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand command = new SqlCommand()
                {
                    Connection = cn,
                    CommandText = spName,
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = timedOutInsecond
                })
                {
                    command.AssignParameter(parameter);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        /// <summary>
        /// Execute paramterized SQL that selects a single value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="spName"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarAsync<T>(this DbContext dbContext, string spName, object parameter, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var cn = new SqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                await cn.OpenAsync();
                return await cn.ExecuteScalarAsync<T>(spName, parameter, commandType: commandType);
            }
        }

        /// <summary>
        /// Execute a command asynchronously
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="spName"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync(this DbContext dbContext, string spName, object parameter, CommandType commandType = CommandType.StoredProcedure, int timedout = 30)
        {
            using (var cn = new SqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                await cn.OpenAsync();
                return await cn.ExecuteAsync(spName, parameter, commandType: commandType, commandTimeout: timedout);
            }
        }

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="spName"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static async Task<(IList<T1> firstSet, IList<T2> secondSet)> QueryMultiple<T1, T2>(this DbContext dbContext, string spName, object parameter, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = new SqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                connection.Open();

                using (var multi = connection.QueryMultiple(spName, parameter, commandType: commandType))
                {
                    var result1 = (await multi.ReadAsync<T1>()).ToList();
                    var result2 = (await multi.ReadAsync<T2>()).ToList();
                    return (result1, result2);
                }
            }
        }

        public static async Task<List<TResult>> FilterOnLargeSetAsync<TSource, TResult, TValue>(
           this IQueryable<TSource> query,
           Expression<Func<TSource, TResult>> converter,
           Func<IEnumerable<TValue>, Expression<Func<TSource, bool>>> funcReturnFilterExpression,
           IEnumerable<TValue> filterSet, int setSize = 1000)
        {
            var result = await filterSet
                .Select((val, ind) => new { val, ind })
                .GroupBy(c => (c.ind / setSize))
                .TransformAsync(async (gr) =>
                {
                    var filterExpression = funcReturnFilterExpression.Invoke(gr.Select(group => group.val).ToArray());
                    return await query.Where(filterExpression).Select(converter).ToArrayAsync();
                }, 1, x => x);

            return result.ToList();
        }
    }
}
