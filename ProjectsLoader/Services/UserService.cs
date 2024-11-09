using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Storages.EntitiesStorage;
using System.Net.Http;
using AutoMapper;
using ProjectsLoader.Models.Infos;

namespace ProjectsLoader.Services
{
    public class UserService
    {
        private readonly PostgresContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public UserService(PostgresContext context, IPasswordHasher passwordHasher, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByLogin(string login) 
        {
            return await _context.Users.Where(x => x.Login == login).FirstOrDefaultAsync();
        }
        
        public async Task<bool> CreateUser(UserInfo userCredentials)
        {
            var user = _mapper.Map<User>(userCredentials);
            return await CreateUser(user);
        }

        public async Task<bool> CreateUser(User user)
        {
            var passwordHash = _passwordHasher.Hash(user.Password);

            var existingUser = await _context.Users
                .Where(x => x.Id == user.Id)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with the same GUID already exists");
            }

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                Login = user.Login,
                Password = passwordHash,
            };

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUser(User user)
        {
            var passwordHash = _passwordHasher.Hash(user.Password);

            var existingUser = await _context.Users
                .Where(x => x.Id == user.Id)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("This user does not exist");

            existingUser.Login = user.Login;
            existingUser.Password = passwordHash;

            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> DeleteUser(Guid id)
        {
            var existingUser = await _context.Users
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("This user does not exist");

            _context.Users.Remove(existingUser);

            _context.SaveChanges() ;

            return true;
        }
    }
}
