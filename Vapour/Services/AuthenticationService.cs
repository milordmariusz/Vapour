﻿using System.CodeDom;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNet.Identity;
using Vapour.Exceptions;
using Vapour.Model;
using Vapour.State;
using Vapour.Utilities;

namespace Vapour.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly VapourDatabaseEntities _data;

        public AuthenticationService(VapourDatabaseEntities data, IPasswordHasher passwordHasher)
        {
            _data = data;
            _passwordHasher = passwordHasher;
        }

        public Task<RegistrationResult> Register(string email, string username, string password, string confirmPassword, 
            string description, int roleId = 2, decimal walletBalance = 0)
        {
            var result = RegistrationResult.Success;

            if (password != confirmPassword)
            {
                result = RegistrationResult.PasswordsDoNotMatch;
            }

            if (password.Length < 8)
            {
                result = RegistrationResult.WeakPassword;
            }

            if (!RegexUtilities.IsValidEmail(email))
            {
                result = RegistrationResult.IncorrectEmail;
            }

            var emailAccount = false;
            foreach (var dataUser in _data.Users)
            {
                if (dataUser.Email.Equals(email))
                {
                    emailAccount = true;
                }
            }

            if (emailAccount)
            {
                result = RegistrationResult.EmailAlreadyExists;
            }

            if (username.Length < 3)
            {
                result = RegistrationResult.IncorrectName;
            }

            if (result == RegistrationResult.Success)
            {
                var hashedPassword = _passwordHasher.HashPassword(password);

                var newUser = new User()
                {
                    Email = email,
                    Name = username,
                    Password = hashedPassword,
                    RoleId = roleId,
                };

                _data.Users.Add(newUser);
                _data.SaveChangesAsync();
            }

            return Task.FromResult(result);
        }

        public async Task<User> Login(string email, string password)
        {
            var storedAccount = _data.Users.First(u => u.Email.Equals(email)); 

            if (storedAccount == null)
            {
                throw new UserNotFoundException(email);
            }
            
            var passwordResult = _passwordHasher.VerifyHashedPassword(storedAccount.Password, password);
            if (passwordResult != PasswordVerificationResult.Success)
            {
                throw new InvalidPasswordException(email, password);
            }

            return storedAccount;
        }
    }
}