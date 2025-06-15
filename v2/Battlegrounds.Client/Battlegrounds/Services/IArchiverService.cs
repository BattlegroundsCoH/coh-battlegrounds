namespace Battlegrounds.Services;

public interface IArchiverService {

    Task<bool> CreateModArchiveAsync(string manifestFile);

}
