using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IZoneRepository : IGenericRepository<Zone>
    {
        Task<Zone?> GetByNameAsync(string name);
    }
}
