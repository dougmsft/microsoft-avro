// Copyright (c) Microsoft Corporation
// All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.
namespace Microsoft.Hadoop.Avro.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Microsoft.Hadoop.Avro.Tools;
    using Microsoft.Hadoop.Avro.Tools.Properties;
    using Xunit;

    [Trait("Category","Tool")]
    public sealed class AvroToolsTests
    {
        public AvroToolsTests()
        {
            MockExecutionContext.Initialize();
        }

        [Fact]
        public void AvroTools_RunWithNoArguments()
        {
            using (var stringReader = new StringReader(string.Empty))
            {
                Console.SetIn(stringReader);

                var actualExitCode = Execute(new string[] { });
                Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            }
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(Resources.InvalidArgsErrorMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunWithWrongArgument()
        {
            var actualExitCode = Execute(new[] { Utilities.GetRandom<string>(false) });
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Equal(Resources.InvalidArgsErrorMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunWithCaseInsensitiveCorrectArgument()
        {
            var actualExitCode = Execute(new[] { "hElP" });
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.Success, actualExitCode);
            Assert.Null(MockExecutionContext.ErrorMessage);
        }

        #region Help Command tests

        [Fact]
        public void AvroTools_RunHelpWithNoArgument()
        {
            var actualExitCode = Execute(new[] { "Help" });
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.Success, actualExitCode);
            Assert.Null(MockExecutionContext.ErrorMessage);
        }

        [Fact]
        public void AvroTools_RunHelpWithWrongArgument()
        {
            var actualExitCode = Execute(new[] { "Help", Utilities.GetRandom<string>(false) });
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Equal(Resources.InvalidArgsErrorMessage, MockExecutionContext.ErrorMessage);
        }

        [Fact]
        public void AvroTools_RunHelpWithEmptyArgument()
        {
            var actualExitCode = Execute(new[] { "Help", HelpCommand.CommandPrefix });
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Equal(Resources.InvalidArgsErrorMessage, MockExecutionContext.ErrorMessage);
        }

        [Fact]
        public void AvroTools_RunHelpWithCorrectArgument()
        {
            var actualExitCode = Execute(new[] { "Help", HelpCommand.CommandPrefix + "Help" });
            var expectedMessage = new HelpCommand().GetUsage();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Null(MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.Success, actualExitCode);
        }

        [Fact]
        public void AvroTools_RunHelpWithTooManyArguments()
        {
            var actualExitCode = Execute(new[] { "Help" }.Concat(Utilities.GetRandom<string[]>(false)).ToArray());
            var expectedMessage = new HelpCommand().GetCommandsList();
            Assert.Equal(expectedMessage, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Equal(Resources.InvalidArgsErrorMessage, MockExecutionContext.ErrorMessage);
        }

        #endregion //Help command tests

        #region CodeGen command tests

        [Fact]
        public void AvroTools_RunCodeGenWithNoArgument()
        {
            var actualExitCode = Execute(new[] { "CodeGen" });
            var expectedMessage = Resources.MissingArgumentsError;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithWrongArgumentsCount()
        {
            var actualExitCode = Execute(new[] { "CodeGen", Utilities.GetRandom<string>(false) });
            var expectedMessage = Resources.MissingArgumentsError;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithTooManyArguments()
        {
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", Utilities.GetRandom<string>(false), Utilities.GetRandom<string>(false), Utilities.GetRandom<string>(false),
                        Utilities.GetRandom<string>(false)
                    });
            var expectedMessage = Resources.ErrorTooManyArguments;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithMissingInputArgumentPrefix()
        {
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "Bla", "/O:."
                    });
            var expectedMessage = Resources.ErrorMissingInputArguments;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithMissingOutputArgumentPrefix()
        {
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:", "Bla"
                    });
            var expectedMessage = Resources.ErrorMissingOutputArguments;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithExtraWrongArgumentNames()
        {
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:", "/O:", "Bla"
                    });
            var expectedMessage = Resources.ErrorSomeArgumentsInvalid;
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithArgumentMissingValue()
        {
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:", "/O:."
                    });
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, Resources.ErrorArgumentMissingItsValue, "/I:");
            Assert.Equal(expectedMessage, MockExecutionContext.ErrorMessage);
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithWrongInputFile()
        {
            var exceptionMessage = Utilities.GetRandom<string>(false);
            MockExecutionContext.OnReadException = new FileNotFoundException(exceptionMessage);
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:Bla", "/O:."
                    });
            Assert.Equal("Bla", MockExecutionContext.FileToRead);
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, Resources.ErrorReadFile, "Bla", exceptionMessage);
            Assert.True(MockExecutionContext.ErrorMessage.Contains(expectedMessage));
            Assert.Equal(ExitCode.InvalidOperation, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithEmptyNamespace()
        {
            MockExecutionContext.FileToReadContent = Utilities.GetRandom<string>(false);
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:.", "/N:"
                    });
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, Resources.ErrorArgumentMissingItsValue, "/N:");
            Assert.True(MockExecutionContext.ErrorMessage.Contains(expectedMessage));
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithEmptyOutputDirectory()
        {
            MockExecutionContext.FileToReadContent = Utilities.GetRandom<string>(false);
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:"
                    });
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, Resources.ErrorArgumentMissingItsValue, "/O:");
            Assert.True(MockExecutionContext.ErrorMessage.Contains(expectedMessage));
            Assert.Equal(ExitCode.InvalidArguments, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithWrongOutputDirectory()
        {
            var exceptionMessage = Utilities.GetRandom<string>(false);
            MockExecutionContext.OnSetDirectoryException = new IOException(exceptionMessage);
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:."
                    });
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, Resources.ErrorWriteDirectory, ".", exceptionMessage);
            Assert.True(MockExecutionContext.ErrorMessage.Contains(expectedMessage));
            Assert.Equal(ExitCode.InvalidOperation, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithWrongSchema()
        {
            MockExecutionContext.FileToReadContent = Utilities.GetRandom<string>(false);
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:."
                    });
            var expectedMessage = Resources.GenerationError.Substring(0, Resources.GenerationError.IndexOf('{'));
            Assert.True(MockExecutionContext.ErrorMessage.Contains(expectedMessage));
            Assert.Equal(ExitCode.InvalidOperation, actualExitCode);
            Assert.Null(MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithWrongSchemaType()
        {
            MockExecutionContext.FileToReadContent = "[\"null\", \"int\"]";
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:."
                    });
            Assert.Equal(Resources.NoSchemataForGeneration, MockExecutionContext.OutMessage);
            Assert.Equal(ExitCode.Success, actualExitCode);
            Assert.Null(MockExecutionContext.ErrorMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithSchemaAndNamespace()
        {
            var expectedFilePath = ".\\E.cs";
            MockExecutionContext.FileToReadContent = "{\"type\": \"enum\", \"name\": \"E\", \"namespace\":\"N\", \"symbols\" : [\"A\", \"B\"]}";
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGen", "/I:bla", "/O:.", "/N:Bla"
                    });
            Assert.Equal(expectedFilePath, MockExecutionContext.FileToWrite);
            Assert.Equal(ExitCode.Success, actualExitCode);
            Assert.Null(MockExecutionContext.ErrorMessage);
            var expectedOutMessage = string.Format(Resources.GenerationInfoMessage, expectedFilePath) + Resources.GenerationFinishedMessage;
            Assert.Equal(expectedOutMessage, MockExecutionContext.OutMessage);
        }

        [Fact]
        public void AvroTools_RunCodeGenWithSchemaAndNamespaceCaseInsensitive()
        {
            var expectedFilePath = ".\\E.cs";
            MockExecutionContext.FileToReadContent = "{\"type\": \"enum\", \"name\": \"E\", \"namespace\":\"N\", \"symbols\" : [\"A\", \"B\"]}";
            var actualExitCode =
                Execute(
                    new[]
                    {
                        "CodeGEN", "/i:bla", "/o:.", "/n:Bla"
                    });
            Assert.Equal(expectedFilePath, MockExecutionContext.FileToWrite);
            Assert.Equal(ExitCode.Success, actualExitCode);
            Assert.Null(MockExecutionContext.ErrorMessage);
            var expectedOutMessage = string.Format(Resources.GenerationInfoMessage, expectedFilePath) + Resources.GenerationFinishedMessage;
            Assert.Equal(expectedOutMessage, MockExecutionContext.OutMessage);
        }

        #endregion //CodeGen command tests

        private static ExitCode Execute(string[] args)
        {
            return Program.Run(args, new MockExecutionContext());
        }
    }
} 