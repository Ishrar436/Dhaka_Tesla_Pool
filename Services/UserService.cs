using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow) => _uow = uow;

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto { Id = user.Id, Name = user.Name, Phone = user.Phone, Role = user.Role };
    }
}