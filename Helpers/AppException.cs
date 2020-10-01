using System;
using System.Globalization;

namespace DatingApp.API.Helpers
{
    // Custom exception class for throwing application specific exceptions that can be caugth and handled within the appication
    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
