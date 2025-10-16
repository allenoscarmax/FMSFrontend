namespace FMSFrontend.ViewModels
{
    // 廣播「哪個頁面」要刷新資料
    public sealed class RefreshPageMessage
    {
        public string PageKey { get; }
        public RefreshPageMessage(string pageKey) => PageKey = pageKey;
    }
}