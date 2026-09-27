using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByPhoneAsync(string phone);
        Task<bool> PhoneExistsAsync(string phone);
    }
}
