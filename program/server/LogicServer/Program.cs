// See https://aka.ms/new-console-template for more information
using ServerCommon;


ServerBoostrap serverBoostrap = new ServerBoostrap();
serverBoostrap.Start("appsettings_logic.json", "appsettings_common.json");
