using ProjectCommon.MySql;
using System;
using System.Collections.Generic;

namespace ProjectCommon.Unit
{
    public interface IConfigTableHandler
    {
        MySqlBase GetMySql();

        IConfigTableMgr BaseMgr { get; }
    }

    public interface IConfigTableMgr
    {
        void LoadConfig();
        //todo.now string改为 type泛型
        void ReloadCfgTable(string tableName);
        T GetConfigTable<T>() where T : class, ISqlConfigTable;
        T GetConfig<T>(string tableName, object id) where T : class, ISqlConfigTable;
        bool HasConfigMap(string tableName);
        bool RegeisterConfig(ISqlConfigTable cfg);
        void ClearConfig();

        Dictionary<string, ISqlConfigTable> GetTableDict();
    }

    public class ConfigTableMgrBase<THandler> : IConfigTableMgr
        where THandler : IConfigTableHandler
    {
        Dictionary<string, ISqlConfigTable> name2CfgTableDict = new Dictionary<string, ISqlConfigTable>();

        readonly THandler configHandler;

        public ConfigTableMgrBase(THandler handler)
        {
            configHandler = handler;
        }

        public void LoadConfig()
        {
            var mySql = configHandler.GetMySql();
            foreach (var it in name2CfgTableDict)
            {
                it.Value.LoadConfig(mySql);
            }
        }

        public void ReloadCfgTable(string tableName)
        {
            if (!name2CfgTableDict.ContainsKey(tableName))
                return;

            name2CfgTableDict[tableName].ReloadConfig(configHandler.GetMySql());
        }
        public Dictionary<string, ISqlConfigTable> GetTableDict()
        {
            return name2CfgTableDict;
        }

        public T GetConfigTable<T>() where T : class, ISqlConfigTable
        {
            var tableName = typeof(T).Name;
            if (!name2CfgTableDict.ContainsKey(tableName))
                return null;

            return name2CfgTableDict[tableName] as T;
        }

        public bool HasConfigMap(string tableName)
        {
            return name2CfgTableDict.ContainsKey(tableName);
        }

        public T GetConfig<T>(string tableName, object id) where T : class, ISqlConfigTable
        {
            if (!name2CfgTableDict.ContainsKey(tableName))
                return null;

            return name2CfgTableDict[tableName].GetConfig(id) as T;
        }

        public void ClearConfig()
        {
            name2CfgTableDict.Clear();
        }

        public bool RegeisterConfig(ISqlConfigTable cfg)
        {
            string tableName = cfg.TableName;
            if (name2CfgTableDict.ContainsKey(tableName))
                return false;

            name2CfgTableDict.Add(tableName, cfg);
            return true;
        }

    }
}
