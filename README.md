# Confirmit.Cdl.Api
Rest API for CDL documents. Contains CRUD methods for document resources, including support for multiple revision snapshots, as well as support for user and end-user rights.

Confirmit.Cdl.Api is the new reliable and fast .NET Core service that intends to replace the old Confirmit.Cml.Storage.Service.
## Documentation
[https://co-osl-tenta69.firmglobal.com/api/cdl/swagger](https://co-osl-tenta69.firmglobal.com/api/cdl/swagger/index.html)
## Database
Old Confirmit.Cml.Storage.Service and new Confirmit.Cdl.Api share the same database called CmlStorage. Process of updating database is called DB migrations. Confirmit.Cml.Storage.Service performed database migrations during deployment and used table DatabaseUpdateHistory to keep update steps. Now updating database from Cml.Storage.Service is stopped. All further DB migration steps must be added to Cdl.Api.

Cdl.Api performs DB update in startup and uses table VersionInfo. It means that Cdl.Api service need to be deployed and then called to start database update.
## Breaking changes
Basically,  new service is compatible with the old one. However, there are several breaking changes:

 1. The client must use introspection scope 'cdl'.
 2. All endpoints that return collections use OData parameters. No more custom parameters for searching and sorting; use OData parameters instead.
 3. PublicMetadata and Private metadata: they can be any arbitrary string, as in Cml storage service. But Cml service always responds with native string without double quotes, and content type is application/json. Cdl service response depends on Accept header. If Accept header is *text/plain*, then result is native string. If Accept header is *application/json*, then result is string in double quotes. Default is *text/plain*.
 4. The service doesn't parse private metadata and no longer extracts fields from it. Specify Hub, LinkedSurveyId and OriginDocumentId explicitly in payload.
 5. Page: TotalAmount renamed to TotalCount
 6. Hub renamed to HubId
 7. Revisions no longer contains fields Hub, LinkedSurveyId and OriginDocumentId
 8. No more onlyWholeCompanies and onlyWholeEnduserLists parameters; new controllers and endpoints; new organization of endpoints: userpermissions, companypermissions, enduserpermissions, enduserlistpermissions.
 9. 'Company' and 'EnduserList' instead of 'Organization' in response.
 10. No IdentityController. Use IDP instead.
 11. No batch support.
 12. No HTTP OPTIONS support. Let's add this later if we need CORS.

## Responsible
 team: Reporting Moscow
 developer: @andreyr