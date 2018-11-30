using System;
using System.Collections.Generic;
using System.Linq;
using ApiCore.Interfaces;
using ApiCore.Helpers;
using ApiCore.Models;



namespace ApiCore.Infrastructure
{
    public class UserService : IUserService
    {
        private NorthwindContext _context;

        public UserService(NorthwindContext context)
        {
            _context = context;
        }

        public Users Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;
            var users = _context.Users.SingleOrDefault(x => x.UserName == username);
            //check username exists
            if (users == null)
                return null;

            //check password is correct
            if (!VerifyPasswordHash(password, users.PasswordHash, users.PasswordSalt))
                return null;

            //authentication successful
            return users;
        }

        public IEnumerable<Users> GetAll()
        {
            return _context.Users;
        }

        public Users GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public Users Create(Users user, string password)
        {
            //throw new NotImplementedException();
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password required");
            if(_context.Users.Any(x=>x.UserName == user.UserName))
                throw new Exception("Username \"" + user.UserName  +"\" is taken");
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            //user.PasswordHash = System.Text.Encoding.UTF8.GetString(passwordHash);
            //user.PasswordSalt = System.Text.Encoding.UTF8.GetString(passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            //_context.Add<Users>(user);
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public Users Update(Users user, string password = null)
        {
            //validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");
            //throw new Exception("Password is required");
            if (_context.Users.Any(x => x.UserName == user.UserName))
                throw new AppException("UserName\"" + user.UserName + "\"is already taken");
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            //user.PasswordHash = System.Text.Encoding.UTF8.GetString(passwordHash);
            //user.PasswordSalt = System.Text.Encoding.UTF8.GetString(passwordSalt);

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;

        }


        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

    }

}
