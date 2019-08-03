using FluentAssertions;
using Xunit;

namespace Cake.Codecov.Tests
{
    public sealed class CodecovAliasesTests
    {
        [Fact]
        public void Should_Use_Same_Settings_As_Specified()
        {
            var fixture = new CodecovAliasesFixture();

            var expected = new CodecovSettings
            {
                Branch = "MyBRanch",
                Build = "MyBuild"
            };
            fixture.Settings = expected;

            var result = fixture.Run();

            result.Args.Should().ContainAll(new[]
            {
                $"--branch \"{expected.Branch}\"",
                $"--build \"{expected.Build}\""
            });
        }

        [Fact]
        public void Should_Use_Specified_Files()
        {
            var fixture = new CodecovAliasesFixture
            {
                Settings = null // The settings is unfortunately saved between runs
            };

            var files = new[]
            {
                "File2143.sda",
                "my/Dsa/.csa",
                "sda.ds/Sda"
            };

            fixture.Files = files;

            var result = fixture.Run();

            result.Args.Should().Contain($"--file \"{string.Join(" ", files)}\"");
        }

        [Fact]
        public void Should_Use_Specified_Files_And_Token()
        {
            var fixture = new CodecovAliasesFixture
            {
                Settings = null
            };

            var files = new[]
            {
                "cover-file1.xml",
                "coverfile2.lcov",
                "somethingelse.dsa"
            };

            var token = "My-Really-Awesome-Token";
            fixture.Files = files;
            fixture.Token = token;

            var result = fixture.Run();
            result.Args.Should()
                .ContainAll(new[]
                {
                    $"--file \"{string.Join(" ", files)}\"",
                    $"--token \"{token}\""
                });
        }

        [Fact]
        public void Should_Use_Specified_File()
        {
            var fixture = new CodecovAliasesFixture
            {
                Settings = null
            };

            var file = "./BUildArtifacts/TestResults/OpenCover.xml";
            fixture.File = file;

            var result = fixture.Run();

            result.Args.Should()
                .Be($"--file \"{file}\"");
        }

        [Fact]
        public void Should_Use_Specified_File_And_Token()
        {
            var fixture = new CodecovAliasesFixture
            {
                Settings = null
            };

            var file = "./BUildArtifacts/TestResults/OpenCover.xml";
            var token = "My-Still-Awesome-Token";
            fixture.File = file;
            fixture.Token = token;

            var result = fixture.Run();

            result.Args.Should()
                .ContainAll(new[]
                {
                    $"--file \"{file}\"",
                    $"--token \"{token}\""
                });
        }
    }
}
