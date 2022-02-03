# DtronixPdf [![NuGet](https://img.shields.io/nuget/v/DtronixPdf.svg?maxAge=60)](https://www.nuget.org/packages/DtronixPdf) [![Action Workflow](https://github.com/Dtronix/DtronixPdf/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Dtronix/DtronixPdf/actions)

DtronixPdf is a .NET 5.0 library to handle interactions with PDFs via the PDFium library which is inherently not thread safe.  This library will serialize all calls which are made to the PDFium backend and execute them all on a single thread via a dispatcher.  Results are then returned through Tasks to the calling site.

Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

[Project Roadmap](https://github.com/orgs/Dtronix/projects/1)

### Usage

- [Nuget Package](https://www.nuget.org/packages/DtronixPdf).
- Manual building. `dotnet build -c Release`

### Build Requirements
- .NET 5.0

### References

- https://github.com/Dtronix/PDFiumCore
- https://pdfium.googlesource.com/pdfium/
- https://github.com/bblanchon/pdfium-binaries
- https://github.com/mono/CppSharp

### License
[MIT](LICENSE) License