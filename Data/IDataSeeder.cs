using System.Threading.Tasks;

namespace SistemaBiblioteca.Data
{
    public interface IDataSeeder
    {
        Task InitializeAsync();
    }
}