using Battlegrounds.Core.Services;

namespace Battlegrounds.App.Services;

public sealed class FileSystemService : IFileSystemService {

    public bool MoveFile(string sourceFile, string destinationFile, bool overwrite = true) {
        throw new NotImplementedException();
    }

    public Stream OpenWriteTempFile(string tempFileName) {
        throw new NotImplementedException();
    }

}
