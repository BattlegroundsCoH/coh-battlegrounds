using System;
using System.Collections.Generic;
using System.Globalization;
using Battlegrounds.Factories;
using NUnit.Framework;

namespace Battlegrounds.Test.Factories;

[TestFixture]
public class LuaSourceFileBuilderTests {

    private LuaSourceFileBuilder _builder;

    [SetUp]
    public void Setup() {
        _builder = new LuaSourceFileBuilder();
    }

    [Test]
    public void ToString_EmptyBuilder_ReturnsEmptyString() {
        // Act
        string result = _builder.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void DeclareGlobal_String_AddsCorrectDeclaration() {
        // Act
        _builder.DeclareGlobal("testVar", "testValue");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("testVar = \"testValue\"\r\n"));
    }

    [Test]
    public void DeclareGlobal_Int_AddsCorrectDeclaration() {
        // Act
        _builder.DeclareGlobal("testVar", 42);

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("testVar = 42\r\n"));
    }

    [Test]
    public void DeclareGlobal_Boolean_AddsCorrectDeclaration() {
        // Act
        _builder.DeclareGlobal("testVar", true);
        _builder.DeclareGlobal("testVar2", false);

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("testVar = true\r\ntestVar2 = false\r\n"));
    }

    [Test]
    public void DeclareGlobal_Double_AddsCorrectDeclaration() {
        // Act
        _builder.DeclareGlobal("testVar", 3.14);

        // Assert
        string expected = $"testVar = {3.14.ToString(CultureInfo.InvariantCulture)}\r\n";
        Assert.That(_builder.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void IncreaseIndent_IncreasesIndentation() {
        // Act
        _builder.IncreaseIndent().DeclareGlobal("testVar", "testValue");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("    testVar = \"testValue\"\r\n"));
    }

    [Test]
    public void DecreaseIndent_DecreasesIndentation() {
        // Arrange
        _builder.IncreaseIndent().IncreaseIndent();

        // Act
        _builder.DecreaseIndent().DeclareGlobal("testVar", "testValue");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("    testVar = \"testValue\"\r\n"));
    }

    [Test]
    public void DecreaseIndent_WhenZero_DoesNotChangeIndentation() {
        // Act
        _builder.DecreaseIndent().DeclareGlobal("testVar", "testValue");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("testVar = \"testValue\"\r\n"));
    }

    [Test]
    public void DeclareTable_AddsCorrectTable() {
        // Act
        _builder.DeclareTable("myTable", table =>
            table.AddKeyValue("key", "value"));

        // Assert
        Assert.That(_builder.ToString(), Does.Contain("myTable = {"));
        Assert.That(_builder.ToString(), Does.Contain("    [\"key\"] = \"value\""));
        Assert.That(_builder.ToString(), Does.Contain("}"));
    }

    [Test]
    public void DeclareTable_EmptyTable_AddsEmptyTable() {
        // Act
        _builder.DeclareTable("emptyTable", _ => { });

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("emptyTable = {}\r\n"));
    }

    [Test]
    public void FluentInterface_AllowsChaining() {
        // Act
        string result = _builder
            .DeclareGlobal("var1", "value1")
            .IncreaseIndent()
            .DeclareGlobal("var2", 42)
            .DecreaseIndent()
            .DeclareGlobal("var3", true)
            .ToString();

        // Assert
        Assert.That(result, Does.Contain("var1 = \"value1\""));
        Assert.That(result, Does.Contain("    var2 = 42"));
        Assert.That(result, Does.Contain("var3 = true"));
    }

    [Test]
    public void WriteToFile_WritesContentToFile() {
        // Arrange
        string testFilePath = Path.Combine(Path.GetTempPath(), "LuaSourceFileBuilderTest.lua");
        _builder.DeclareGlobal("testVar", "testValue");

        try {
            // Act
            _builder.WriteToFile(testFilePath);

            // Assert
            Assert.That(File.Exists(testFilePath), Is.True);
            string fileContent = File.ReadAllText(testFilePath);
            Assert.That(fileContent, Is.EqualTo("testVar = \"testValue\"\r\n"));
        } finally {
            // Clean up
            if (File.Exists(testFilePath)) {
                File.Delete(testFilePath);
            }
        }
    }

    [Test]
    public void WriteToFile_ReturnsBuilderForChaining() {
        // Arrange
        string testFilePath = Path.Combine(Path.GetTempPath(), "LuaSourceFileBuilderTest.lua");

        try {
            // Act
            var result = _builder
                .DeclareGlobal("var1", "value1")
                .WriteToFile(testFilePath)
                .DeclareGlobal("var2", 42);

            // Assert
            Assert.That(File.Exists(testFilePath), Is.True);
            string fileContent = result.ToString();
            Assert.That(fileContent, Does.Contain("var1 = \"value1\""));
            Assert.That(fileContent, Does.Contain("var2 = 42"));
        } finally {
            // Clean up
            if (File.Exists(testFilePath)) {
                File.Delete(testFilePath);
            }
        }
    }

}

[TestFixture]
public class TableBuilderTests {
    private LuaSourceFileBuilder.TableBuilder _builder;

    [SetUp]
    public void Setup() {
        _builder = new LuaSourceFileBuilder.TableBuilder(0);
    }

    [Test]
    public void ToString_EmptyTable_ReturnsEmptyTableString() {
        // Act
        string result = _builder.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("{}"));
    }

    [Test]
    public void AddValue_String_AddsCorrectValue() {
        // Act
        _builder.AddValue("test");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("{" + Environment.NewLine + "    \"test\"" + Environment.NewLine + "}"));
    }

    [Test]
    public void AddValue_Int_AddsCorrectValue() {
        // Act
        _builder.AddValue(42);

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("{" + Environment.NewLine + "    42" + Environment.NewLine + "}"));
    }

    [Test]
    public void AddValue_Boolean_AddsCorrectValue() {
        // Act
        _builder.AddValue(true);

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("{" + Environment.NewLine + "    true" + Environment.NewLine + "}"));
    }

    [Test]
    public void AddValue_Double_AddsCorrectValue() {
        // Act
        _builder.AddValue(3.14);

        // Assert
        string expected = "{" + Environment.NewLine +
                         $"    {3.14.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine +
                         "}";
        Assert.That(_builder.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddKeyValue_StringKey_AddsCorrectKeyValue() {
        // Act
        _builder.AddKeyValue("key", "value");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("{" + Environment.NewLine + "    [\"key\"] = \"value\"" + Environment.NewLine + "}"));
    }

    [Test]
    public void AddKeyValue_IntKey_AddsCorrectKeyValue() {
        // Act
        _builder.AddKeyValue(1, "value");

        // Assert
        Assert.That(_builder.ToString(), Is.EqualTo("{" + Environment.NewLine + "    [1] = \"value\"" + Environment.NewLine + "}"));
    }

    [Test]
    public void AddNestedTable_AddsCorrectNestedTable() {
        // Act
        _builder.AddNestedTable(nested => nested.AddValue("innerValue"));

        // Assert
        string expected = 
            "{" + Environment.NewLine +
            "    {" + Environment.NewLine +
            "        \"innerValue\"" + Environment.NewLine +
            "    }" + Environment.NewLine +
            "}";
        string actual = _builder.ToString();
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AddNestedTable_WithStringKey_AddsCorrectNestedTable() {
        // Act
        _builder.AddNestedTable("nested", nested => nested.AddValue("innerValue"));

        // Assert
        Assert.That(_builder.ToString(), Does.Contain("[\"nested\"] = {"));
        Assert.That(_builder.ToString(), Does.Contain("\"innerValue\""));
    }

    [Test]
    public void AddNestedTable_WithIntKey_AddsCorrectNestedTable() {
        // Act
        _builder.AddNestedTable(1, nested => nested.AddValue("innerValue"));

        // Assert
        Assert.That(_builder.ToString(), Does.Contain("[1] = {"));
        Assert.That(_builder.ToString(), Does.Contain("\"innerValue\""));
    }

    [Test]
    public void AddDictionary_StringKeys_AddsAllEntries() {
        // Arrange
        var dict = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true },
            { "key4", 3.14 }
        };

        // Act
        _builder.AddDictionary(dict);

        // Assert
        string result = _builder.ToString();
        Assert.That(result, Does.Contain("[\"key1\"] = \"value1\""));
        Assert.That(result, Does.Contain("[\"key2\"] = 42"));
        Assert.That(result, Does.Contain("[\"key3\"] = true"));
        Assert.That(result, Does.Contain($"[\"key4\"] = {3.14.ToString(CultureInfo.InvariantCulture)}"));
    }

    [Test]
    public void AddDictionary_IntKeys_AddsAllEntries() {
        // Arrange
        var dict = new Dictionary<int, object>
        {
            { 1, "value1" },
            { 2, 42 },
            { 3, true }
        };

        // Act
        _builder.AddDictionary(dict);

        // Assert
        string result = _builder.ToString();
        Assert.That(result, Does.Contain("[1] = \"value1\""));
        Assert.That(result, Does.Contain("[2] = 42"));
        Assert.That(result, Does.Contain("[3] = true"));
    }

    [Test]
    public void AddArray_AddsAllValues() {
        // Arrange
        var array = new object[] { "text", 42, true, 3.14 };

        // Act
        _builder.AddArray(array);

        // Assert
        string result = _builder.ToString();
        Assert.That(result, Does.Contain("\"text\""));
        Assert.That(result, Does.Contain("42"));
        Assert.That(result, Does.Contain("true"));
        Assert.That(result, Does.Contain(3.14.ToString(CultureInfo.InvariantCulture)));
    }

    [Test]
    public void AddDictionary_NullDictionary_ThrowsArgumentNullException() {
        // Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddDictionary((Dictionary<string, string>)null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddDictionary((Dictionary<int, string>)null!));
    }

    [Test]
    public void AddArray_NullArray_ThrowsArgumentNullException() {
        // Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddArray((IEnumerable<string>)null!));
    }

}
