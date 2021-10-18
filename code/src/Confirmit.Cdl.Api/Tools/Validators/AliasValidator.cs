using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.ViewModel;
using JetBrains.Annotations;
using System.Linq;

namespace Confirmit.Cdl.Api.Tools.Validators
{
    public static class AliasValidator
    {
        public static AliasToCreateDto Validate(AliasToCreateDto alias)
        {
            if (alias == null)
                throw new BadRequestException("Missing alias.");

            alias = new AliasToCreateDto
            {
                Namespace = alias.Namespace?.ToLowerInvariant(),
                Alias = alias.Alias?.ToLowerInvariant(),
                DocumentId = alias.DocumentId
            };

            CheckField(alias.Namespace, "namespace");
            CheckField(alias.Alias, "alias");

            return alias;
        }

        [AssertionMethod]
        private static void CheckField(string field, string fieldName)
        {
            if (string.IsNullOrEmpty(field))
                throw new BadRequestException($"Missing {fieldName}.");
            if (!field.All(IsValidChar))
                throw new BadRequestException($"Field {fieldName} is not well-formed URI part.");
        }

        private static bool IsValidChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '~';
        }
    }
}