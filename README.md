# Website Blocker

A Windows application that allows users to block and unblock websites system-wide by modifying the hosts file. Built with C# and Windows Forms.

## Features
- Modern, full-screen user interface
- Block websites with a simple input
- View list of currently blocked websites
- Easily unblock websites when needed
- Automatically handles both www and non-www versions of domains
- Requires administrator privileges for hosts file modification

## Requirements
- Windows OS
- .NET 8.0 or later
- Administrator privileges (for hosts file modification)

## Installation
1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the application
4. Run as administrator when using the application

## Usage
1. Enter a website domain (e.g., facebook.com)
2. Click "Block Website" to add it to the blocked list
3. To unblock, select a website from the list and click "Unblock Selected"
4. Changes take effect immediately across all browsers (You will need to close the browser first)
