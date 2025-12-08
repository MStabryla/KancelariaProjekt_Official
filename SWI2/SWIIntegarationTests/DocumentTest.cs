using Microsoft.AspNetCore.Mvc.Testing;
using SWI2.Models;
using SWI2.Models.Company;
using SWI2.Models.Documents;
using SWI2DB.Models.Account;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SWI2.Models.Response;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("SWIIntegarationTests.AlphabeticalOrderer", "SWIIntegarationTests")]
    public class DocumentTest : BasicTests
    {
        public DocumentTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        [Fact]
        public async Task GetDocuments()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/documentpanel/document";

            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<DocumentViewModel>>();
            Assert.True(responseContent.totalCount > 0);
            Assert.True(responseContent.elements.Count > 0);
        }

        [Fact]
        public async Task GetDocumentTypes()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/documentpanel/document/types";

            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<DocumentTypeViewModel[]>();
            Assert.True(responseContent.Length > 0);
        }

        [Fact]
        public async Task InsertDocument()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/documentpanel/document/types";
            var testFile = "testFile.txt";

            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<DocumentTypeViewModel[]>();
            var documentType = responseContent.First();

            await ActivateToken(client);
            url = "/api/companypanel";
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent2.elements.First();

            await ActivateToken(client);
            url = "/api/documentpanel/document";
            var model = new DocumentViewModel()
            {
                DocumentTypeId = documentType.Id,
                CompanyId = company.Id,
                Notes = "Test note",
                Comment = "Sended",
                OutDocument = true,
                IsProtocol = false,
            };
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            content.Add(new StringContent(JsonConvert.SerializeObject(model)),"modelJson");
            response = await client.PostAsync(url, content);
            fileStream.Close();
            await CheckOk(response);
            var document = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DocumentViewModel>>();

            //usuwanie
            var admin = await GetAuthorizedHttpClient("admin");
            url = "/api/documentpanel/document/" + document.Data.Id;
            response = await admin.DeleteAsync(url);
            await CheckOk(response);
        }
        [Fact]
        public async Task InsertDocumentType()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/documentpanel/document/types";
            var model = new DocumentTypeViewModel()
            {
                Name="Test DocType"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var document = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DocumentTypeViewModel>>();

            //usuwanie
            var admin = await GetAuthorizedHttpClient("admin");
            url = "/api/documentpanel/document/types/" + document.Data.Id;
            response = await admin.DeleteAsync(url);
            await CheckOk(response);
        }
    }
}
