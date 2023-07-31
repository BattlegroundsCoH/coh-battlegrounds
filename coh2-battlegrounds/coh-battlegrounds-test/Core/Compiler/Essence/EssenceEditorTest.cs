using Battlegrounds.Compiler.Essence;

namespace Battlegrounds.Testing.Core.Compiler.Essence;

public class EssenceEditorTest {

    [SetUp] 
    public void SetUp() { 
        if (Environment.GetEnvironmentVariable("TEST_LOCATION") is "github") {
            Assert.Inconclusive("Cannot test Essence Editor in Github");
        }
    }

    [TearDown] 
    public void TearDown() {
        if (Directory.Exists("data\\scar\\winconditions")) {
            Directory.Delete("data\\scar\\winconditions");
        }
    }

    [Test]
    public void CanCompile() {
        Assert.Inconclusive();
       /* Directory.CreateDirectory("data\\scar\\winconditions");

        File.WriteAllText("data\\scar\\winconditions\\test.scar", """
            print("hello world");
            """);

        File.WriteAllText("archive.lua", """
            definition = {
                nice_name = "bg_test_archive",
                toc_data = {
                    alias = "data",
                    toc_name = "data",
                    file1 = {
                        file_name = "test.scar",
                        relative_path = "data\scar\winconditions\test.scar",
                        verification = "None",
                        storage = "StreamCompress",
                        encryption = "None"
                    }
                }
            }
            """);

        EssenceEditor editor = new EssenceEditor();
        editor.Archive("archive.lua", "", "test.sga");
       */
    }

}
