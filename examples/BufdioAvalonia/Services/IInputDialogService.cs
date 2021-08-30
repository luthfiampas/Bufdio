using System.Threading.Tasks;

namespace BufdioAvalonia.Services
{
    public interface IInputDialogService
    {
        Task<string> OpenAsync(string title = "", string description = "");
    }
}
