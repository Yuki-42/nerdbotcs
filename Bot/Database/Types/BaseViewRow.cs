namespace Bot.Database.Types;

public class BaseViewRow(string connectionString, HandlersGroup handlersGroup)
    : TypeBase(connectionString, handlersGroup);