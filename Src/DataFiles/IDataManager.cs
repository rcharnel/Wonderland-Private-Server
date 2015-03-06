using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFiles
{
    public interface IDataManager
    {
        Task<bool> Load(string file);
        object GetObject(object a);
    }
}
