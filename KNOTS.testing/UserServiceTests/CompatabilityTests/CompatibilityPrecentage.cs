using KNOTS.Compability;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class CompatibilityPrecentage{
    [Fact]
    public void CompatibilityPrecentage_ReturnsCorrectResult(){
        var pirmas = new CompatibilityScore("1", "2", 1, 2, new List<string>());
        var antras = new CompatibilityScore("1", "2", 0, 2, new List<string>());
        var ttrecias = new CompatibilityScore("1", "2", 2, 2, new List<string>());
        var ketvirtas = new CompatibilityScore("1", "2", 0, 0, new List<string>());
        
        Assert.Equal(50.0, pirmas.Percentage);
        Assert.Equal(0.0, antras.Percentage);
        Assert.Equal(100.0, ttrecias.Percentage);
        Assert.Equal(0.0, ketvirtas.Percentage);
    }
    
}