using AutoMapper;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Validators;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Services
{
    public class AliasService : BaseService
    {
        public AliasService(CdlDbContext dbContext, IHttpContextAccessor httpContext,
            IMapper mapper, Factory factory, IAccountLoader accountLoader,
            HubPermissionReader hubPermissionReader, SurveyPermissionReader surveyPermissionReader)
            : base(dbContext, httpContext.HttpContext.User, mapper,
                factory, accountLoader, hubPermissionReader, surveyPermissionReader)
        {
        }

        public async Task<AliasToCreateDto> ValidateAliasAsync(AliasToCreateDto alias)
        {
            alias = AliasValidator.Validate(alias);
            var result = new AliasToCreateDto
            {
                Namespace = alias.Namespace,
                Alias = alias.Alias,
                DocumentId = alias.DocumentId
            };

            if (await FindAliasAsync(result.Namespace, result.Alias) != null)
                throw new BadRequestException("Unable to set new alias. Try different namespace or alias name.");

            return result;
        }

        public async Task<AliasDto> CreateAliasAsync(AliasToCreateDto alias)
        {
            var documentAlias = Mapper.Map<AliasToCreateDto, DocumentAlias>(alias);
            DbContext.Aliases.Add(documentAlias);
            await DbContext.SaveChangesAsync();

            return AliasToDto(documentAlias);
        }

        public async Task<bool> DeleteAliasAsync(long aliasId)
        {
            var toDelete = await GetAliasByIdAsync(aliasId);
            if (toDelete == null)
                return false;

            DbContext.Aliases.Remove(toDelete);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<AliasDto> UpdateAliasLinkAsync(DocumentAlias alias, long newDocumentId)
        {
            alias.DocumentId = newDocumentId;
            await DbContext.SaveChangesAsync();

            return AliasToDto(alias);
        }

        public AliasDto AliasToDto(DocumentAlias alias)
        {
            if (alias == null)
                throw new ArgumentNullException(nameof(alias));

            return Mapper.Map<DocumentAlias, AliasDto>(alias);
        }

        public async Task<IQueryable<AliasDto>> GetAliasesAsync()
        {
            return
                from document in await GetInitialQueryForAvailableDocumentsAsync()
                from alias in DbContext.Aliases
                where alias.DocumentId == document.Resource.Id
                select new AliasDto
                {
                    Id = alias.Id,
                    Namespace = alias.Namespace,
                    Alias = alias.Alias,
                    DocumentId = document.Resource.Id
                };
        }
    }
}