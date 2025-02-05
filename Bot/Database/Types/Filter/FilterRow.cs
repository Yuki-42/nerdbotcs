using System.Data;

namespace Bot.Database.Types.Filter;

public class FilterRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
	: BaseRow(connectionString, handlersGroup, reader)
{
}