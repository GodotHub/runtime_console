using System;

namespace RuntimeConsole;

public interface IExpendObjectRequester
{
    event Action<object, string> RequestCreateNewPanel;
}