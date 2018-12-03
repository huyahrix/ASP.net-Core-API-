using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCore.Models;

namespace ApiCore.Interfaces
{
    public interface IUserService
    {
        Users Authenticate(string Usersname, string password);
        IEnumerable<Users> GetAll();
        Users GetById(string id);
        Users Create(Users Users, string password);
        Users Update(Users Users, string password = null);
        void Delete(int id);
    }
}
