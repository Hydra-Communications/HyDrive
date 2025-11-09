using Domain.Models;

namespace HyDrive.Application.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateUser(string firstName, string lastName, string username, string password, string email);
    Task<User?> LoginUser(string email, string password);
}