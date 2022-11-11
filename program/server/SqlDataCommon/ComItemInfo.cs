using System;
using FluentNHibernate.Mapping;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProjectCommon.MySql;


namespace SqlDataCommon
{
	public class ComItemInfo
	{
		public ComItemInfo() { }

		// 物品模板id
		public virtual int ItemID {get; set;}
		// 物品类型
		public virtual short ItemType {get; set;}
		// 物品逻辑参数
		public virtual string ItemLogic {get; set;}
		// 是否进入背包(客户端用)
		public virtual short IsBag {get; set;}
		// 到期时间(秒) 0为无限制
		public virtual int EndTime {get; set;}
		// 是否初始化
		public virtual bool NeedInit {get; set;}
		// 物品名称
		public virtual string ItemName {get; set;}
		// 描述
		public virtual string ItemDesc {get; set;}

	}

	public class ComItemInfoMap : ClassMap<ComItemInfo>
	{
		public ComItemInfoMap()
		{
			Table("com_item_info");
			Id(x => x.ItemID).GeneratedBy.Assigned();
			Map(x => x.ItemType).Default("0");
			Map(x => x.ItemLogic).Default("");
			Map(x => x.IsBag).Default("0");
			Map(x => x.EndTime).Default("0");
			Map(x => x.NeedInit).Default("false");
			Map(x => x.ItemName).Default("");
			Map(x => x.ItemDesc).Default("");
			DynamicUpdate();
		}
	}

	public class ComItemInfoExt
	{
		public int ItemID { get {return Base.ItemID;} set {Base.ItemID=value;}}
		public short ItemType { get {return Base.ItemType;} set {Base.ItemType=value;}}
		public string ItemLogic { get {return Base.ItemLogic;} set {Base.ItemLogic=value;}}
		public short IsBag { get {return Base.IsBag;} set {Base.IsBag=value;}}
		public int EndTime { get {return Base.EndTime;} set {Base.EndTime=value;}}
		public bool NeedInit { get {return Base.NeedInit;} set {Base.NeedInit=value;}}
		public string ItemName { get {return Base.ItemName;} set {Base.ItemName=value;}}
		public string ItemDesc { get {return Base.ItemDesc;} set {Base.ItemDesc=value;}}

		[JsonIgnore]
		public List<int> ItemLogicList = new();
		private void ParseItemLogicExt()
		{
			if (string.IsNullOrEmpty(ItemLogic)) return;
			ItemLogicList = JsonConvert.DeserializeObject<List<int>>(ItemLogic);
		}

		public ComItemInfo Base { get; private set; }

		public ComItemInfoExt()
		{
			Base = new ComItemInfo();
		}

		public ComItemInfoExt(ComItemInfo info)
		{
			Base = info;
			ParseItemLogicExt();
		}

		public void StoreJson()
		{
			ItemLogic = JsonConvert.SerializeObject(ItemLogicList);
		}
	}


	public class ComItemInfoExtTable : SqlConfigBase<ComItemInfo, ComItemInfoExt>
	{
		public ComItemInfoExtTable()
		{
			TableName = nameof(ComItemInfo);
		}

		public override void ParseConfigExtToDictAfterLoadFromDb(IList<ComItemInfo> list)
		{
			foreach (var it in list)
			{
				if (!extTableAsDict.ContainsKey(it.ItemID))
				{
					extTableAsDict.Add(it.ItemID, new ComItemInfoExt(it));
				}
			}
		}
	}
}

