# My A level Computer Science Project

Welcome to my A level Computer Science project! This project contains a lot of different things, including a Unity project, Python scripts, and more.

## 🔗 Linked repositories
The website and assets are available on another repository, [draggie306/draggiegames.com](https://github.com/Draggie306/draggiegames.com)
The server is available on another repository, [draggie306/DraggieGamesServer](https://github.com/Draggie306/DraggieGamesServer)

## 📁 Project Structure

- `Assets/` - Contains most assets for the project, including materials, images, and packages.
    - `Assets/Scenes/` - Contains the scenes for the project and handmade C# scripts.
- `Packages/` - Unity json files for package list
- `python/` - Contains Python scripts including autoupdate and tools for the project.
- `saturnian-distributeBuild.py` - Python script to distribute the build using Cloudflare R2 object storage.

## 📚 Additional Documentation

- `CompSci Project.docx` - Writeup for the project.
- More coming soon!

## 🏗️ Build

### 🧱 Unity project 
Git clone the repository, then download the Unity Hub and install 2022.3.14f1
Open the project in Unity and build it.

### 🐍 Client-side python scripts
- In `/python/autoupdate` and `/python/tools`, run `pip install -r requirements.txt` to install the required packages for each respective script.
- You can then run the scripts using `python3 <scriptname>.py`
- To build the autoupdate script, run `pyinstaller --onefile --noconsole autoupdate.py` in the `/python/autoupdate` directory.
- To build the tools script, run `pyinstaller --onefile tools.py` in `/python/tools`.

### 📤 Distribute build

To distribute the build, run the `saturnian-distributeBuild.py` script in the root directory of the project.
- This assumes there is a `build/` directory in the root directory of the project. It will create a new subfolder with an incremented number and upload the build to that folder after compressing it.

You will need your own Cloudflare R2 object storage account, and you will need your own Cloudflare API key and email and environment variables.
This uses `rclone` to upload the build to the object storage bucket.

## 📝 Note

This project is still in development, and is not yet ready for use.
