using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Models.DTOs;
using EmployeeManagementSystem.Repository;

namespace EmployeeManagementSystem.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository, SecurityTokenService tokenService)
        {
            _userRepository = userRepository;
        }

        public async Task<AppUser?> RegisterUserAsync(RegisterDto userDto)
        {
            // Check if the user already exists
            var userExists = await _userRepository.UserExistsAsync(userDto.UserName, userDto.Email);
            if (!userExists)
            {
                CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                // Create a new AppUser object
                var newUser = new AppUser
                {
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };
                // Add the new user to the repository
                return await _userRepository.AddUserAsync(newUser);
            }
            return null; // User already exists
        }

        public async Task<AppUser?> LoginUserAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(loginDto.Username);
            if (user == null)
            {
                return null; // User not found
            }
            // Verify the password
            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null; // Invalid password
            }
            return user; // Successful login
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
           using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
           var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
           bool isValid = true;
           for (int i = 0; i < passwordHash.Length; i++)
           {
                if (passwordHash[i] != computedHash[i])
                {
                    isValid = false;
                    break;
                }
           }
           return isValid;
        }

        // Create password hash and salt
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
