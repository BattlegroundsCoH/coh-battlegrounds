namespace Battlegrounds.Core.Services;

public interface IFileSystemService {

    Stream OpenWriteTempFile(string tempFileName);

    bool MoveFile(string sourceFile, string destinationFile, bool overwrite = true);

}
