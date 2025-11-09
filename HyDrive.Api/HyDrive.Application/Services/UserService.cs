using Domain.Models;
using HyDrive.Api;
using HyDrive.Application.Interfaces.Repositories;
using HyDrive.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace HyDrive.Application.Services;

public class UserService : IUserService
{
    private readonly AppSettings _appSettings;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUserRepository _userRepository;
    
    public UserService(AppSettings appSettings, IUserRepository userRepository)
    {
        _appSettings = appSettings;
        _userRepository = userRepository;
    }
    
    #region User Operations

    public async Task<User> CreateUser(string firstName, string lastName, string username, string password, string email)
    {
        var existingUsers = await _userRepository.GetAllAsync();
        if (existingUsers.Any(u => u.Username == username || u.Email == email))
            throw new InvalidOperationException("Username or email already exists.");

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            Email = email
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        
        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();
        return user;
    }
    
    public async Task<User?> LoginUser(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return verificationResult == PasswordVerificationResult.Success ? user : null;
    }
    
    #endregion
}