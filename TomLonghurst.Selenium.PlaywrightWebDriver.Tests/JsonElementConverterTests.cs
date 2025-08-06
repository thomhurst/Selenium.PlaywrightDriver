using System;
using System.Text.Json;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using NUnit.Framework;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Tests;

[TestFixture]
public class JsonElementConverterTests
{
    [Test]
    public void ConvertToNativeType_Should_Convert_Number_To_Int()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("5").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.EqualTo(5));
        Assert.That(result, Is.TypeOf<int>());
    }
    
    [Test]
    public void ConvertToNativeType_Should_Convert_Double_To_Double()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("3.14").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.EqualTo(3.14));
        Assert.That(result, Is.TypeOf<double>());
    }
    
    [Test]
    public void ConvertToNativeType_Should_Convert_String_To_String()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("\"hello\"").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.EqualTo("hello"));
        Assert.That(result, Is.TypeOf<string>());
    }
    
    [Test]
    public void ConvertToNativeType_Should_Convert_Boolean_To_Boolean()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("true").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.EqualTo(true));
        Assert.That(result, Is.TypeOf<bool>());
    }
    
    [Test]
    public void ConvertToNativeType_Should_Convert_Null_To_Null()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("null").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public void ConvertToNativeType_Should_Pass_Through_Non_JsonElement()
    {
        // Arrange
        var input = 42;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(input);
        
        // Assert
        Assert.That(result, Is.EqualTo(42));
        Assert.That(result, Is.TypeOf<int>());
    }
    
    [Test]
    public void ConvertToNativeType_Should_Convert_Array()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("[1, \"hello\", true]").RootElement;
        
        // Act
        var result = JsonElementConverter.ConvertToNativeType(jsonElement);
        
        // Assert
        Assert.That(result, Is.TypeOf<object[]>());
        var array = (object?[])result!;
        Assert.That(array.Length, Is.EqualTo(3));
        Assert.That(array[0], Is.EqualTo(1));
        Assert.That(array[1], Is.EqualTo("hello"));
        Assert.That(array[2], Is.EqualTo(true));
    }
}