<br/>
<p align="center">
  <a href="https://github.com/DavidMacak/ConverterProjectPublic">
    <img src="ConverterProject.Web/wwwroot/images/logo-sm.png" alt="Logo" height="40">
  </a>
  <p align="center">
    <a href="https://davakconverter.azurewebsites.net/">View Demo</a>
  </p>
</p>



## About The Project

Simple web eml/msg to pdf converter.

## Built With

* .NET 8
* ASP.NET MVC
* iText
* MimeKit
* MsgReader

### Installation

1. Get iText license at https://itextpdf.com/
2. Clone the repo
```sh
git clone https://github.com/DavidMacak/ConverterProjectPublic.git
```
3. Rename your itext license key to `itextkey.json` and move it to project root folder.
4. In program.cs you this this for local deployment:
```csharp
//builder.Services.AddTransient<IFileService, BlobService>();
builder.Services.AddTransient<IFileService, LocalFileService>();
```

### Azure Deployment
1. Get iText license and move it into project root folder as `itextkey.json`
2. Create Azure Blob Storage
3. Create 2 containers `tobeconverted` and `converted`
4. Put your Blob Storage connections string in `appsettings.json`
5. In program.cs comment LocalFileService and uncomment BlobService:
```csharp
builder.Services.AddTransient<IFileService, BlobService>();
//builder.Services.AddTransient<IFileService, LocalFileService>();
```
## Roadmap

 - [ ] Multiple conversions at once
 - [ ] Save logs on blob storage.
 - [ ] Use Application Insights.
 - [ ] Hash uploaded files.
 - [ ] Limit daily usage.
 - [ ] Better frontend
 - [ ] Use .NET Queue
 - [ ] Unit tests


## Bugs & imperfections

 - Unhandled exception when uploading files larger than 30MB
 - Logs are stored locally in wwwroot/Logs.
 - iText eats a lot of ram (+-600MB) because of font conversion.
 - Video thumbnails not converted as image.
 - Gif is not converted.
