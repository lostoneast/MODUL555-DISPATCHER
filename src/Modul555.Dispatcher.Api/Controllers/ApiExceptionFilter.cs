using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DispatcherApp.Controllers;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, message) = context.Exception switch
        {
            DispatcherApp.Application.Common.ConflictException ex => (409, ex.Message),
            PostgresException { SqlState: "40001" or "40P01" or "23503" } => (409, "Связанные данные изменились. Обновите страницу и повторите операцию."),
            KeyNotFoundException => (404, "Запись не найдена."),
            ArgumentException ex => (400, ex.Message),
            DbUpdateConcurrencyException => (409, "Запись была изменена. Обновите данные и повторите операцию."),
            DbUpdateException { InnerException: PostgresException { SqlState: "40001" or "40P01" } }
                => (409, "Связанные данные изменились одновременно с сохранением. Обновите карточку и повторите операцию."),
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }
                => (409, "Запись с такими уникальными значениями уже существует."),
            DbUpdateException { InnerException: PostgresException { SqlState: "23503" } }
                => (409, "Операция нарушает связи между сущностями."),
            _ => (0, ""),
        };
        if (status == 0) return;
        context.Result = new ObjectResult(new { message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
