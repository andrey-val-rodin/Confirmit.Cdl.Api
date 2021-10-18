using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Services;
using Confirmit.Cdl.Api.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools
{
    public static class LinksExtensions
    {
        public static void FillLinks(this RootDto root, ICustomer customer, IUrlHelper url)
        {
            var self = url.RelativeLink("GetRootInfo", null);
            if (customer == null)
                root.Links = new JObject
                {
                    {"self", self},
                    {"spec", self.TrimEnd('/') + "/swagger"}
                };
            else if (customer.IsInRole(Role.Enduser))
                root.Links = new JObject
                {
                    {"self", self},
                    {"publishedRevisions", url.RelativeLink("GetPublishedRevisions", null)},
                    {"spec", self.TrimEnd('/') + "/swagger"}
                };
            else
                root.Links = new JObject
                {
                    {"self", self},
                    {"documents", url.RelativeLink("GetDocuments", null)},
                    {"publishedRevisions", url.RelativeLink("GetPublishedRevisions", null)},
                    {"spec", self.TrimEnd('/') + "/swagger"}
                };
        }

        public static async Task FillLinksAsync(this DocumentDto document, BaseService service, IUrlHelper url)
        {
            var links = new JObject();
            var id = document.Id;
            if (await service.HasAliasesAsync(id))
            {
                var routeValues = new Dictionary<string, object>
                {
                    {"$filter", $"DocumentId eq {document.Id}"}
                };
                links.Add("aliases", url.RelativeLink("GetAliases", routeValues));
            }

            if (await service.HasPermissionAsync(id, Permission.Manage, ResourceStatus.Exists))
            {
                if (await service.HasCommitsAsync(id))
                    links.Add("commits", url.RelativeLink("GetCommits",
                        new
                        {
                            id = document.Id
                        }));
                if (await service.HasRevisionsAsync(id))
                    links.Add("revisions", url.RelativeLink("GetDocumentRevisions",
                        new
                        {
                            id = document.Id
                        }));
            }

            if (document.PublishedRevisionId != null)
                links.Add("publishedRevision", url.RelativeLink("GetPublishedRevision",
                    new
                    {
                        id = document.Id
                    }));

            if (links.HasValues)
                document.Links = links;
        }

        public static void FillLinks(this CommitDto commit, IUrlHelper url)
        {
            var links = new JObject();
            if (commit.RevisionId != null)
                links.Add("revision", url.RelativeLink("GetRevisionById",
                    new
                    {
                        revisionId = commit.RevisionId
                    }));

            if (links.HasValues)
                commit.Links = links;
        }

        public static void FillLinks(this RevisionDto revision, IUrlHelper url)
        {
            var links = new JObject
            {
                {
                    "document", url.RelativeLink("GetDocumentById", new { id = revision.DocumentId })
                }
            };

            revision.Links = links;
        }

        public static void FillLinks(this AliasDto alias, IUrlHelper url)
        {
            var links = new JObject
            {
                {
                    "document", url.RelativeLink("GetDocumentById", new { id = alias.DocumentId })
                }
            };

            alias.Links = links;
        }

        public static string RelativeLink(this IUrlHelper url, string routeName, object values)
        {
            var link = new Uri(url.Link(routeName, values));
            return link.PathAndQuery;
        }
    }
}