# Battlegrounds Desktop Launcher V2.0.0

Placeholder text

## Developer Notes

The project attempts to follow best practices in regard to CI/CD.

### Prequisites
The Battlegrounds client project requires the following components to be pre-installed to begin development:
* Protoc compiler
* .NET SDK 10

### Distribution Pipeline

The distribution pipeline makes use of [Velopack](https://velopack.io/) for providing an installer and handling automatic updates.

Developers will need to install the vpk .NET tool:
```bash
dotnet tool install -g vpk
```

To release a new version, make a new release on Github and give it an appropriate version. The tag must be prefixed with `v`.
A Github workflow will handle the rest.
