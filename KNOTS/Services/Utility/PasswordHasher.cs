namespace KNOTS.Services;

/// <summary>
/// Provides password hashing and verification using the BCrypt algorithm.
/// </summary>
internal static class PasswordHasher {
    
    /// <summary>
    /// Hashes a plain-text password using BCrypt with enhanced security.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The hashed password as a string.</returns>
        public static string Hash(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

    /// <summary>
    /// Verifies a plain-text password against a hashed password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The hashed password to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
        public static bool Verify(string password, string hash) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}