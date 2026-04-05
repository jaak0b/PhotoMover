
# PhotoMover

I built PhotoMover to make importing and organizing photos boringly reliable. Point it at an SD card (or a folder), tell it where to put the files and how to group them, and it copies photos into a predictable folder structure based on the metadata in each file.

Why I use it:
- Stops my photos from ending up in a single messy folder.
- Lets me find images by camera and date without hunting through thousands of files.
- Keeps the import process fast and repeatable.

Key features
- Detects and imports from SD / microSD cards.
- Extracts EXIF data (date taken, camera model, lens, file name/extension) and uses it to build destination paths.
- Flexible grouping rules with placeholders (for example: `{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}`).
- Embedded FTP server for automated transfers when needed.

Screenshots
<img width="998" height="655" alt="image" src="https://github.com/user-attachments/assets/9806cd23-ef53-478c-919d-a75faa01a79b" />
<img width="1003" height="653" alt="image" src="https://github.com/user-attachments/assets/b80159ec-8d26-4073-b99d-f995d16dd74c" />
<img width="1000" height="659" alt="image" src="https://github.com/user-attachments/assets/59f35d1b-9729-4654-96c0-b2a6266dbbc1" />

Quick start
- Requirements: .NET 10 SDK, Windows (WPF UI).
- Build: open the solution in Visual Studio or run `dotnet build`.
- Run: start `PhotoMover` from Visual Studio or run the built executable from `bin`.
- Import: open `SD Card Import`, click `Detect Cards`, pick a destination folder and import.
- Rules: open `Grouping Rules` to create patterns and preview how files will be organized.

Configuration notes
- Destination folders are selected per-import in the UI.
- Only one rule can be active at a time; use separate rules for different camera models or workflows.
- The FTP server can be started from the `FTP Server` tab for automated delivery.

Contributing
- Fixes and improvements are welcome. Keep changes focused and include tests for core behavior where appropriate.

License
- MIT — see `LICENSE`.
