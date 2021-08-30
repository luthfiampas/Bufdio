using System.Threading.Tasks;

namespace BufdioAvalonia.Services
{
    public interface IFileDialogService
    {
        Task<string> OpenAsync();
    }
}
