using System;
using FluentNHibernate.Mapping;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectCommon.MySql;


namespace SqlDataCommon
{
	public class SqlPlayerItem
	{
		public SqlPlayerItem() { }

		// 物品id
		public virtual int GID {get; set;}
		// 玩家ID
		public virtual int PlayerID {get; set;}
		// 物品类型
		public virtual int ItemID {get; set;}
		// 物品数量
		public virtual int ItemCount {get; set;}
		// 到期时间
		public virtual DateTime EndTime {get; set;}

	}

	public class SqlPlayerItemMap : ClassMap<SqlPlayerItem>
	{
		public SqlPlayerItemMap()
		{
			Table("sql_player_item");
			Id(x => x.GID).GeneratedBy.Native();
			Map(x => x.PlayerID).Default("0").Index("_playerid_");
			Map(x => x.ItemID).Default("0").Index("_itemid_");
			Map(x => x.ItemCount).Default("0");
			Map(x => x.EndTime);
			DynamicUpdate();
		}
	}

	public class SqlPlayerItemExt
	{
		public int GID { get {return Base.GID;} set {Base.GID=value;}}
		public int PlayerID { get {return Base.PlayerID;} set {Base.PlayerID=value;}}
		public int ItemID { get {return Base.ItemID;} set {Base.ItemID=value;}}
		public int ItemCount { get {return Base.ItemCount;} set {Base.ItemCount=value;}}
		public DateTime EndTime { get {return Base.EndTime;} set {Base.EndTime=value;}}

		public SqlPlayerItem Base { get; private set; }

		public SqlPlayerItemExt()
		{
			Base = new SqlPlayerItem();
		}

		public SqlPlayerItemExt(SqlPlayerItem info)
		{
			Base = info;
		}

		public void StoreJson()
		{
		}
	}


	public class SqlPlayerItemExtTable : SqlConfigBase<SqlPlayerItem, SqlPlayerItemExt>
	{
		public SqlPlayerItemExtTable()
		{
			TableName = nameof(SqlPlayerItem);
		}

		public override void ParseConfigExtToDictAfterLoadFromDb(IList<SqlPlayerItem> list)
		{
			foreach (var it in list)
			{
				if (!extTableAsDict.ContainsKey(it.GID))
				{
					extTableAsDict.Add(it.GID, new SqlPlayerItemExt(it));
				}
			}
		}
	}
}

