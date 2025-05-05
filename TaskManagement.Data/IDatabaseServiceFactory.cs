using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Data
{
    public interface IDatabaseServiceFactory
    {
        /// <summary>
        /// Creates an instance of IDatabaseService.
        /// </summary>
        /// <returns>An instance of IDatabaseService.</returns>
        Task<IDatabaseService> CreateAsync();
    }
}
