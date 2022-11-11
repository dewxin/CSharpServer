using System;
using FluentNHibernate.Mapping;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectCommon.MySql;


namespace SqlDataCommon
{
	public class SqlServerInfo
	{
		public SqlServerInfo() { }

		// 服务器ID
		public virtual ushort ServerID {get; set;}
		// 服务器名
		public virtual string ServerName {get; set;}
		// 服务器类型
		public virtual string ServerType {get; set;}
		// 服务器IP
		public virtual string ServerIP {get; set;}
		// 服务器端口
		public virtual ushort ServerPort {get; set;}
		// 服务器状态
		public virtual short ServerState {get; set;}
		// 游戏ID
		public virtual short GameID {get; set;}

	}

	public class SqlServerInfoMap : ClassMap<SqlServerInfo>
	{
		public SqlServerInfoMap()
		{
			Table("sql_server_info");
			Id(x => x.ServerID).GeneratedBy.Native();
			Map(x => x.ServerName).Default("").Unique();
			Map(x => x.ServerType).Default("").Index("_servertype_");
			Map(x => x.ServerIP).Default("");
			Map(x => x.ServerPort).Default("0");
			Map(x => x.ServerState).Default("0");
			Map(x => x.GameID).Default("0").Index("_gameid_");
			DynamicUpdate();
		}
	}

	public class SqlServerInfoExt
	{
		public ushort ServerID { get {return Base.ServerID;} set {Base.ServerID=value;}}
		public string ServerName { get {return Base.ServerName;} set {Base.ServerName=value;}}
		public string ServerType { get {return Base.ServerType;} set {Base.ServerType=value;}}
		public string ServerIP { get {return Base.ServerIP;} set {Base.ServerIP=value;}}
		public ushort ServerPort { get {return Base.ServerPort;} set {Base.ServerPort=value;}}
		public short ServerState { get {return Base.ServerState;} set {Base.ServerState=value;}}
		public short GameID { get {return Base.GameID;} set {Base.GameID=value;}}

		public SqlServerInfo Base { get; private set; }

		public SqlServerInfoExt()
		{
			Base = new SqlServerInfo();
		}

		public SqlServerInfoExt(SqlServerInfo info)
		{
			Base = info;
		}

		public void StoreJson()
		{
		}
	}


	public class SqlServerInfoExtTable : SqlConfigBase<SqlServerInfo, SqlServerInfoExt>
	{
		public SqlServerInfoExtTable()
		{
			TableName = nameof(SqlServerInfo);
		}

		public override void ParseConfigExtToDictAfterLoadFromDb(IList<SqlServerInfo> list)
		{
			foreach (var it in list)
			{
				if (!extTableAsDict.ContainsKey(it.ServerID))
				{
					extTableAsDict.Add(it.ServerID, new SqlServerInfoExt(it));
				}
			}
		}
	}
}

