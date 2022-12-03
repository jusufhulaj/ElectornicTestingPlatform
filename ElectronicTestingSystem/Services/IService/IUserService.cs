using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.DTOs.Exam;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElectronicTestingSystem.Services.IService
{
    public interface IUserService
    {
        Task CreateUser(User userToCreate);
        Task<User> GetUser(string userId);
        Task<User> UpdateUser(string id, UserUpdateDTO userToUpdate);
        Task DeleteUser(string userId);

        Task<List<User>> GetAllUsers();
    }
}
