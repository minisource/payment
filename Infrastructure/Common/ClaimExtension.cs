using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace Infrastructure.Common
{
    public static class ClaimExtension
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            if (!(user.Identity is ClaimsIdentity identity)) return null;

            var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (!(user.Identity is ClaimsIdentity identity)) return null;

            var userName = identity.FindFirst(ClaimTypes.Name).Value;

            return userName;
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            return identity?.FindFirst(claimType)?.Value;
        }

        public static string FindFirstValue(this IIdentity identity, string claimType)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity?.FindFirstValue(claimType);
        }

        public static string GetUserId(this IIdentity identity)
        {
            return identity?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static T GetUserId<T>(this IIdentity identity) where T : IConvertible
        {
            var userId = identity?.GetUserId();
            return userId.HasValue()
                ? (T)Convert.ChangeType(userId, typeof(T), CultureInfo.InvariantCulture)
                : default;
        }

        public static string GetUserName(this IIdentity identity)
        {
            return identity?.FindFirstValue(ClaimTypes.Name);
        }
    }
}