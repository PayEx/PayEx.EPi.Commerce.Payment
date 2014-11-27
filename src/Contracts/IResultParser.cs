
namespace Epinova.PayExProvider.Contracts
{
    public interface IResultParser
    {
        T Deserialize<T>(string xml) where T : class;
    }
}
