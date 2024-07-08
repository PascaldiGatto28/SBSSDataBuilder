// Ignore Spelling: Linq

namespace SBSSData.Application.LinqPadQuerySupport
{
    public interface IHtmlCreator
    {
        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null);
    }
}
