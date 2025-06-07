File Content Indexing Project
Overview
A distributed system with two agents (ScannerA, ScannerB) indexing words in .txt files and a Master process aggregating data via Named Pipes. The project uses multithreading, CPU core affinity, and inter-process communication.
Commit History

May 25, 2025: Started project development, initialized repository.
June 2, 2025:
Reorganized project structure, separated ScannerA, ScannerB, and added empty Master.
Implemented first working ScannerA code (without CPU affinity).
Added ScannerB functionality.
Introduced multithreading with Task.Run.
Added SendDataToMaster template.
Created a test folder with .txt files.
Removed outdated project structure.


June 3, 2025:
Implemented working Master to receive and process data.
Added test directories for project validation.
Updated scanners to support multiple usage (path re-entry).


June 5, 2025: Consolidated changes:
Added data aggregation in Master using ConcurrentDictionary.
Renamed files for clarity.
Added error message for directories without .txt files.



How to Run

Open scanner_project.sln in Visual Studio.
Build the solution.
Run Master.exe, then ScannerA.exe and ScannerB.exe.
Enter a directory path containing .txt files.
Press ESC in Master to view the aggregated word index.

Requirements

.NET 9.0
Windows (for ProcessorAffinity)

