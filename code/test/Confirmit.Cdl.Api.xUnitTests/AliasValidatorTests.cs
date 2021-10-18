using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Validators;
using Confirmit.Cdl.Api.ViewModel;
using JetBrains.Annotations;
using Xunit;
using Action = System.Action;

namespace Confirmit.Cdl.Api.xUnitTests
{
    public class AliasValidatorTests
    {
        private static readonly string[] BadStrings =
        {
            "\r",
            @"some
text",
            "&",
            "abc%",
            "?a=b",
            ".",
            "!"
        };

        private static readonly string[] GoodStrings =
        {
            "abc1",
            "0",
            "confirmit-themes",
            "_",
            "~",
            "-"
        };

        [Fact]
        public void Validate_AliasIsNull_Exception()
        {
            VerifyExpectedException(
                () => AliasValidator.Validate(null),
                "Missing alias.");
        }

        [Fact]
        public void Validate_NullNamespace_Exception()
        {
            VerifyExpectedException(
                () => AliasValidator.Validate(new AliasToCreateDto { Namespace = null, Alias = "my_doc" }),
                "Missing namespace.");
        }

        [Fact]
        public void Validate_NullAlias_Exception()
        {
            VerifyExpectedException(
                () => AliasValidator.Validate(new AliasToCreateDto { Namespace = "my-namespace", Alias = null }),
                "Missing alias.");
        }

        [Fact]
        public void Validate_BadNamespace_Exception()
        {
            foreach (var badString in BadStrings)
            {
                VerifyExpectedException(
                    () => AliasValidator.Validate(new AliasToCreateDto { Namespace = badString, Alias = "my_doc" }),
                    "Field namespace is not well-formed URI part.");
            }
        }

        [Fact]
        public void Validate_BadAlias_Exception()
        {
            foreach (var badString in BadStrings)
            {
                VerifyExpectedException(
                    () => AliasValidator.Validate(
                        new AliasToCreateDto { Namespace = "my-namespace", Alias = badString }),
                    "Field alias is not well-formed URI part.");
            }
        }

        [Fact]
        public void Validate_GoodNamespaceAndAlias_Success()
        {
            foreach (var goodString in GoodStrings)
            {
                AliasValidator.Validate(new AliasToCreateDto { Namespace = goodString, Alias = goodString });
            }
        }

        #region Helpers

        [AssertionMethod]
        private static void VerifyExpectedException(Action action, string expectedErrorMessage)
        {
            try
            {
                action.Invoke();
            }
            catch (BadRequestException e)
            {
                Assert.True(expectedErrorMessage == e.Message,
                    $"Wrong exception message (expected and actual):\r\n{expectedErrorMessage}\r\n{e.Message}");
                return;
            }

            Assert.False(true, "Test did not throw an exception. ValidationException was expected");
        }

        #endregion
    }
}
