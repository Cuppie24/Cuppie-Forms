using cuppie_forms_api.Data;
using cuppie_forms_api.DTO;
using cuppie_forms_api.Models;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace cuppie_forms_api.Services;

public class AuthService
{
    private readonly CuppieDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger _logger;

    // Внедряем логгер через DI
    public AuthService(CuppieDbContext dbContext, IConfiguration config, JwtTokenService jwtTokenService, ILogger logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Чтобы убедиться, что логгер не null
    }

    public async Task<OperationResult<User>> AddUser(RegisterModelDto registerModel)
    {
        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == registerModel.Username);

        if (user != null)
        {
            _logger.Warning("Попытка зарегистрировать пользователя с уже существующим именем: {Username}", registerModel.Username);
            return OperationResult<User>.Failure("Пользователь с таким именем уже существует", ErrorCode.Conflict);
        }

        _logger.Information("Регистрация нового пользователя: {Username}", registerModel.Username);

        CryptoService cryptoService = new();
        var passSalt = cryptoService.GenerateSalt();
        user = new User
        {
            Username = registerModel.Username,
            Salt = passSalt,
            PasswordHash = cryptoService.HashPassword(registerModel.Password, passSalt),
            Email = registerModel.Email
        };

        _dbContext.User.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.Information("Пользователь успешно зарегистрирован: {Username}", registerModel.Username);
        return OperationResult<User>.Success(user);
    }

    public async Task<OperationResult<string>> IsAuthenticated(LoginModelDto loginModel)
    {
        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == loginModel.Username);

        if (user == null)
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        }

        CryptoService cryptoService = new();

        // Проверяем пароль
        var isPasswordValid = cryptoService.VerifyPassword(loginModel.Password, user.PasswordHash, user.Salt);

        if (!isPasswordValid)
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        }

        var jwtToken = _jwtTokenService.GenerateJwtToken(user);
        if (string.IsNullOrEmpty(jwtToken))
        {
            _logger.Error("Не удалось сгенерировать JWT токен для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Не удалось сгенерировать JWT токен", ErrorCode.InternalServerError);
        }

        _logger.Information("Сгенерирован токен для пользователя {Username}, длина токена: {TokenLength}", loginModel.Username, jwtToken.Length);

        return OperationResult<string>.Success(jwtToken);
    }

    public OperationResult<string> RefreshToken(string refreshToken)
    {
        _logger.Information("Попытка обновления токена для refreshToken: {RefreshToken}", refreshToken);

        var token = _jwtTokenService.RefreshToken(refreshToken);
        if (!string.IsNullOrEmpty(token.Data))
        {
            _logger.Information("Токен успешно обновлен, новый токен: {NewTokenLength}", token.Data.Length);
            return OperationResult<string>.Success(token.Data);
        }

        _logger.Error("Ошибка при обновлении токена: {ErrorMessage}", token.ErrorCode);
        return OperationResult<string>.Failure("Ошибка при обновлении токена", token.ErrorCode);
    }
}
