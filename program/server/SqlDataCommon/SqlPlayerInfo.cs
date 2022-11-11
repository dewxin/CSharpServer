using System;
using FluentNHibernate.Mapping;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectCommon.MySql;


namespace SqlDataCommon
{
	public class SqlPlayerInfo
	{
		public SqlPlayerInfo() { }

		// 玩家ID
		public virtual int PlayerID {get; set;}
		// 是否无效
		public virtual bool IsInvalid {get; set;}
		// 账号
		public virtual string Account {get; set;}
		// 密码
		public virtual string Password {get; set;}
		// 昵称
		public virtual string NickName {get; set;}
		// 头像
		public virtual string HeadIcon {get; set;}
		// 金币
		public virtual Int64 Gold {get; set;}
		// 最后下线时间
		public virtual DateTime LastLogoutTime {get; set;}

	}

	public class SqlPlayerInfoMap : ClassMap<SqlPlayerInfo>
	{
		public SqlPlayerInfoMap()
		{
			Table("sql_player_info");
			Id(x => x.PlayerID).GeneratedBy.Native(x=>x.AddParam("initial_value", "10000"));
			Map(x => x.IsInvalid).Default("false");
			Map(x => x.Account).Default("").Unique();
			Map(x => x.Password).Default("");
			Map(x => x.NickName).Default("").Index("_nickname_");
			Map(x => x.HeadIcon).Default("");
			Map(x => x.Gold).Default("0").Index("_gold_");
			Map(x => x.LastLogoutTime).Index("_lastlogouttime_");
			DynamicUpdate();
		}
	}

	public class SqlPlayerInfoExt
	{
		public int PlayerID { get {return Base.PlayerID;} set {Base.PlayerID=value;}}
		public bool IsInvalid { get {return Base.IsInvalid;} set {Base.IsInvalid=value;}}
		public string Account { get {return Base.Account;} set {Base.Account=value;}}
		public string Password { get {return Base.Password;} set {Base.Password=value;}}
		public string NickName { get {return Base.NickName;} set {Base.NickName=value;}}
		public string HeadIcon { get {return Base.HeadIcon;} set {Base.HeadIcon=value;}}
		public Int64 Gold { get {return Base.Gold;} set {Base.Gold=value;}}
		public DateTime LastLogoutTime { get {return Base.LastLogoutTime;} set {Base.LastLogoutTime=value;}}

		public SqlPlayerInfo Base { get; private set; }

		public SqlPlayerInfoExt()
		{
			Base = new SqlPlayerInfo();
		}

		public SqlPlayerInfoExt(SqlPlayerInfo info)
		{
			Base = info;
		}

		public void StoreJson()
		{
		}
	}


	public class SqlPlayerInfoExtTable : SqlConfigBase<SqlPlayerInfo, SqlPlayerInfoExt>
	{
		public SqlPlayerInfoExtTable()
		{
			TableName = nameof(SqlPlayerInfo);
		}

		public override void ParseConfigExtToDictAfterLoadFromDb(IList<SqlPlayerInfo> list)
		{
			foreach (var it in list)
			{
				if (!extTableAsDict.ContainsKey(it.PlayerID))
				{
					extTableAsDict.Add(it.PlayerID, new SqlPlayerInfoExt(it));
				}
			}
		}
	}
}

