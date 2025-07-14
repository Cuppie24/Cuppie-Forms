using System.Reflection.Emit;
using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;

namespace Cuppie.Application.Interfaces.DAO;

public interface IUserDao
{
    Task<OperationResult<UserEntity>> AddUserAsync(UserEntity user);
    Task<OperationResult<string>> UpdateUserAsync(UserEntity updatedUser, int userId);
    Task<OperationResult<string>> DeleteUserAsync(int userId);
    Task<OperationResult<UserEntity>> GetUserByIdAsync(int userId);
    Task<OperationResult<UserEntity>> GetUserByUsernameAsync(string username);
    Task<OperationResult<UserEntity>> GetUserByEmailAsync(string email);
}