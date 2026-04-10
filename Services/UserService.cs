using Microsoft.EntityFrameworkCore;
using ProcurementSystem.Data;
using ProcurementSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProcurementSystem.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;


        public UserService(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        // ============================
        // USERS
        // ============================

        /// <summary>
        /// Отримати всіх користувачів (для адміністратора)
        /// </summary>
        public List<User> GetAllUsers()
        {
            return _db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToList();
        }

        /// <summary>
        /// Отримати користувача за Id
        /// </summary>
        public User? GetById(int id)
        {
            return _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Перевірка, чи логін вже існує
        /// </summary>
        public bool LoginExists(string login)
        {
            return _db.Users.Any(u => u.Login == login);
        }

        /// <summary>
        /// Створення нового користувача (тільки Admin)
        /// </summary>
        public void CreateUser(
            string fullName,
            string login,
            string password,
            int roleId)
        {
            if (LoginExists(login))
                throw new InvalidOperationException("Користувач з таким логіном вже існує");

            var user = new User
            {
                FullName = fullName,
                Login = login,
                PasswordHash = PasswordService.Hash(password),
                RoleId = roleId,
                IsActive = true
            };
            _audit.Log($"Створює користувача {login}");

            _db.Users.Add(user);
            _db.SaveChanges();
        }

        /// <summary>
        /// Редагування користувача
        /// </summary>


        /// <summary>
        /// Зміна пароля користувача
        /// </summary>


        /// <summary>
        /// Блокування / розблокування користувача
        /// </summary>


        // ============================
        // ROLES
        // ============================

        /// <summary>
        /// Отримати всі ролі
        /// </summary>
        public List<Role> GetRoles()
        {
            return _db.Roles
                .OrderBy(r => r.Name)
                .ToList();
        }

        /// <summary>
        /// Отримати роль за назвою
        /// </summary>
        public Role? GetRoleByName(string roleName)
        {
            return _db.Roles.FirstOrDefault(r => r.Name == roleName);
        }
        public void DeleteUser(int userId)
        {
            var user = _db.Users.Find(userId)
                ?? throw new InvalidOperationException("User not found");
            _audit.Log($"Видаляє користувача {user.Login}");
            _db.Users.Remove(user);
            _db.SaveChanges();
        }


        public void SetActive(int userId, bool isActive)
        {
            var user = _db.Users.Single(u => u.Id == userId);
            user.IsActive = isActive;


            _db.SaveChanges();
        }

        public void UpdateUser(int userId, string fullName, int roleId)
        {
            var user = _db.Users.Find(userId);
            user.FullName = fullName;
            user.RoleId = roleId;
            _audit.Log($"Редагує користувача {user.Login}");

            _db.SaveChanges();
        }

        public void ChangePassword(int userId, string newPassword)
        {
            var user = _db.Users.Find(userId);
            user.PasswordHash = PasswordService.Hash(newPassword);
            _audit.Log($"Змінює пароль для користувача {user.Login}");

            _db.SaveChanges();
        }

        public bool IsUserBlocked(string login)
        {
            return _db.Users.Any(u => u.Login == login && !u.IsActive);
        }

        public User? GetUserByLogin(string login)
        {
            return _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login);
        }



    }
}
