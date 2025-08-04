using System.Collections.Generic;

namespace RuntimeConsole;

public interface IObjectMemberProvider
{
    IEnumerable<IMemberEditor> Populate(object obj);
}