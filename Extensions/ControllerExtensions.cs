using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

namespace SistemaBiblioteca.Extensions
{
    public static class ControllerExtensions
    {
        public static void AddModelErrors(this Controller controller, Exception ex)
        {
            if (ex is DbUpdateException dbUpdateEx)
            {
                controller.ModelState.AddModelError("", "Erro de banco de dados. " +
                    "Tente novamente, e se o problema persistir, consulte o administrador.");
            }
            else
            {
                controller.ModelState.AddModelError("", "Ocorreu um erro inesperado. " +
                    "Tente novamente, e se o problema persistir, consulte o administrador.");
            }
        }
    }
}