using System;
using System.Collections.Generic;

namespace KNOTS.Services;

/// <summary>
/// Generates unique room codes for newly created game rooms.
/// </summary>
/// <remarks>
/// Room codes are 4-digit numeric identifiers used for joining specific game sessions.
/// This class ensures that generated codes do not collide with currently active ones.
/// </remarks>
public class RoomCodeGenerator{
    private readonly Random _random = new();
    
    /// <summary>
    /// Generates a unique 4-digit room code that does not exist in the provided set of existing codes.
    /// </summary>
    /// <param name="existingCodes">A collection of currently active room codes to avoid duplicates.</param>
    /// <returns>A unique 4-digit room code represented as a string.</returns>
    /// <example>
    /// <code>
    /// var generator = new RoomCodeGenerator();
    /// var existing = new HashSet&lt;string&gt; { "1234", "5678" };
    /// string newCode = generator.Generate(existing); // e.g. "4829"
    /// </code>
    /// </example>
    public string Generate(HashSet<string> existingCodes) {
        string code;
        do { code = _random.Next(1000, 9999).ToString(); }
        while (existingCodes.Contains(code));
        return code;
    }
}