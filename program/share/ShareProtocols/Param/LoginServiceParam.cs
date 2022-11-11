using CommonRpc.RpcBase;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Param
{
    #region Client to Login

    //TODO 这部分数据加密
    [MessagePackObject]
    public class AccountData
    {
        [Key(1)]
        public string Account { get; set; }

        [Key(2)]
        public string Password { get; set; }
    }

    public enum RegisterResultEnum
    {
        None = 0,
        Succeed = 1,
        AccountAlreadyRegistered = 2,
        FailDueToDbError = 3,
    }

    [MessagePackObject]
    public class RegisterResult : RpcResult<RegisterResult>
    {
        [Key(1)]
        public RegisterResultEnum Result { get; set; } = RegisterResultEnum.Succeed;
    }


    public enum LoginResultEnum
    {
        None = 0,
        Succeed = 1,
        AccountNotExist,
        PasswordWrong,
        ServerNotAvail,
    }

    [MessagePackObject]
    public class LoginResult : RpcResult<LoginResult>
    {
        [Key(1)]
        public LoginResultEnum Result { get; set; } = LoginResultEnum.Succeed;
        [Key(2)]
        public string GateServerIP { get; set; }
        [Key(3)]
        public ushort GateServerPort { get; set; }
        [Key(4)]
        public string Token { get; set; }
    }


    #endregion
}
