using System;
using System.Collections.Generic;

namespace KNOTS.Services;
public class RoomCodeGenerator{
    private readonly Random _random = new();
    public string Generate(HashSet<string> existingCodes) {
        string code;
        do { code = _random.Next(1000, 9999).ToString(); }
        while (existingCodes.Contains(code));
        return code;
    }
}