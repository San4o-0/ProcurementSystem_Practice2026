using ProcurementSystem.Data;
using ProcurementSystem.Models;
using ProcurementSystem.Services;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    private readonly UserService _userService;

    public AuthService(UserService userService)
    {
        _userService = userService;
    }

    public User? Login(string login, string password)
    {
        var user = _userService.GetUserByLogin(login);
        if (user == null)
            return null;

        if (_userService.IsUserBlocked(login))
            throw new InvalidOperationException("КОРИСТУВАЧ ЗАБЛОКОВАНО!");

        if (user.PasswordHash != PasswordService.Hash(password))
            return null;

        return user;
    }
}
