using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;
//using ProjectCommon.Unit;
using System;
using System.Collections.Generic;
using System.Reflection;

using System.Configuration;
using NHibernate.Criterion;

namespace ProjectCommon.MySql
{
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            Console.WriteLine(sql.ToString());
            return sql;
        }
    }

    public class MySqlBase : IFindAll
    {
        public MySqlBase()
        {
        }

        protected ISessionFactory sessionFactory { get; set; }


        public Action<Object> OnError { get; set; }
        

        #region 游戏服务端使用
        public void InitMySql(string connectStr, Assembly assemblyForMapping)
        {
            var config = MySQLConfiguration.Standard.ConnectionString(connectStr);
            sessionFactory = CreateSessionFactory(config, assemblyForMapping);
        }
        private ISessionFactory CreateSessionFactory(IPersistenceConfigurer sqlConfig, Assembly assembly)
        {
            return Fluently.Configure()
           .Database(sqlConfig)
           .Mappings(m =>
           {
               m.FluentMappings.AddFromAssembly(assembly);
           })
           .BuildSessionFactory();
        }
        #endregion


        #region 工具 表格导入数据库使用
        public void InitMySqlCreateTableFromAssembly(string connectStr, Assembly assembly)
        {
            var config = MySQLConfiguration.Standard.ConnectionString(connectStr);
            sessionFactory = CreateSessionFactoryExport(config, assembly);
        }

        private ISessionFactory CreateSessionFactoryExport(IPersistenceConfigurer sqlConfig, Assembly assembly)
        {
            return Fluently.Configure()
           .Database(sqlConfig)
           .Mappings(m =>
           {
               m.FluentMappings.AddFromAssembly(assembly);
           })
           .ExposeConfiguration(ExportSchema)
           .BuildSessionFactory();
        }

        private void ExportSchema(NHibernate.Cfg.Configuration config)
        {
            var showLogStr = ConfigurationManager.AppSettings["showlog"];
            if (!string.IsNullOrEmpty(showLogStr) && bool.Parse(showLogStr))
            {
                new SchemaExport(config).Create(true, true);
                config.SetProperty("show_sql", "true")
                    .SetInterceptor(new SqlStatementInterceptor());
            }
            else
            {
                new SchemaExport(config).Create(false, true);
                config.SetProperty("show_sql", "false");
            }
        }
        #endregion


        public IStatelessSession OpenStatelesSession()
        {
            return sessionFactory.OpenStatelessSession();
        }
        public ISession OpenSession()
        {
            return sessionFactory.OpenSession();
        }



        public IList<T> FindAll<T>() where T : class
        {
            using (var session = OpenStatelesSession())
            {
                return session.CreateCriteria(typeof(T)).List<T>();
            }
        }

        //todo 参数是string感觉还不够灵活，数据库字段变了，这里也要跟着改
        // 可以先用反射将 不为空的字段 转成 对应的Restrictions
        // 性能有问题的话再试试自动化生成？
        public IList<T> FindList<T>(string fieldName, object val) where T : class
        {
            using (var session = OpenStatelesSession())
            {
                var criteria = session.CreateCriteria<T>();
                //TODO orm层感觉有问题，会频繁装箱
                var list = criteria.Add(Restrictions.Eq(fieldName, val)).List<T>();
                return list;
            }
        }

        //todo 感觉有问题，肯定有很多装箱操作
        public T FindOne<T>(string fieldName, object val) where T : class
        {
            var list = FindList<T>(fieldName, val);
            if(list != null && list.Count > 0)
                return list[0];

            return null;
        }


        public T FindOneHQL<T>(string queryString, params object[] args) where T : class
        {
            var list = FindHQL<T>(queryString, 1, args);
            if (list != null && list.Count > 0)
                return list[0];
            return null;
        }

        public IList<T> FindHQL<T>(string queryString, int limit = 0, params object[] args) where T : class
        {
            if (sessionFactory == null)
                return null;

            using (var s = sessionFactory.OpenStatelessSession())
            {
                try
                {
                    var query = s.CreateQuery(queryString);
                    if (limit > 0)
                        query.SetMaxResults(limit);
                    InitHQLParams(ref query, args);
                    var list = query.List<T>();
                    return list;
                }
                catch (Exception e)
                {
                    OnError(e);
                    return null;
                }
            }
        }

        /// 更新/删除带参数
        public bool UpdateHQL(string queryString, params object[] args)
        {
            if (sessionFactory == null)
                return false;

            using (var s = sessionFactory.OpenStatelessSession())
            {
                try
                {
                    var cq = s.CreateQuery(queryString);
                    InitHQLParams(ref cq, args);
                    cq.ExecuteUpdate();
                    return true;
                }
                catch (Exception e)
                {
                    OnError(e);
                    return false;
                }
            }
        }

        public void InitHQLParams(ref IQuery query, params object[] args)
        {
            int index = 0;
            foreach (var o in args)
            {
                //query = query.SetParameter("param"+ index.ToString(), o);
                query = query.SetParameter(index, o);
                index++;
            }
        }


        public bool Update(object entity)
        {
            using (var session = OpenStatelesSession())
            {
                try
                {
                    session.Update(entity);
                    return true;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                    return false;
                }
            }
        }

        public int Insert(object entity)
        {
            using (var session = OpenStatelesSession())
            {
                try
                {
                    var result = (int)session.Insert(entity);
                    return result;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                    return -1;
                }
            }
        }


    }
}
