using System;
using FluentNHibernate.Mapping;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectCommon.MySql;


namespace SqlDataCommon
{
	public class SqlLoginInfo
	{
		public SqlLoginInfo() { }

		// 玩家ID
		public virtual int PlayerID {get; set;}
		// 逻辑服务器ID
		public virtual ushort LogicID {get; set;}
		// 登录状态
		public virtual short LoginState {get; set;}
		// 登录时间
		public virtual DateTime LoginTime {get; set;}

	}

	public class SqlLoginInfoMap : ClassMap<SqlLoginInfo>
	{
		public SqlLoginInfoMap()
		{
			Table("sql_login_info");
			Id(x => x.PlayerID).GeneratedBy.Assigned();
			Map(x => x.LogicID).Default("0").Index("_logicid_");
			Map(x => x.LoginState).Default("0");
			Map(x => x.LoginTime).Index("_logintime_");
			DynamicUpdate();
		}
	}

	public class SqlLoginInfoExt
	{
		public int PlayerID { get {return Base.PlayerID;} set {Base.PlayerID=value;}}
		public ushort LogicID { get {return Base.LogicID;} set {Base.LogicID=value;}}
		public short LoginState { get {return Base.LoginState;} set {Base.LoginState=value;}}
		public DateTime LoginTime { get {return Base.LoginTime;} set {Base.LoginTime=value;}}

		public SqlLoginInfo Base { get; private set; }

		public SqlLoginInfoExt()
		{
			Base = new SqlLoginInfo();
		}

		public SqlLoginInfoExt(SqlLoginInfo info)
		{
			Base = info;
		}

		public void StoreJson()
		{
		}
	}


	public class SqlLoginInfoExtTable : SqlConfigBase<SqlLoginInfo, SqlLoginInfoExt>
	{
		public SqlLoginInfoExtTable()
		{
			TableName = nameof(SqlLoginInfo);
		}

		public override void ParseConfigExtToDictAfterLoadFromDb(IList<SqlLoginInfo> list)
		{
			foreach (var it in list)
			{
				if (!extTableAsDict.ContainsKey(it.PlayerID))
				{
					extTableAsDict.Add(it.PlayerID, new SqlLoginInfoExt(it));
				}
			}
		}
	}
}

