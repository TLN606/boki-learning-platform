using System;

namespace Boki_ELearning.Helpers
{
    public static class RoleHelper
    {
        public const string Admin = "Admin";
        public const string Student = "HocSinh";

        public static string Normalize(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return Student;
            }

            var value = role.Trim();
            if (value.Equals(Admin, StringComparison.OrdinalIgnoreCase))
            {
                return Admin;
            }

            if (value.Equals(Student, StringComparison.OrdinalIgnoreCase) ||
                value.Equals("HOCSINH", StringComparison.OrdinalIgnoreCase))
            {
                return Student;
            }

            return value;
        }

        public static bool IsAdmin(string role)
        {
            return Normalize(role).Equals(Admin, StringComparison.OrdinalIgnoreCase);
        }
    }
}
