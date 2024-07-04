using System.Data;

namespace Bot.Database.Types;

public class BaseViewRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : TypeBase(connectionString, handlersGroup, reader);