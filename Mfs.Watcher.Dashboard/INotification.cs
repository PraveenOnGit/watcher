using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfs.Watcher.Dashboard
{
    public interface INotification
    {
        void UpdateMovieList(string dataRow);
        void UpdateConnectionStatus();
    }
}
