// See https://aka.ms/new-console-template for more information
using ServerCommon;

//TODO 需要确保引用了GameServerBase程序集，不然抛出异常
ServerBoostrap serverBoostrap = new ServerBoostrap();
serverBoostrap.Start("appsettings_login.json", "appsettings_common.json");
