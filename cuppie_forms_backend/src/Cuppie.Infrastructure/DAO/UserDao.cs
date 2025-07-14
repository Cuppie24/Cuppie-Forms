using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces.DAO;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cuppie.Infrastructure.DAO;

public class UserDao(CuppieDbContext dbContext) : IUserDao
{
    public async Task<OperationResult<UserEntity>> AddUserAsync(UserEntity user)
    {
        try
        {
            dbContext.User.Add(user);
            await dbContext.SaveChangesAsync();
            return OperationResult<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"Ошибка при добавлении нового пользователя в базу данных: {ex.Message}";
            return OperationResult<UserEntity>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<string>> UpdateUserAsync(UserEntity updatedUser, int userId)
    {
        try
        {
            var userToUpdate = await dbContext.User.FindAsync(userId);
            if(userToUpdate is null) 
                return OperationResult<string>.Failure("Невозможно обновить пользователя в БД. Такого пользователя не существует",  ErrorCode.NotFound);
            
            userToUpdate.Username = updatedUser.Username;
            userToUpdate.Email = updatedUser.Email;
            userToUpdate.PasswordHash = updatedUser.PasswordHash;
            userToUpdate.Salt = updatedUser.Salt;
            userToUpdate.UserOrganisations = updatedUser.UserOrganisations;

            await dbContext.SaveChangesAsync();
            return OperationResult<string>.Success("Пользователь обновлен успешно");
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"Ошибка при обновлении пользователя в базе данных: {ex.Message}";
            return OperationResult<string>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<string>> DeleteUserAsync(int userId)
    {
        try
        {
            var userToDelete = await dbContext.User.FindAsync(userId);
            if (userToDelete is null)
                return OperationResult<string>.Failure(
                    "Невозможно удалить пользователя из БД. Такого пользователя не существует",
                    ErrorCode.NotFound);
            dbContext.User.Remove(userToDelete);
            await dbContext.SaveChangesAsync();
            return OperationResult<string>.Success("Пользователь удален успешно");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка при удалении пользователя из БД:  {ex.Message}";
            return OperationResult<string>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<UserEntity>> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await dbContext.User.FindAsync(userId);
            if (user is null)
                return OperationResult<UserEntity>.Failure("Пользователь не найден", ErrorCode.NotFound);

            return OperationResult<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка при получении пользователя из БД: {ex.Message}";
            return OperationResult<UserEntity>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<UserEntity>> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await dbContext.User.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
                return OperationResult<UserEntity>.Failure("Пользователь с таким username не найден", ErrorCode.NotFound);

            return OperationResult<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка при получении пользователя из БД: {ex.Message}";
            return OperationResult<UserEntity>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<UserEntity>> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await dbContext.User.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return OperationResult<UserEntity>.Failure("Пользователь с таким email не найден", ErrorCode.NotFound);

            return OperationResult<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            var errorMessage = 
                $"Ошибка при получении пользователя из БД по email: {ex.Message}";
            return OperationResult<UserEntity>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }
}
