using Contracts.Entities;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq.Protected;
using ProjectsLoader.Services;
using ProjectsScanner.Scanners;
using Storages.EntitiesStorage;

namespace ProjectScanner.Tests;

public class UnitTest1
{
    /*[Fact]
    public void Test1()
    {
        var mock = new Mock<HttpClient>();
        var controller = new WebPagesScanner(mock.Object);

        string jsonContent = "{\n    \"stargazers_count\": 1234,\n    \"created_at\": \"2020-05-15T12:30:00Z\",\n    \"default_branch\": \"main\"\n}";
        string jsonReadme = "A sample project using ASPNET Core for building web applications.";
        string urlPostfix = "IlyaGrigorievWeb/ProjectsLoader";
        var result = controller.GetInfoProject(jsonContent, jsonReadme, urlPostfix);
        
        var metaData = new GitHubProject
        {
            UrlPostfix = "IlyaGrigorievWeb/ProjectsLoader",
            WebFramework =  WebFrameworks.Aspnet,
            Stars = 1234,
            CreationDate = new DateTime(2020, 05, 15, 12, 30, 00, DateTimeKind.Utc),
            DefaultBranch = "main",
        };

        Assert.Equal(metaData.UrlPostfix, result.UrlPostfix);
        Assert.Equal(metaData.WebFramework, result.WebFramework);
        Assert.Equal(metaData.Stars, result.Stars);
        Assert.Equal(metaData.CreationDate, result.CreationDate);
        Assert.Equal(metaData.DefaultBranch, result.DefaultBranch);
    }*/

    [Fact]
    public async void Test2()
    {
        var repoResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{ \"stargazers_count\": 1234, \"created_at\": \"2020-05-15T12:30:00Z\", \"default_branch\": \"main\", \"owner\": {\"login\": \"IlyaGrigorievWeb\"}, \"name\": \"ProjectsLoader\" }")
        };
        var readmeResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{ \"web_framework\": \"Aspnet\" }")
        };
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(repoResponse)
            .ReturnsAsync(readmeResponse);
        
        var httpClient = new HttpClient(handlerMock.Object);
        
        var controller = new WebPagesScanner(httpClient);
        
        string url = "https://github.com/IlyaGrigorievWeb/ProjectsLoader";
        var result = await controller.GetGitHubProject(url);
        
        var metaData = new GitHubProject
        {
            UrlPostfix = "IlyaGrigorievWeb/ProjectsLoader",
            WebFramework = WebFrameworks.Aspnet,
            Stars = 1234,
            CreationDate = new DateTime(2020, 05, 15, 12, 30, 00, DateTimeKind.Utc),
            DefaultBranch = "main",
        };
        
        Assert.Equal(metaData.UrlPostfix, result.UrlPostfix);
        Assert.Equal(metaData.WebFramework, result.WebFramework);
        Assert.Equal(metaData.Stars, result.Stars);
        Assert.Equal(metaData.CreationDate, result.CreationDate);
        Assert.Equal(metaData.DefaultBranch, result.DefaultBranch);
    }
    
    // [Fact]
    // public async void Test3()
    // {
    //     var mock = new Mock<HttpClient>();
    //     
    //     var metaData = new GitHubProject()
    //     {
    //         UrlPostfix = "IlyaGrigorievWeb/ProjectsLoader",
    //         WebFramework = WebFrameworks.Aspnet,
    //         Stars = 1234,
    //         CreationDate = new DateTime(2020, 05, 15, 12, 30, 00, DateTimeKind.Utc),
    //         DefaultBranch = "main",
    //     };
    //     
    //     var controller = new Mock<WebPagesScanner>();
    //     controller.CallBase = true;
    //     controller.Setup(x => x.GetGitHubProject(It.IsAny<string>())).ReturnsAsync(metaData);
    //
    //     User user = new User()
    //     {
    //         Id = Guid.NewGuid(),
    //         Login = "IlyaGrigorievWeb",
    //     };
    //     
    //     var result = await controller.GetGitHubProject(user, "ProjectsLoader");
    //     
    //     
    //     Assert.Equal("IlyaGrigorievWeb/ProjectsLoader", result.UrlPostfix);
    // }
    
    [Fact]
    public async void Test4()
    {
        var options = new DbContextOptionsBuilder<PostgresContext>()
            .UseInMemoryDatabase("FakeDb_ForConstructorOnly")
            .Options;

        using var context = new PostgresContext(options);
        
        var metaData = new GitHubProject()
        {
            UrlPostfix = "IlyaGrigorievWeb/ProjectsLoader",
            WebFramework = WebFrameworks.Aspnet,
            Stars = 1234,
            CreationDate = new DateTime(2020, 05, 15, 12, 30, 00, DateTimeKind.Utc),
            DefaultBranch = "main",
        };
        
        var scaner = new Mock<IWebPagesScanner>();
        scaner.CallBase = true;
        scaner.Setup(x => x.GetGitHubProject(It.IsAny<string>())).ReturnsAsync(metaData);

        User user = new User()
        {
            Id = Guid.NewGuid(),
            Login = "IlyaGrigorievWeb",
        };
        
        var service = new GitHubService(context, scaner.Object);
        
        var result = await service.GetGitHubProject("https://github.com/IlyaGrigorievWeb/ProjectsLoader");
        
        Assert.Equal(metaData.UrlPostfix, result.UrlPostfix);
        Assert.Equal(metaData.WebFramework, result.WebFramework);
        Assert.Equal(metaData.Stars, result.Stars);
        Assert.Equal(metaData.CreationDate, result.CreationDate);
        Assert.Equal(metaData.DefaultBranch, result.DefaultBranch);
    }
}

public static class MyLogic
{
    public static int Calculate(int input)
    {
        return input;
    }
}



