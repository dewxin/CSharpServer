
using Newtonsoft.Json;
using ProjectCommon.Unit;
using System.Collections.Generic;

namespace ProjectCommon.MySql
{
    public interface ISqlConfigTable
    {
        void LoadConfig(IFindAll mysql);
        void ReloadConfig(IFindAll mysql);
        object GetConfig(object id);
        string GetJsonMap();
        string TableName { get; }
    }

    /// <summary>
    /// to support multiple database source , we need to pass the class implemented IFinddAll 
    /// </summary>
    /// <typeparam name="TypeBase">The original class stored in database</typeparam>
    /// <typeparam name="TypeExt">The class we will use in practice, parsed from TypeBase</typeparam>
    public abstract class SqlConfigBase<TypeBase, TypeExt> : ISqlConfigTable
        where TypeBase : class
        where TypeExt : class
    {
        //todo 上层调用的时候，好像都是int，会有一次装箱的开销。
        protected SortedDictionary<object, TypeExt> extTableAsDict = new SortedDictionary<object, TypeExt>();
        private bool hasLoaded = false;

        public abstract void ParseConfigExtToDictAfterLoadFromDb(IList<TypeBase> list);

        public delegate void OnConfig(ISqlConfigTable cfg);
        /// <summary>
        /// 加载通知接口，业务层配置以接受通知。
        /// </summary>
        public OnConfig OnLoadConfig { get; set; }

        public int Count
        {
            get
            {
                return extTableAsDict.Count;
            }
        }

        public string TableName { get; protected set; }

        public void LoadConfig(IFindAll mysql)
        {
            if (hasLoaded)
                return;

            var list = mysql.FindAll<TypeBase>();
            ParseConfigExtToDictAfterLoadFromDb(list);
            hasLoaded = true;

            if (OnLoadConfig != null)
                OnLoadConfig(this);
        }

        public void ReloadConfig(IFindAll mysql)
        {
            extTableAsDict.Clear();
            var list = mysql.FindAll<TypeBase>();
            ParseConfigExtToDictAfterLoadFromDb(list);

            if (OnLoadConfig != null)
                OnLoadConfig(this);
        }

        public string GetJsonMap()
        {
            return JsonConvert.SerializeObject(extTableAsDict);
        }


        public TypeExt GetTConfig(object id)
        {
            return GetConfig(id) as TypeExt;
        }

        public object GetConfig(object id)
        {
            if (extTableAsDict.ContainsKey(id))
            {
                return extTableAsDict[id];
            }
            return null;
        }

        public SortedDictionary<object, TypeExt>.Enumerator GetEnumerator()
        {
            return extTableAsDict.GetEnumerator();
        }

        public SortedDictionary<object, TypeExt> GetMap()
        {
            return extTableAsDict;
        }
    }
}
