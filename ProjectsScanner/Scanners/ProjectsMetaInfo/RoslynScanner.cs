using Contracts.Entities;
using Contracts.Interfaces;

namespace ProjectsScanner.Scanners;

public class Source { public string SourceName { get; set; }}

 class Github : Source { public int stars { get; set; }}

 class Bitbucket : Source { public double nestars { get; set; }}
public class RoslynScanner<T> where T : Source
{
    public T source { get; }

    public RoslynScanner(T source)
    {
        var scaner = new RoslynScanner<Github>(new Github());
        var scaner1 = new RoslynScanner<Bitbucket>(new Bitbucket());
        var a = scaner1.source;
        var b = scaner.source;
        //var z = new DotNetProjectScanner();
        //z.getLoggerFramework();
    }
    
    
    
    public WebFrameworks getProjectWebFramework()
    {
        throw new NotImplementedException();
    }

    public string getProjectLanguage()
    {
        return "C#";
    }

    public bool isLoggingSupported()
    {
        throw new NotImplementedException();
    }

    public string getLoggerFramework()
    {
        throw new NotImplementedException();
    }
}