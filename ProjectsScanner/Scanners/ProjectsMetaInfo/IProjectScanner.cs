using Contracts.Interfaces;

namespace ProjectsScanner.Scanners;

public interface IProjectScanner
{
    WebFrameworks getProjectWebFramework();

    //support languages as enum
    string getProjectLanguage();
}