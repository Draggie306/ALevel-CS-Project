import tkinter as tk
import os
import getpass
import sys
from os import mkdir, path, system, environ
from subprocess import Popen
from datetime import datetime
from time import sleep, time
from turtle import update
from uuid import uuid4
from tqdm import tqdm
from typing import Optional
import traceback
import logging
import zipfile
from threading import Thread
import base64
from math import ceil
from cryptography.fernet import Fernet
import json
import hashlib
import shutil
import requests
from tkinter import messagebox, scrolledtext, filedialog, simpledialog

print("Modules loaded.")

global build, client  # we need to define these as global so we can use them in functions.

build = 1                   # Build is used for update checks. if server build is higher than local build, update is available.
version = "0.0.4(a)"        # Version is used for logging and update checks, easier to read than build and is the same as the unity version.
build_date = 1696262626     # Build date is unix time of when the file was compiled.

dev_mode = False

username = getpass.getuser()
current_exe_path = sys.executable

# Pretty print
green_colour = "\033[92m"
red_colour = "\033[91m"
yellow_colour = "\033[93m"
blue_colour = "\033[94m"
orange_colour = "\033[33m"
cyan_colour = "\033[96m"
magenta_colour = "\033[95m"
reset_colour = "\033[0m"
lily_colour = "\033[95m"
clear_line = "\033[K"
up_one_line = "\033[F"
start_of_line = "\033[0G"
clear_from_line_start = "\033[1K"
clear_above_line_overwrite = "\033[F\033[K"


system(f"title TrimmedDraggieTools v{version} (build {build}) initialised")

environ_dir = environ['USERPROFILE']
start_time = time()


if not dev_mode:
    DraggieTools_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\Draggie\\TrimmedDraggieTools")
    Draggie_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\Draggie")
    #   Fixes issues on first-time entry.
    if not path.exists(Draggie_AppData_Directory):
        mkdir(f"{environ_dir}\\AppData\\Roaming\\Draggie\\")
    if not path.exists(DraggieTools_AppData_Directory):
        mkdir(DraggieTools_AppData_Directory)
else:
    uuid_gen = uuid4()
    # uuid_gen = "test1234"
    DraggieTools_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\Draggie{uuid_gen}\\TrimmedDraggieTools")
    Draggie_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\TrimmedDraggie{uuid_gen}")

    if not path.exists(Draggie_AppData_Directory):
        mkdir(f"{environ_dir}\\AppData\\Roaming\\Draggie{uuid_gen}\\")
    if not path.exists(DraggieTools_AppData_Directory):
        mkdir(DraggieTools_AppData_Directory)

if not path.exists(f"{DraggieTools_AppData_Directory}\\Logs"):
    mkdir(f"{DraggieTools_AppData_Directory}\\Logs")
    print(f"[MainInit] Made DraggieTools_AppData_Directory Logs: {DraggieTools_AppData_Directory}\\Logs", 2)

if not path.exists(DraggieTools_AppData_Directory):
    mkdir(DraggieTools_AppData_Directory)
    print(f"[MainInit] Made DraggieTools_AppData_Directory: {DraggieTools_AppData_Directory}", 2)

if not path.exists(f"{DraggieTools_AppData_Directory}\\UpdatedBuildsCache"):
    mkdir(f"{DraggieTools_AppData_Directory}\\UpdatedBuildsCache")
    print(f"[MainInit] Made UpdatedBuildsCache Directory: {DraggieTools_AppData_Directory}\\UpdatedBuildsCache", 2)

if not path.exists(f"{DraggieTools_AppData_Directory}\\SourceCode"):
    mkdir(f"{DraggieTools_AppData_Directory}\\SourceCode")
    print(f"[MainInit] Made SourceCode Directory: {DraggieTools_AppData_Directory}\\SourceCode", 2)


def tqdm_download(download_url, save_dir, desc: Optional[str] = None, overwrite: Optional[bool] = False, return_exceptions: Optional[bool] = False):
    # Networking component codename is dash
    def download_file(download_url, save_dir):
        response = dash_get(download_url, stream=True)
        total_size = int(response.headers.get("content-length", 0))
        block_size = 1024  # 1 Kibibyte
        written = 0
        desc = download_url.split("/")[-1]
        log(f"Attempting to download a file. Content length: {total_size} bytes. ({download_url})", 1, False, component="dash")
        print(blue_colour)
        with open(save_dir, "wb") as f:
            for data in tqdm(response.iter_content(block_size), total=ceil(total_size // block_size), unit="KB", desc=desc):
                written = written + len(data)
                f.write(data)
        print(reset_colour)
        log(f"Downloaded the file! {total_size} bytes. ({download_url})", 1, False, component="dash")

    if return_exceptions:
        download_file(download_url, save_dir)
    else:
        try:
            download_file(download_url, save_dir)
        except KeyboardInterrupt:
            log("Keyboard interrupt: going back to first choice.", 3, False, component="dash")
            return choice1()
        except Exception as e:
            log(f"\n[DownloadError] An error has occurred downloading the file. {download_url}\n{e}\n{traceback.format_exc()}", 4, component="dash")


print(f"{clear_above_line_overwrite}Loading functions...")


"""
Here are the prints for directory determining.
"""

if dev_mode:
    print(f"\n\n*-* Beta Tester Prints *-*\n\nAppData Directory: {DraggieTools_AppData_Directory}")
    sleep(0.05)
    print(f"Executable location (Where the EXE file is saved locally): {sys.executable}")
    sleep(0.05)
    print(f"Absolute Application Path (Wher PyInstaller runs EXE from): {path.dirname(path.abspath(__file__))}\n\nDevmode is ON, therefore enhanced logging is active.\nThe log file is located in the Roaming AppData directory")
    sleep(0.05)

logging.basicConfig(filename=f'{DraggieTools_AppData_Directory}\\Logs\\[{username}]_{version}-{build}-{time()}.log', encoding='utf-8', level=logging.DEBUG)
logging.debug(f'Established uplink at {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}')

directory = sys.executable
if dev_mode:
    logging.info(f"Assigned directory to {sys.executable}")


def log(text, log_level: Optional[int] = 2, output: Optional[bool] = True, event: Optional[str] = None, component: Optional[str] = None) -> None:
    """
    Logs and prints the text inputted. The logging level is 1: DEBUG, 2: INFO (Default), 3: WARNING, 4: ERROR, 5: CRITICAL\n
    :param text: The text to log\n
    :param log_level: The logging level\n
    :param output: Whether to output the text to the console\n
    :param event: The event to log (Success, Warning, Error, None). Gives the text a colour, overwrites log_level implied colour\n
    :param component: The component of the program that is logging the text. Codenames are used for this\n
    """

    if component is not None:
        if component.lower() == "dash":
            text = f"{lily_colour}[dashNetworking]{reset_colour} {text}"
        elif component.lower() == "main":
            text = f"[Main] {text}"
        elif component.lower() == "updater":
            text = f"[Updater] {text}"

    match log_level:
        case 1:
            logging.debug(text)
        case 2:
            logging.info(text)
        case 3:
            logging.warning(text)
        case 4:
            logging.error(text)
        case 5:
            logging.critical(text)
        case _:
            logging.info(text)

    if event is not None:
        if event == "Success":
            colour = green_colour
        elif event == "Warning":
            colour = yellow_colour
        elif event == "Error":
            colour = red_colour
        else:
            colour = blue_colour
        text = f"{colour}{text}{reset_colour}"

    if output:
        if log_level is None:
            log_level = 2
        elif log_level <= 2:
            print(f"{text}{reset_colour}")
        elif log_level == 3:
            print(f"{yellow_colour}{text}{reset_colour}")
        else:
            print(f"{red_colour}{text}{reset_colour}")
        update_status(text)

        if log_level >= 4:
            messagebox.showerror("Error", text)
    else:
        if log_level == 1:
            # print(f"{text}{cyan_colour} [debug] {reset_colour}")
            pass


def dash_get(*args, **kwargs):
    """
    Drop in replacement for requests.get that uses the logging system.
    """
    # Networking component codename is dash
    log(f"Getting data from: ({args[0]})", 1, False, component="dash")
    x = requests.get(*args, **kwargs)
    log(f"GET request returned with status code {x.status_code}. ({args[0]})", 1, False, component="dash")
    return x


def dash_post(*args, **kwargs):
    """
    Drop in replacement for requests.post that uses the logging system.
    """
    # Networking component codename is dash
    log(f"POSTing data to: ({args[0]})", 1, False, component="dash")
    x = requests.post(*args, **kwargs)
    log(f"POSTing data returned with status code {x.status_code}. ({args[0]})", 1, False, component="dash")
    return x


def ask_for_directory():
    """
    This opens a file dialog box to select a folder and returns the path 
    """
    root = tk.Tk()
    root.withdraw()  # hidesmain window
    directory = filedialog.askdirectory()  # show "ask directory" dialog
    root.destroy()
    return directory

login = None  # use global variable to store login data across functions - diffulcult to pass it around tkinter


def projectsaturnian():
    saturnian_appdir = f"{Draggie_AppData_Directory}\\Saturnian"

    cached_token = None
    username = getpass.getuser()
    username = f"{username.lower()}.3d060a9b-f248-4e2b-babd-e6d5d2c2ab8b"
    # generate a key from the username
    hash_key = hashlib.sha256(username.encode()).digest()
    # create a Fernet key from the hash
    fernet_key = Fernet(base64.urlsafe_b64encode(hash_key))
    log(f"fernet_key: {fernet_key}", output=False)

    if not os.path.isfile(f"{saturnian_appdir}\\Saturnian_data.json"):
        log("[saturnian] No data file found. Creating one now...", log_level=3)
        os.makedirs(saturnian_appdir, exist_ok=True)
        with open(f"{saturnian_appdir}\\Saturnian_data.json", "w") as f:
            first_info = {"current_version": None, "tier": None, "install_dir": saturnian_appdir}
            json.dump(first_info, f)
            log(f"{green_colour}[saturnian] Gamedata file created successfully.")

    def encrypt_token(token):
        log("[encrypt_token] encrypting token")
        update_status("Encrypting token...")
        encrypted_binary_token = fernet_key.encrypt(token.encode())
        log(f"[encrypt_token] encrypted_binary_token: {encrypted_binary_token}")
        # convert the binary token to a string
        encrypted_token = encrypted_binary_token.decode()
        log(f"[encrypt_token] encrypted_token: {encrypted_token}")
        return encrypted_token

    def decrypt_token(encrypted_token):
        log("[decrypt_token] decrypting token")
        token = encrypted_token
        token = bytes(encrypted_token, encoding="utf-8") # this took a while to figure out
        print(f"token: {token}")
        return fernet_key.decrypt(token)

    def read_tokenfile_contents():
        """
        Gets the encrypted token, if it exists.
        """
        log("[read_tokenfile_contents] Reading token file contents...", output=False, log_level=1)
        if os.path.isfile(f"{saturnian_appdir}\\token.bin"):
            with open(f"{saturnian_appdir}\\token.bin", "r") as f:
                cached_token = f.read()
                log(f"[read_tokenfile_contents] Found cached token: {cached_token}", output=False)
                return cached_token
        log("[read_tokenfile_contents] No cached token found.", output=False)
        return None

    def write_token(encrypted_token):
        """
        Writes the unencrypted token to a file. YOU MUST ENCRYPT THE TOKEN BEFORE PASSING IT TO THIS FUNCTION.
        """
        log("[write_token] writing encrypted token")
        if not os.path.isdir(saturnian_appdir):
            os.makedirs(saturnian_appdir, exist_ok=True)
            log(f"[write_token] Created directory: {saturnian_appdir}")
        with open(f"{saturnian_appdir}\\token.bin", "wb") as f:
            f.write(encrypted_token.encode())
            log(f"[write_token] Wrote encrypted token to file: {saturnian_appdir}\\token.bin")

    def read_datafile_attribute(attribute):
        """
        Reads the datafile and returns the value of the attribute.
        """
        try:
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
                saturnian_data = json.load(f)
                log(f"[saturnian/datafile] Read datafile: {saturnian_data}")
            return saturnian_data[attribute]
        except Exception as e:
            if attribute == "install_dir":
                log(f"[saturnian/datafile] Error reading datafile for attribute {attribute}, returning default value: {saturnian_appdir}", log_level=3)
                return saturnian_appdir
            return log(f"[saturnian/datafile] Error reading Saturnian data file: {e}", log_level=4)

    def write_datafile_attribute(attribute, value):
        """
        Writes the value of the attribute to the datafile.
        """
        try:
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
                saturnian_data = json.load(f)
                log(f"[saturnian/datafile] Read datafile: {saturnian_data}", log_level=1)
            saturnian_data[attribute] = value
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "w") as f:
                json.dump(saturnian_data, f, indent=4)
                log(f"[saturnian/datafile] Wrote attribute {attribute} to datafile: {value}")
        except Exception as e:
            log(f"[saturnian/datafile] Error writing attribute {attribute} to datafile: {e}", log_level=4)

    def submit_login():
        global login
        email = email_entry.get()
        password = password_entry.get()
        log(f"{blue_colour}Logging in...")
        login = dash_post("https://client.draggie.games/login", json={"email": email, "password": password, "from": "SaturnianUpdater/DraggieTools"})
        print(f"received login content: {login.content}")
        login_window.destroy()  # close the window after successful login

    cached_token = read_tokenfile_contents()

    if not cached_token:
        log(f"\n\n[ProjectSaturnian] {red_colour}You must log in to download builds from the gameserver, and to validate your license.{reset_colour}", log_level=3)
        log(f"[ProjectSaturnian] {magenta_colour}If you do not have an account, you can create one at:{cyan_colour} https://alpha.draggiegames.com/register{reset_colour}", log_level=3)
        log(f"\n\n{yellow_colour}Please enter your Draggie Games login credentials below.{reset_colour}", log_level=3)

        login_window = tk.Tk()
        login_window.title("Login")

        tk.Label(login_window, text="Draggie Games email: ").grid(row=0)
        email_entry = tk.Entry(login_window)
        email_entry.grid(row=0, column=1)

        tk.Label(login_window, text="Password: ").grid(row=1)
        password_entry = tk.Entry(login_window, show="*")
        password_entry.grid(row=1, column=1)

        tk.Button(login_window, text="Submit", command=submit_login).grid(row=2, column=1, sticky=tk.W)

        login_window.mainloop()

        if login and login.status_code == 200:
            log(f"\n\n{green_colour}Login successful.\n")
            server_token = login.json()["auth_token"]
            log(f"Server returned token: {server_token}", log_level=1)
            newly_encrypted_token = encrypt_token(server_token)
            log(f"newly_encrypted_token: {newly_encrypted_token}", log_level=1)
            write_token(newly_encrypted_token)
            log("Token written to file.", log_level=1)
            update_status("Successfully logged in to Draggie Games!")
            preferred_install_location = messagebox.askquestion("Install Location", "Would you like to install the Saturnian project to the default location?", icon='warning')
            if preferred_install_location == "yes":
                write_datafile_attribute("install_dir", saturnian_appdir)
            else:
                custom_install_location = ask_for_directory()
                if not os.path.isdir(custom_install_location):
                    os.makedirs(custom_install_location, exist_ok=True)
                    log(f"Created directory: {custom_install_location}")
                write_datafile_attribute("install_dir", custom_install_location)

        else:
            log("\n\nLogin failed! Please try again.", log_level=4)
            projectsaturnian()

    try:
        cached_token = read_tokenfile_contents()
        if not cached_token:
            log("No cached token found. Please try again.", log_level=4)
            projectsaturnian()
        encrypted_token = cached_token
        log(f"encrypted_token: {encrypted_token}", output=True)
        decrypted_token = decrypt_token(encrypted_token)
        log(f"token: {decrypted_token}", output=True)
        token = decrypted_token.decode()
        log(f"Final read token: {decrypted_token}", output=True)
    except Exception as e:
        log(f"[saturnian/errors] Error decrypting token: {e}", log_level=4)
        save_login_error = messagebox.askquestion("Saving your login", "Would you like to install the required files to the default location?", icon='warning')
        if save_login_error == "yes":
            if not os.path.isdir(saturnian_appdir):
                os.makedirs(saturnian_appdir, exist_ok=True)
                log(f"[write_token] Created directory: {saturnian_appdir}")
            if os.path.isfile(f"{saturnian_appdir}\\token.bin"):
                os.remove(f"{saturnian_appdir}\\token.bin")
                log(f"[write_token] Deleted file: {saturnian_appdir}\\token.bin")
            else:
                log(f"[write_token] File not found: {saturnian_appdir}\\token.bin", log_level=3)
            projectsaturnian()
        else:
            selected_directory = ask_for_directory()
            if not os.path.isdir(selected_directory):
                os.makedirs(selected_directory, exist_ok=True)
                log(f"[write_token] Created directory: {selected_directory}")
            if os.path.isfile(f"{selected_directory}\\token.bin"):
                os.remove(f"{selected_directory}\\token.bin")
                log(f"[write_token] Deleted file: {selected_directory}\\token.bin")
            else:
                log(f"[write_token] File not found: {selected_directory}\\token.bin", log_level=3)
                log("Skipping token deletion...", log_level=3)

    # After validating the token, we can use it to log in.

    log(f"\n\n{green_colour}Token decryption successful!")
    log("Logging in to your account...")

    def token_login(token):
        endpoint = "https://client.draggie.games/token_login"
        login = dash_post(endpoint, json={"token": token, "from": "SaturnianUpdater/DraggieTools"})
        if login.status_code == 200:
            response = json.loads(login.content)
            log(f"{green_colour}Token login successful. Received response: {response}", output=False)
            return token
        else:
            log(f"{red_colour}Token login failed.")
            choice1()
        # log(f"Received token login content: {login.content}")

    new_token = token_login(token)
    known_token = new_token
    # log(f"new_token: {known_token}")
    log(f"{green_colour}Logged in successfuly!")

    def get_saturnian_info(known_token):
        log("Getting Saturnian info...")
        endpoint = "https://client.draggie.games/api/v1/saturnian/game/gameData/licenses/validation"
        x = dash_get(endpoint, json={"token": known_token, "from": "SaturnianUpdater/TrimmedDraggieTools"})
        if x.status_code == 200:
            try:
                response = json.loads(x.content)
                log(f"[saturnian/OnlineAccount] Your tier: {green_colour}{response['type']}")
                log(f"[saturnian/OnlineAccount] Saturnian current version: {green_colour}v{response['currentVersion']}")
                return response
            except Exception as e:
                log(f"[saturnian/errors.account] Exception: {e}", log_level=4)
                log(f"[saturnian/errors] Received version status code: {x.status_code}", log_level=4)
        else:
            log(f"[saturnian/errors.account] ERROR: Received Saturnian version status code: {x.status_code}", log_level=4)
            error_message = json.loads(x.content)
            log(f"[saturnian/errors.account] ERROR: {error_message['message']}", log_level=4)
            sleep(4)

    server_json_response = get_saturnian_info(known_token)
    saturnian_current_version = server_json_response["currentVersion"]

    # Read the json file
    try:
        with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
            saturnian_data = json.load(f)
            log(f"{green_colour}[saturnian] Successfully read datafile.")
            log(f"Datafile contents: {saturnian_data}", output=False, log_level=1)
    except Exception as e:
        return log(f"[saturnian/errors] Error reading Saturnian data file: {e}", log_level=4)

    def promote_project_lily():
        """
        Prompts the user to install the AutoUpdate project codename Lily.
        """
        log("\nMake sure to check out the Discord server and assign roles for updates: https://discord.gg/GfetCXH")
        client = input(f"Note: {orange_colour}Saturnian{reset_colour} is still in development, so there may be bugs.\n\n{orange_colour}NOTICE: To auto update the project, make sure you have {magenta_colour}AutoUpdate{orange_colour} installed.{reset_colour}\nWould you like to open the {magenta_colour}AutoUpdate{reset_colour} menu now? Y/N\n\n>>> ")
        match client.lower():
            case "y":
                draggieclient()
            case _:
                log("Okay, returning to main menu...")
                projectsaturnian()

    # Now, if the server version is different from the local version, we need to update Saturnian.

    def download_saturnian_build():
        download_url = server_json_response["downloadUrl"]
        log("[saturnian/buildDL] Grabbing authenticated build...")
        log(f"[saturnian/buildDL] Grabbing authenticated build from {download_url}", output=False)

        preferred_install_location = read_datafile_attribute("install_dir")
        tqdm_download(download_url, f"{preferred_install_location}\\Saturnian.bin", overwrite=True)

        log(f"{green_colour}[saturnian/buildDL] Download complete. Decompressing...")
        with zipfile.ZipFile(f"{preferred_install_location}\\Saturnian.bin", "r") as zip_ref:
            zip_ref.extractall(f"{preferred_install_location}\\SaturnianGame")
        sys.stdout.flush()
        sys.stdout.write("\r") # TODO: make it clear the line above
        sys.stdout.flush()
        log(f"\n{green_colour}[saturnian/buildDL] Extraction complete.")
        write_datafile_attribute("current_version", saturnian_current_version)

    if saturnian_current_version != read_datafile_attribute("current_version"):
        log(f"{yellow_colour}[saturnian/Updater] Local game version is different from server version! Input 1 to download and install the new version.")

        choice = messagebox.askquestion("Update available", f"Good news! There is a new version of Saturnian available (version {saturnian_current_version}). This update will be downloaded and installed automatically.\n\nWould you like to update now? (You are currently on version {read_datafile_attribute('current_version')}", icon='info')
        match choice:
            case "yes":
                log(f"[saturnian/Updater] Downloading build version {saturnian_current_version}...")
                download_saturnian_build()
                log(f"{green_colour}[saturnian/Updater] Update download was successful.")

                write_datafile_attribute("current_version", saturnian_current_version)
                write_datafile_attribute("tier", server_json_response["type"])
                promote_project_lily()
            case _:
                log("Okay, returning to main menu...")
                projectsaturnian()

    preferred_install_location = read_datafile_attribute("install_dir")
    if not os.path.isfile(f"{preferred_install_location}\\SaturnianGame\\Saturnian.exe"):
        log("[saturnian/Updater] There is no build in the SaturnianGame folder. This might be because you deleted it, or the download failed. Input 1 to download the build, or 0 to return to the main menu.")
        choice = input("\n\n>>> ")
        match choice:
            case _:
                try:
                    download_saturnian_build()
                    log(f"\n{green_colour}[saturnian/Updater] Update completed!")
                except Exception as e:
                    return log(f"[saturnian/errors] Error downloading Saturnian build: {e}", log_level=4, event="error")

    to_open = log(f"\n\n{cyan_colour}Manage your installation of the project!{reset_colour}\n\n[0] Back to main menu\n[1] Open the game\n[2] Uninstall the project\n[3] Open the game folder\n[4] Change installation directory\n\n>>> ")

    # https://stackoverflow.com/questions/42581016/how-do-i-display-a-dialog-that-asks-the-user-multi-choice-question-using-tkinter

    options = ["Back to main menu", "Open the game", "Uninstall the project", "Open the game folder", "Change installation directory"]
    prompt = "\n".join(f"[{i}] {option}" for i, option in enumerate(options))
    to_open = simpledialog.askstring("Manage your installation", prompt)

    # to_open = messagebox.askquestion("Manage your installation", "What would you like to do?", icon='info')
    match to_open.lower():
        case "1":
            return projectsaturnian()
        case "open the game":
            preferred_install_location = read_datafile_attribute("install_dir")
            Popen(f"{preferred_install_location}\\SaturnianGame\\Saturnian.exe")
            sleep(4)
        case "2":
            log("[saturnian/Updater] Uninstalling Saturnian...")
            try:
                # Remove directory tree
                # shutil.rmtree(f"{preferred_install_location}\\SaturnianGame")
                for file in os.listdir(f"{preferred_install_location}\\SaturnianGame"):
                    try:
                        os.remove(f"{preferred_install_location}\\SaturnianGame\\{file}")
                        log(f"{green_colour}[saturnian/Updater] Removed {file}", log_level=2, output=True)
                    except Exception as e:
                        log(f"[saturnian/errors] Error removing {file}: {e}", log_level=3)
                try:
                    os.rmdir(f"{preferred_install_location}\\SaturnianGame")
                    os.remove(f"{preferred_install_location}\\Saturnian.bin")
                    log(f"{green_colour}[saturnian/Updater] Removed SaturnianGame folder", log_level=2, output=True)
                except Exception as e:
                    return log(f"[saturnian/errors] Error removing SaturnianGame folder: {e}", log_level=4, event="error")
                log(f"{green_colour}[saturnian/Updater] Saturnian uninstalled successfully.")
            except Exception as e:
                return log(f"[saturnian/errors] Error uninstalling Saturnian: {e}", log_level=4, event="error")
        case "open the game folder":
            preferred_install_location = read_datafile_attribute("install_dir")
            Popen(f'explorer /select,"{preferred_install_location}\\SaturnianGame\\Saturnian.exe"')
        case "change installation directory":
            log("[saturnian/Updater] Changing installation directory...")
            start_time = time()
            try:
                new_saturnian_install_dir = ask_for_directory()

                if not os.path.isdir(new_saturnian_install_dir):
                    os.makedirs(new_saturnian_install_dir, exist_ok=True)
                # Cut/move the files over
                try:
                    for file in os.listdir(f"{preferred_install_location}\\SaturnianGame"):
                        shutil.move(f"{preferred_install_location}\\SaturnianGame\\{file}", f"{new_saturnian_install_dir}\\SaturnianGame\\{file}")
                        log(f"{green_colour}[saturnian/Updater] Moved {file} to {new_saturnian_install_dir}\\SaturnianGame\\{file}", log_level=2, output=True)
                except Exception as e:
                    log(f"[saturnian/errors] Error moving files, there may be no files to move: {e}", log_level=3)
                # Write the new data file with the new directory
                write_datafile_attribute("install_dir", new_saturnian_install_dir)
                end_time = time()
                log(f"{green_colour}[saturnian/Updater] Installation directory changed successfully. Took {end_time - start_time} seconds.")
                sleep(2)
            except Exception as e:
                return log(f"[saturnian/errors] Error changing installation directory: {e}\n{traceback.format_exc()}", log_level=4, event="error")
        case _:
            log(f"{red_colour}[saturnian/Updater] Invalid option. Please try again.")
            sleep(1)
            projectsaturnian()
    projectsaturnian()


def update_status(status):
    status_label.config(text=status)
    root.update()


def draggieclient():
    """
    Project Lily: aka DraggieClient
    """
    # Project Lily: aka DraggieClient

    # display tkinter status bar

    known_safe_url = "https://lily.draggie.games/lily_trimmed.exe"
    # In Trimmed/GUI mode, we don't need to ask the user for input.
    update_status("Checking prerequisites for AutoUpdater...")
    try:
        # This sets the target path to the user's AppData directory for it to be installed to.
        target_path = os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client\\lily.exe")
        lily_ensure_appdata_dir = (f"{environ_dir}\\AppData\\Local\\Draggie\\Trimmed_Client")
        lily_ensure_appdata_dir_via_expanded = (f"{os.path.expanduser('~')}\\AppData\\Local\\Draggie\\Trimmed_Client")
    except Exception as e:
        return log(f"[ProjectLily] There was a critical error with trying to determine access to an essential directory: {e}: {traceback.format_exc()}.\n\n", log_level=4)

    if not os.path.exists(lily_ensure_appdata_dir):
        try:
            log("[ProjectLily] Prerequisite directory does not exist. Creating it now...")
            os.makedirs(lily_ensure_appdata_dir, exist_ok=True)
            log(f"{green_colour}[ProjectLily] Prerequisite directory made: {lily_ensure_appdata_dir}")
        except Exception as e:
            log(f"[ProjectLily] There was a critical error with trying to create a directory: {e}: {traceback.format_exc()}. Trying again with a different method...\n\n", log_level=3)
            os.makedirs(lily_ensure_appdata_dir_via_expanded, exist_ok=True)
            log(f"{green_colour}[ProjectLily] Prerequisite directory made: {lily_ensure_appdata_dir_via_expanded}")

    if not os.path.exists(f"{lily_ensure_appdata_dir}\\Logs"):
        os.makedirs(f"{lily_ensure_appdata_dir}\\Logs", exist_ok=True)
        log(f"{green_colour}[ProjectLily] Prerequisite directory made: {lily_ensure_appdata_dir}\\Logs")

    update_status("Downloading AutoUpdater...")
    log(f"Téléchargement de contenu depuis \"{known_safe_url}\" vers {target_path}...", log_level=1)
    try:
        tqdm_download(known_safe_url, target_path, overwrite=True)
        os.startfile(target_path)
        log(f"{green_colour}Your system now has DraggieClient installed! Running in the background, it will automatically keep all of your files by me up to date! Enjoy.")
        update_status("AutoUpdater installed successfully!")
    except PermissionError as e:
        log(f"[ProjectLily] {e}\nThe client is likely running! We don't need to install it again.", log_level=3)
        update_status("Looks like the client is already running!")
    except Exception as e:
        log(f"[ProjectLily] An error occured: {e}: {traceback.format_exc()}", log_level=4)
        update_status(f"An error occurred - {e}")
    sleep(1)


def run_projectsaturnian():
    try:
        projectsaturnian()
        messagebox.showinfo("Success", "The function ran successfully.")
    except Exception as e:
        messagebox.showerror("Error", f"An error occurred: {str(e)}")


def start_thread(event):
    global thread
    thread = Thread(target=run_projectsaturnian)
    thread.daemon = True
    thread.start()
    root.after(20, check_thread)


def check_thread():
    if thread.is_alive():
        root.after(20, check_thread)


root = tk.Tk()
root.title("Project Saturnian Launcher")
status_label = tk.Label(root, text="")
status_label.pack()

frame = tk.Frame(root)
frame.pack()

button = tk.Button(frame, text="Run Project Saturnian", command=run_projectsaturnian)
button.bind('<Button-1>', start_thread)
button.pack(side=tk.LEFT)

text_area = scrolledtext.ScrolledText(root, wrap = tk.WORD, width = 40, height = 10, font = ("Times New Roman",15))
text_area.pack()

root.mainloop()
