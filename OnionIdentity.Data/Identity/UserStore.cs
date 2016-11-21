using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using OnionIdentity.Data.Infrastructure;
using OnionIdentity.Data.Repositories;
using OnionIdentity.Model.Models;

namespace OnionIdentity.Data.Identity
{
    public interface IUserStore : IUserLoginStore<User, int>,
        IUserClaimStore<User, int>,
        IUserRoleStore<User, int>,
        IUserPasswordStore<User, int>,
        IUserSecurityStampStore<User, int>,
        IUserEmailStore<User, int>,
        IUserPhoneNumberStore<User, int>,
        IUserTwoFactorStore<User, int>,
        IUserLockoutStore<User, int>
    {

    }

    internal class UserStore : IUserStore
    {
        private IUnitOfWork _uow;
        private IUserRepository _userRepository;
        private IUserLoginRepository _userLoginRepository;
        private IUserClaimRepository _userClaimRepository;
        private IUserRoleRepository _userRoleRepository;
        private IRoleRepository _roleRepository;

        public UserStore(IUnitOfWork uow,
            IUserRepository userRepository,
            IUserLoginRepository userLoginRepository,
            IUserClaimRepository userClaimRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository)
        {
            _uow = uow;
            _userRepository = userRepository;
            _userLoginRepository = userLoginRepository;
            _userClaimRepository = userClaimRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed;

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && _uow != null)
            {
                if (disposing)
                {
                    _uow = null;
                    _userRepository = null;
                    _userLoginRepository = null;
                    _userClaimRepository = null;
                    _userRoleRepository = null;
                    _roleRepository = null;
                }
                _disposed = true;
            }
        }

        #region user

        public async Task CreateAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Add(user);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Update(user);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Remove(user);
            await _uow.SaveChangesAsync();
        }

        public async Task<User> FindByIdAsync(int userId)
        {
            ThrowIfDisposed();
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            return await _userRepository.FindByNameAsync(userName);
        }

        #endregion

        #region userlogin

        public async Task AddLoginAsync(User user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            _userLoginRepository.Add(new UserLogin { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey, UserId = user.Id });
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            _userLoginRepository.Remove(l => l.UserId == user.Id && l.LoginProvider == login.LoginProvider && l.UserId == user.Id);
            await _uow.SaveChangesAsync();
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var logins = await _userLoginRepository.GetByUserId(user.Id);
            return logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
        }

        public async Task<User> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null)
                throw new ArgumentNullException("login");

            var userLogin = await _userLoginRepository.FindByLoginProviderAndProviderKey(login.LoginProvider, login.ProviderKey);
            if (userLogin == null)
                return default(User);

            return await _userRepository.GetByIdAsync(userLogin.UserId);
        }

        #endregion

        #region claims

        public async Task<IList<Claim>> GetClaimsAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var claims = await _userClaimRepository.GetByUserId(user.Id);

            return claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public async Task AddClaimAsync(User user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            _userClaimRepository.Add(new UserClaim
            {
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveClaimAsync(User user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            _userClaimRepository.Remove(c => c.UserId == user.Id && c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region roles

        public async Task AddToRoleAsync(User user, string roleName)
        {
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            var role = await _roleRepository.FindByNameAsync(roleName);
            if (role == null)
                throw new InvalidOperationException("role not found");

            _userRoleRepository.Add(new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id
            });
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveFromRoleAsync(User user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            var role = await _roleRepository.FindByNameAsync(roleName);
            if (role == null)
                throw new InvalidOperationException("role not found");

            _userRoleRepository.Remove(r => r.UserId == user.Id && r.RoleId == role.Id);
            await _uow.SaveChangesAsync();
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return await _roleRepository.GetRolesNameByUserId(user.Id);
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            var role = await _roleRepository.FindByNameAsync(roleName);
            if (role == null)
                throw new InvalidOperationException("role not found");

            return await _userRoleRepository.IsInRoleAsync(user.Id, role.Id);
        }

        #endregion

        public async Task SetPasswordHashAsync(User user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.PasswordHash = passwordHash;
            await _uow.SaveChangesAsync();
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task SetSecurityStampAsync(User user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.SecurityStamp = stamp;
            await _uow.SaveChangesAsync();
        }

        public Task<string> GetSecurityStampAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        public async Task SetEmailAsync(User user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.Email = email;
            await _uow.SaveChangesAsync();
        }

        public Task<string> GetEmailAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        public async Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.EmailConfirmed = confirmed;

            await _uow.SaveChangesAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return await _userRepository.FindByEmailAsync(email);
        }

        public async Task SetPhoneNumberAsync(User user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.PhoneNumber = phoneNumber;

            await _uow.SaveChangesAsync();
        }

        public Task<string> GetPhoneNumberAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public async Task SetPhoneNumberConfirmedAsync(User user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.PhoneNumberConfirmed = confirmed;

            await _uow.SaveChangesAsync();
        }

        public async Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.TwoFactorEnabled = enabled;

            await _uow.SaveChangesAsync();
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : new DateTimeOffset());
        }

        public async Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? new DateTime?() : lockoutEnd.UtcDateTime;

            await _uow.SaveChangesAsync();
        }

        public async Task<int> IncrementAccessFailedCountAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            ++user.AccessFailedCount;

            await _uow.SaveChangesAsync();

            return user.AccessFailedCount;
        }

        public async Task ResetAccessFailedCountAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.AccessFailedCount = 0;

            await _uow.SaveChangesAsync();
        }

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEnabled);
        }

        public async Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.LockoutEnabled = enabled;
            await _uow.SaveChangesAsync();
        }
    }
}
