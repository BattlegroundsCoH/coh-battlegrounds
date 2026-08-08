using Battlegrounds.Models.News;

namespace Battlegrounds.Services;

public interface INewsService {

    Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int count, bool forceRefresh = false, CancellationToken ct = default);
    
    Task<NewsPage> GetPageAsync(int page, int pageSize, CancellationToken ct = default);

}
