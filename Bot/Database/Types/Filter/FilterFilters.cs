using System.Data;
using System.Data.Common;

namespace Bot.Database.Types.Filter;

public class FilterFilters(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseType(connectionString, handlersGroup, reader)
{
}