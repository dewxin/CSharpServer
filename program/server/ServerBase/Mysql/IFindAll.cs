using System.Collections.Generic;

namespace ProjectCommon.MySql
{
    public interface IFindAll
    {
        IList<T> FindAll<T>() where T : class;
    }
}
