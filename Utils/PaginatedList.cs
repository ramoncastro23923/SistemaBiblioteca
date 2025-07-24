using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaBiblioteca.Utils
{
    /// <summary>
    /// Classe para implementação de paginação em listas
    /// </summary>
    /// <typeparam name="T">Tipo dos itens da lista</typeparam>
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalItems { get; private set; }
        public int PageSize { get; private set; }

        /// <summary>
        /// Construtor da lista paginada
        /// </summary>
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalItems = count;
            PageSize = pageSize;

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public bool IsFirstPage => PageIndex == 1;
        public bool IsLastPage => PageIndex == TotalPages;

        /// <summary>
        /// Cria uma lista paginada assincronamente a partir de uma fonte IQueryable
        /// </summary>
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 1)
                throw new ArgumentException("Page index must be greater than 0", nameof(pageIndex));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var count = await source.CountAsync();
            var items = await source.Skip(
                (pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Cria uma lista paginada a partir de uma lista existente
        /// </summary>
        public static PaginatedList<T> CreateFromList(
            List<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count;
            var items = source.Skip(
                (pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}