
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
<img width="999" height="650" alt="image" src="https://github.com/user-attachments/assets/f58e2350-de02-413b-b7e0-657c943ad9e2" />
<img width="994" height="651" alt="image" src="https://github.com/user-attachments/assets/8cf4db86-c70c-48d1-8c0c-56d8d7f4a369" />
<img width="996" height="653" alt="image" src="https://github.com/user-attachments/assets/802baa93-08ea-4cb9-8732-d7610d7834ef" />
<img width="1001" height="656" alt="image" src="https://github.com/user-attachments/assets/fa914ac6-c006-4a9a-900c-3c2ff456e6e0" />

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
