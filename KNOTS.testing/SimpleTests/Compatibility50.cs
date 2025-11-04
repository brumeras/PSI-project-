namespace KNOTS.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using KNOTS.Compability;
using KNOTS.Services.Compability;
using KNOTS.Data;
using Xunit; 

public class Compatibility50
{   
    [Fact]
    public void checks_if_Compatibility_50()
    {
        var score = new CompatibilityScore("Cat", "Dog", 5, 10, new List<string>());
        Assert.Equal(50, score.Percentage);
    }
}