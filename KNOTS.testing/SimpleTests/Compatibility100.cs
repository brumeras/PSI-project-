namespace KNOTS.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using KNOTS.Compability;
using KNOTS.Services.Compability;
using KNOTS.Data;
using Xunit; 
public class Compatibility100
{   
    //Marks as if it is a test case
    [Fact]
    public void Checks_if_result_100()
    {
        var score = new CompatibilityScore("Emilija", "Kamile", 10, 10, new List<string>());
        
        //checks whether it matches or not
        Assert.Equal(100.0, score.Percentage);
    }
}