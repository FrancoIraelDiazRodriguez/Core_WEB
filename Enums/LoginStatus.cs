using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core_Web.Enums
{
    public enum LoginStatus
    {
        Success,
        InvalidCredentials,
        lockedOut,
        Inactive,
        PasswordExpired
    }
}
