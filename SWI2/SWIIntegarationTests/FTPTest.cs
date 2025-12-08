using FluentFTP;
using Microsoft.AspNetCore.Mvc.Testing;
using SWI2.Models;
using SWI2.Models.Company;
using SWI2.Models.FTP;
using SWI2.Services.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnit.Project.Attributes;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("SWIIntegarationTests.AlphabeticalOrderer", "SWIIntegarationTests")]
    public class FTPTest : BasicTests
    {
        public FTPTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        [Fact]
        public async Task GetHomeFiles()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/home";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<FileListModel>();
            Assert.Equal("/users/user_108", responseContent.Path);
        }

        [Fact, TestPriority(0, TestType.Get)]
        public async Task SendFile()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/";
            string homeUrl = "/users/user_108";
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            var response = await client.PostAsync(url,content);
            fileStream.Close();
            await CheckOk(response);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact, TestPriority(1, TestType.Get)]
        public async Task DownloadFile()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/";
            string homeUrl = "/users/user_108";
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            var response = await client.PostAsync(url, content);
            fileStream.Close();
            await CheckOk(response);

            url = "/api/filepanel/download" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.GetAsync(url);
            await CheckOk(response);
            Assert.True(response.Content.Headers.ContentLength > 0);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            await CheckOk(response);

        }

        [Fact, TestPriority(2, TestType.Get)]
        public async Task SendFileToDepartment()
        {
            var client = await GetAuthorizedHttpClient("clientII");

            string url = "api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent2.elements.First();

            url = "api/filepanel/";
            string homeUrl = "/companies/" + company.Id + "/" + department.FolderName;
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            response = await client.PostAsync(url, content);
            fileStream.Close();
            await CheckOk(response);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact, TestPriority(3, TestType.Get)]
        public async Task SendFileToDepartmentFail()
        {
            var client = await GetAuthorizedHttpClient("admin");

            string url = "api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent2.elements.First();

            client = await GetAuthorizedHttpClient("clientII");
            url = "api/filepanel/";
            string homeUrl = "/companies/" + company.Id + "/" + department.FolderName;
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            response = await client.PostAsync(url, content);
            fileStream.Close();
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);

            //clearing
            client = await GetAuthorizedHttpClient("admin");
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact, TestPriority(4, TestType.Get)]
        public async Task DownloadFileFromDepartment()
        {
            var client = await GetAuthorizedHttpClient("clientII");

            string url = "api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent2.elements.First();

            url = "api/filepanel/";
            string homeUrl = "/companies/" + company.Id + "/" + department.FolderName;
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            response = await client.PostAsync(url, content);
            fileStream.Close();
            await CheckOk(response);

            url = "/api/filepanel/download" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.GetAsync(url);
            await CheckOk(response);
            Assert.True(response.Content.Headers.ContentLength > 0);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task CreateDirectory()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/folder";
            string homeUrl = "/users/user_108";
            string folderName = "test";

            url += "?path=" + Encoding64.Base64Encode(homeUrl) + "&folderName=" + folderName;
            var response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<FtpListItem>();
            Assert.Equal(folderName, responseContent.Name);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel/folder" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + folderName);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact, TestPriority(5, TestType.Get)]
        public async Task RenameFile()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/";
            string homeUrl = "/users/user_108";
            string testFile = "testFile.txt";

            url += "?path=" + Encoding64.Base64Encode(homeUrl);
            var content = new MultipartFormDataContent();
            var fileStream = new FileStream(testFile, FileMode.Open);
            content.Add(new StreamContent(fileStream), "file", testFile);
            var response = await client.PostAsync(url, content);
            fileStream.Close();
            await CheckOk(response);

            await ActivateToken(client);
            url = "/api/filepanel/" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile) + "&newName=testFile2.txt";
            testFile = "testFile2.txt";
            response = await client.PutAsync(url, null);
            await CheckOk(response);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + testFile);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }
        [Fact]
        public async Task RenameDirectory()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/filepanel/folder";
            string homeUrl = "/users/user_108";
            string folderName = "test";

            url += "?path=" + Encoding64.Base64Encode(homeUrl) + "&folderName=" + folderName;
            var response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<FtpListItem>();
            Assert.Equal(folderName, responseContent.Name);

            await ActivateToken(client);
            url = "/api/filepanel/" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + folderName) + "&newName=test2";
            folderName = "test2";
            response = await client.PutAsync(url, null);
            await CheckOk(response);

            //clearing
            await ActivateToken(client);
            url = "/api/filepanel/folder" + "?path=" + Encoding64.Base64Encode(homeUrl + "/" + folderName);
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

    }
}
