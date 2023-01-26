using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Data;

/// <summary>
/// 
/// </summary>
public interface IFileTableReader {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    Table? ReadFile(string filename);

}
