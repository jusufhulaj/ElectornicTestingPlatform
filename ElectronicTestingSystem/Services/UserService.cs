using AutoMapper;
using ElectronicTestingSystem.Data.UnitOfWork;
using ElectronicTestingSystem.Models;
using ElectronicTestingSystem.Models.DTOs;
using ElectronicTestingSystem.Models.Entities;
using ElectronicTestingSystem.Services.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectronicTestingSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateUser(User userToCreate)
        {
            _unitOfWork.Repository<User>().Create(userToCreate);
            _unitOfWork.Complete();
        }
        public async Task<User> GetUser(string userId)
        {
            var user = await _unitOfWork.Repository<User>().GetById(user => user.Id == userId).FirstOrDefaultAsync();

            return user;
        }
        public async Task<List<User>> GetAllUsers()
        {
            var users = await _unitOfWork.Repository<User>().GetAll().ToListAsync();

            return users;
        }

        public async Task<User> UpdateUser(string id, UserUpdateDTO userToUpdate)
        {
            var newUser = await GetUser(id);

            if (newUser != null)
            {
                newUser.FirstName = userToUpdate.FirstName != null ? userToUpdate.FirstName : newUser.FirstName;
                newUser.LastName = userToUpdate.LastName != null ? userToUpdate.LastName : newUser.LastName;
                newUser.Gender = userToUpdate.Gender != null ? userToUpdate.Gender : newUser.Gender;
                newUser.UserName = userToUpdate.UserName != null ? userToUpdate.UserName : newUser.UserName;
                newUser.PhoneNumber = userToUpdate.PhoneNumber != null ? userToUpdate.PhoneNumber : newUser.PhoneNumber;
                newUser.DateOfBirth = userToUpdate.DateOfBirth != null ? userToUpdate.DateOfBirth : newUser.DateOfBirth;
            }

            _unitOfWork.Repository<User>().Update(newUser);
            _unitOfWork.Complete();

            return newUser;
        }

        public async Task DeleteUser(string userId)
        {
            var user = await GetUser(userId);
            var userExams = await _unitOfWork.Repository<RequestedExams>().GetByCondition(ue => ue.UserId == user.Id).ToListAsync();

            foreach(var userExam in userExams)
            {
                userExam.User = null;
                userExam.UserId = null;
            }

            _unitOfWork.Repository<RequestedExams>().UpdateRange(userExams);
            
            _unitOfWork.Repository<User>().Delete(user);
            _unitOfWork.Complete();
        }
    }
}
