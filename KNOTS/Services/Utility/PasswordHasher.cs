namespace KNOTS.Services;
internal static class PasswordHasher {
        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

        public static bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}