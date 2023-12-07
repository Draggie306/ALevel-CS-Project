import asyncio
import base64
import datetime
import getpass
import hashlib
import json
import logging
import os
import shutil
import sys
import time
import traceback
import zipfile
import uuid
import pyglet
import tqdm
import socket
import random
from typing import Optional

import requests
import win32com.client
import winshell
from fernet import Fernet
from win10toast import ToastNotifier
w10_toast = ToastNotifier()
# from win11toast import toast_async as toast, notify, update_progress


build = 1
build_time = 1697475802
version = "beta"
username = getpass.getuser()
environ_dir = os.environ['USERPROFILE']
lily_colour = "\033[95m"
reset_colour = "\033[0m"

update_refresh_duration_sec = 900 # 15 minutes - this is default but can be changed by the server

# To create a requirements.txt file, run this in the terminal:
# pip freeze > requirements.txt


def w10_toast_notification(title, message, duration: Optional[int] = 10, icon_url: Optional[str] = None):
    log_print(f"[toast_notification] Attempting to show a toast notification using Win10 API. Title: {title}, Message: {message}, Duration: {duration}, Icon URL: {icon_url}", True, 2)
    if icon_url is None:
        if not os.path.isfile(f"{Client_AppData_Directory}\\notification-logo.ico"):
            try:
                download_url = "https://ibaguette.com/favicon.ico"
                icon_path = f"{Client_AppData_Directory}\\notification-logo.ico"
                x = dash_get(download_url, stream=True)
                with open(icon_path, "wb") as f:
                    f.write(x.content)
            except Exception as e:
                log_print(f"[toast_notification] An error has occurred downloading the icon. {e}\n{traceback.format_exc()}", True, 4)
                icon_path = None
        else:
            icon_path = f"{Client_AppData_Directory}\\notification-logo.ico"
    else:
        icon_path = None
    try:
        w10_toast.show_toast(
            title,
            message,
            duration=duration,
            icon_path=icon_path,
            threaded=True,
        )
    except TypeError as e:
        log_print(f"[toast_notification] A (probably insignificant) TypeError has occurred: {e}\n{traceback.format_exc()}", True, 3)
    except Exception as e:
        log_print(f"[toast_notification] An error has occurred: {e}\n{traceback.format_exc()}", True, 4)



try:
    shortcut_path = os.path.join(winshell.startup(), "Client.lnk")
    f"{environ_dir}AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup"
    target_directory = os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client")
    target_path = os.path.expanduser("~\\AppData\\Local\\Draggie\\Client\\lily.exe")
    print(f"[InitialInfo] shortcut_path: {shortcut_path}\n[InitialInfo] target_directory: {target_directory}\n[InitialInfo] target_path: {target_path}")
    directory = sys.executable

    Client_AppData_Directory = (f"{environ_dir}\\AppData\\Local\\Draggie\\Trimmed_Client")
    Draggie_AppData_Directory = (f"{environ_dir}\\AppData\\Local\\Draggie")
    update_directory = os.path.join(target_directory, "update")
except Exception as e:
    print(f"[InitialInfo] There was an error with trying to determine access to an essential directory: {e}: {traceback.format_exc()}.\n\n\nRetrying with other directory")
    time.sleep(1)
    try:
        base_dir = os.path.abspath(f"{environ_dir}\\AppData\\Local\\Draggie") # this won't work as the module is not callable
        print("[InitialInfo] Swapping. Note that some functionality may be disabled...")
        shortcut_path = os.path.join(winshell.startup(), "Client.lnk")
        target_directory = os.path.join(base_dir, "Client")
        target_path = os.path.join(base_dir, "Client", "lily.exe")
        print(f"[InitialInfo] shortcut_path: {shortcut_path}\n[InitialInfo] target_directory: {target_directory}\n[InitialInfo] target_path: {target_path}")
        directory = sys.executable

        Client_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\Draggie\\Trimmed_Client")
        Draggie_AppData_Directory = (f"{environ_dir}\\AppData\\Roaming\\Draggie")
        update_directory = os.path.join(target_directory, "update")
    except Exception as e:
        print(f"There was an error with trying to determine access to a secondary directory: {e}: {traceback.format_exc()}.\n\n\nRetrying with final directory")
        base_dir = os.path.dirname(os.path.abspath(__file__))
        print("Swapping. Note that some functionality may be disabled...")
        # shortcut_path = os.path.join(winshell.startup(), "Client.lnk")
        shortcut_path = None
        target_directory = os.path.join(base_dir, "Trimmed_Client")
        target_path = os.path.join(base_dir, "Client", "lily.exe")
        print(f"shortcut_path: not defined\ntarget_directory: {target_directory}\ntarget_path: {target_path}")
        directory = sys.executable

        Client_AppData_Directory = (f"{base_dir}\\Draggie\\Trimmed_Client")
        Draggie_AppData_Directory = (f"{base_dir}\\Draggie")
        update_directory = os.path.join(target_directory, "update")

# -*-*-*-*-* DIRECTORY CREATION *-*-*-*-*-

if not os.path.exists(Client_AppData_Directory):
    os.makedirs(Client_AppData_Directory, exist_ok=True)
    print(f"[InitialDirCreation] makedirs: {Client_AppData_Directory}")

if not os.path.exists(f"{Client_AppData_Directory}\\Logs"):
    os.makedirs(f"{Client_AppData_Directory}\\Logs", exist_ok=True)
    print(f"[InitialDirCreation] mkdir: {Client_AppData_Directory}\\Logs")

if not os.path.exists(f"{Client_AppData_Directory}\\Logs\\modulation"):
    os.makedirs(f"{Client_AppData_Directory}\\Logs\\modulation", exist_ok=True)
    print(f"[InitialDirCreation] mkdir-modulation: {Client_AppData_Directory}Logs\\modulation")

# -*-*-*-*-* LOGGING *-*-*-*-*-

current_log_name = f"[{username}]-{build}.{time.time()}.log"

logging.basicConfig(
    filename=f"{Client_AppData_Directory}\\Logs\\{current_log_name}",
    encoding='utf-8',
    level=logging.DEBUG,
    # f"{datetime.datetime.now().strftime(r'[%d/%m/%Y %H:%M:%S]').ljust(25)}"
    format="%(levelname)-10s | %(message)s",
)
logging.debug(f'[FirstLog] Established uplink at {datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")}')


def log_print(text, printing: Optional[bool] = True, level: Optional[int] = 2, stacklevel: Optional[int] = None, component: Optional[str] = None) -> None:
    """
    `text` (str): the text to log/print\n
    `printing` (optional bool): whether to print the output or not. default: Falsen\n
    `level` (optional int): which of logging's 5 levels to mark the log as. default: 2 (INFO)\n
    `stacklevel` (optional int): which level to log the stacktrace at. default: 1 (the function itself)
    """
    if not stacklevel:
        stacklevel = 1
    if component:
        if component.lower() == "dash":
            print(f"{lily_colour}[dashNetworking]{reset_colour} {text}")
            text = f"[dashNetworking] {text}"
    if printing:
        if not component:
            print(text)

    if level is None:
        level = 2

    # format=f"{datetime.datetime.now().strftime(r'[%d/%m/%Y %H:%M:%S.%f]').ljust(30)} | %(levelname)-10s | %(message)s",

    text = f"{datetime.datetime.now().strftime(r'[%d/%m/%Y %H:%M:%S.%f]').ljust(30)} | {text}"

    match level:
        case 1:
            logging.debug(msg=text, stacklevel=stacklevel)
        case 2:
            logging.info(msg=text, stacklevel=stacklevel)
        case 3:
            logging.warning(msg=text, stacklevel=stacklevel)
        case 4:
            logging.error(msg=text, stacklevel=stacklevel)
        case 5:
            logging.critical(msg=text, stacklevel=stacklevel)
        case _:
            logging.info(msg=text, stacklevel=stacklevel)


log_print(f"[PreStartup] Build: {build}\n[PreStartup] BuildTime: {datetime.datetime.fromtimestamp(build_time).strftime('%Y-%m-%d %H:%M:%S')} ({build_time})")
log_print(f"[PreStartup] sys.executable: {directory}", printing=False)
log_print(f"[PreStartup] ClientDir: {Client_AppData_Directory}", printing=False)
log_print(f"[PreStartup] DDir: {Draggie_AppData_Directory}", printing=False)
log_print(f"Queried the current version: running build: {build}")


def dash_get(*args, **kwargs):
    """
    Drop in replacement for requests.get that uses the logging system.
    """
    # Networking component codename is dash
    log_print(f"Attempting to GET data. ({args[0]})", False, 1, component="dash")
    x = requests.get(*args, **kwargs)
    log_print(f"GET returned status code {x.status_code}. ({args[0]})", False, 1, component="dash")
    return x


def dash_post(*args, **kwargs):
    """
    Drop in replacement for requests.post that uses the logging system.
    """
    # Networking component codename is dash
    log_print(f"Attempting to POST data... ({args[0]})", False, 1, component="dash")
    headers = kwargs.get("headers", {})
    headers["User-Agent"] = f"TrimmedDraggieClient/{build}"
    headers["Draggie-Client-Version"] = f"{build}"
    headers["X-DraggieGames-App"] = "TrimmedAutoUpdateClient"
    headers["Username"] = f"{username}"
    try:
        kwargs["headers"] = headers
        x = requests.post(*args, **kwargs)
        log_print(f"POST returned with status code {x.status_code}. ({args[0]})", False, 1, component="dash")
        return x
    except Exception as e:
        log_print(f"POST failed with exception {e}.", False, 4, component="dash")
        return None


def download(download_url, save_dir, use_tqdm: Optional[bool] = False, self_update: Optional[bool] = False):
    """Downloads the newest build of the program and writes a file to the current directory"""
    try:
        if use_tqdm:
            log_print(f"[mainDownload] Attempting to download a file. ({download_url})", False)
            with requests.get(download_url, stream=True) as r:
                r.raise_for_status()
                with open(save_dir, "wb") as f:
                    pbar = tqdm.tqdm(total=int(r.headers["Content-Length"]), unit="B", unit_scale=True, unit_divisor=1024)
                    for chunk in r.iter_content(chunk_size=8192):
                        if chunk:
                            f.write(chunk)
                    pbar.close()
            log_print(f"[mainDownload] Downloaded the file! ({download_url})", False)
        else:
            response = dash_get(download_url, stream=True)

            total_size = int(response.headers.get("content-length", 0))

            log_print(f"[mainDownload] Attempting to download a file. Content length: {total_size} bytes. ({download_url})", False)
            with open(save_dir, "wb") as f:
                f.write(response.content)

            log_print(f"[mainDownload] Downloaded the file! {total_size} bytes. ({download_url})", False)

        with open(f"{Client_AppData_Directory}\\OldExecutableDir.txt", "w") as file:
            file.write(f"{sys.executable}")
            file.close()

    except Exception as e:
        log_print(f"\n[mainDownload/ERROR] An error has occured downloading the file. {download_url}\n{e}\n{traceback.format_exc()}", False, 4)
        if self_update:
            log_print(f"[mainDownload/ERROR] Self update requested. Downloading the file to client.exe. {download_url}", False, 4)
            log_print(f"[mainDownload/ERROR] Retrying this to client.exe. {download_url}\n{e}\n{traceback.format_exc()}", False, 4)
            response_2 = dash_get(download_url, stream=True)

            total_size = int(response_2.headers.get("content-length", 0))

            target_path = os.path.expanduser("~\\AppData\\Local\\Draggie\\Client\\client.exe")
            log_print(f"[mainDownload/ERROR] target_path: {target_path}", False, 2)
            log_print(f"[mainDownload/ERROR] Attempting to download a file. Content length: {total_size} bytes. ({download_url})", False, 2)
            with open(target_path, "wb") as f:
                f.write(response_2.content)
        else:
            log_print("[mainDownload/ERROR] A self update has not been requested. Ignoring this error, it will be called automatically later.", True, 4)

    # sys.exit()


def draggietools_updater():
    tools_environ_dir = os.environ['USERPROFILE']
    DraggieTools_AppData_Directory = f"{tools_environ_dir}\\AppData\\Roaming\\Draggie\\TrimmedDraggieTools"
    # Draggie_AppData_Directory = f"{tools_environ_dir}\\AppData\\Roaming\\Draggie"
    if not os.path.exists(DraggieTools_AppData_Directory):
        os.makedirs(DraggieTools_AppData_Directory, exist_ok=True)
        log_print(f"[tools/InitDirCreation] mkdir: {DraggieTools_AppData_Directory}")

    if not os.path.exists(f"{DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt"):
        log_print(f"[tools/NoAutoUpdateInfo] Checked for AutoUpdateInfo file at {DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt", True, 2)
        return log_print("[tools/NoAutoUpdateInfo] No AutoUpdateInfo file found. Skipping update for DraggieTools.", True, 3)

    try:
        with open(f"{DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt", "r") as file:
            log_print(f"[tools/JSON] Opened the file {DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt", True, 2)
            try:
                json_data = json.load(file)
                log_print(f"[tools/JSON] Loaded the JSON data from {DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt", True, 2)
            except Exception as e:
                return log_print(f"[tools/JSON/ERROR] An error has occurred loading the JSON data from {DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt\n{e}\n{traceback.format_exc()}", True, 4)

        tools_installation_directory = json_data['tools_installation_directory']
        build = json_data['build']

        server_latest_build = dash_get("https://raw.githubusercontent.com/Draggie306/DraggieTools/main/build.txt").text
        if int(server_latest_build) > int(build):
            log_print("[tools/UpdateAvailable] An update is available for DraggieTools. Updating...", True, 2)
            log_print(f"[tools/UpdateAvailable] Downloading the file to '{tools_installation_directory}'", True, 2)
            download("https://github.com/Draggie306/ALevel-CS-Project/blob/main/python/tools/dist/DraggieTools_Trimmed.exe?raw=true", f"{tools_installation_directory}")
            log_print(f"[tools/UpdateAvailable] Successfully updated DraggieTools to build {server_latest_build}, from build {build}.", True, 2)
            json_data['build'] = server_latest_build
            json_data['updated_by_client'] = True
            with open(f"{DraggieTools_AppData_Directory}\\Client_AutoUpdateInfo.txt", "w") as file:
                json.dump(json_data, file, indent=4)
                log_print(f"[tools/UpdateAvailable] Updated the build number in the AutoUpdateInfo file to {server_latest_build}.", True, 2)
            w10_toast_notification("DraggieTools", f"Your installation of DraggieTools has been automatically updated to build {server_latest_build}.", duration=10)
        else:
            log_print(f"[tools/NoUpdateAvailable] No update is available for DraggieTools (current build: {build}, server build: {server_latest_build}).", True, 2)
    except Exception as e:
        log_print(f"[tools/UpdateError] An error has occured updating DraggieTools. {e}\n{traceback.format_exc()}", True, 3)


def saturnian_updater():
    saturnian_environ_dir = os.environ['USERPROFILE']
    saturnian_appdir = f"{saturnian_environ_dir}\\AppData\\Roaming\\Draggie\\Saturnian"

    if not os.path.isfile(f"{saturnian_appdir}\\token.bin"):
        return log_print("[saturnian/main] No token file found. Skipping update for Saturnian.", True, 3)

    try:
        username = getpass.getuser()
        username = f"{username.lower()}.3d060a9b-f248-4e2b-babd-e6d5d2c2ab8b"
        hash_key = hashlib.sha256(username.encode()).digest()
        fernet_key = Fernet(base64.urlsafe_b64encode(hash_key))
    except Exception as e:
        return log_print(f"[saturnian/cryptography] An error has occurred creating the Fernet key. {e}\n{traceback.format_exc()}", True, 4)

    log_print("[saturnian/main] Basic checks passed, checking for updates for Saturnian...", True, 2)

    def read_tokenfile_contents():
        """
        Gets the encrypted token, if it exists.
        """
        log_print("[saturnian/cryptography] Reading token file contents")
        if os.path.isfile(f"{saturnian_appdir}\\token.bin"):
            with open(f"{saturnian_appdir}\\token.bin", "rb") as f:
                cached_token = f.read()
                log_print(f"[saturnian/cryptography] Found cached token: {cached_token}")
                return cached_token
        return None

    try:
        encrypted_token = read_tokenfile_contents()
        log_print(f"[saturnian/cryptography] Encrypted token: {encrypted_token}")
        decrypted_token = fernet_key.decrypt(encrypted_token)
        log_print(f"[saturnian/cryptography] Decrypted token: {decrypted_token}")
        token = decrypted_token.decode()
        log_print(f"[saturnian/cryptography] Final decoded token value: {token}")
        if uuid.UUID(token, version=4):
            log_print("[saturnian/cryptography] The token seems to be formatted in a valid way.", True, 2)
        else:
            return log_print("[saturnian/cryptography] The token doesn't seem to match the valid format of a version 4 Universally Unique Identifier. Aborting.", True, 3)
    except Exception as e:
        return log_print(f"[saturnian/cryptography] An error has occurred decrypting the token. \n{traceback.format_exc()}\nerr: {e}", True, 4)

    # After validating the token, we can use it to log in.

    log_print(f"[saturnian/accountManager] Validation successful, logging in with token {token}")

    def token_login(token_to_login):
        log_print("[saturnian/accountManager] Logging to token login endpoint...", True, 2)
        endpoint = "https://client.draggie.games/token_login"
        login = dash_post(endpoint, json={"token": token_to_login, "from": "TrimmedAutoUpdateClient/SaturnianUpdater"})
        if login.status_code == 200:
            response = json.loads(login.content)
            username = response['username']
            log_print(f"[saturnian/accountManager] Successfully logged in to server endpoint \"token_login\" as username: {username}", True, 2)
            return token
        else:
            log_print(f"[saturnian/accountManager] Token login failed! {login.status_code} {login.content}", True, 3)
            return None

    new_token = token_login(token)
    if new_token is None:
        return log_print("[saturnian/accountManager] Token login failed, aborting.", True, 3)
    known_token = new_token
    # print(f"new_token: {known_token}")
    log_print("[saturnian/accountManager] Logged in successfuly using a token!", False, 2)

    def get_saturnian_info(known_token):
        log_print("[saturnian/accountInfo] Getting license info from server...")
        endpoint = "https://client.draggie.games/api/v1/saturnian/game/gameData/licenses/validation"
        x = dash_get(endpoint, json={"token": known_token, "from": "TrimmedAutoUpdateClient/SaturnianUpdater"})
        if x.status_code == 200:
            try:
                response = json.loads(x.content)
                return response
            except Exception as e:
                return log_print(f"[saturnian/accountInfo] Error: exception getting json response /api/v1/saturnian/game/gameData/licenses/validation. Saturnian version status code: {x.status_code} {e}", True, 3)
        else:
            return log_print(f"[saturnian/accountInfo] Error: non-200 response /api/v1/saturnian/game/gameData/licenses/validation. Saturnian version status code: {x.status_code}\n\nTRACEBACK\n\n{traceback.format_exc()}", True, 4)

    server_json_response = get_saturnian_info(known_token)
    if server_json_response is None:
        return log_print(f"[saturnian/accountInfo] Error: server_json_response is None. Aborting.\n{traceback.format_exc()}", True, 3) 
    saturnian_current_version = server_json_response["currentVersion"]

    if not os.path.isfile(f"{saturnian_appdir}\\Saturnian_data.json"):
        with open(f"{saturnian_appdir}\\Saturnian_data.json", "w") as f:
            first_info = {"current_version": None, "tier": None}
            json.dump(first_info, f)
            log_print("[saturnian/LocalData] Saturnian data file created.")

    # Read the json file
    try:
        with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
            saturnian_data = json.load(f)
            log_print(f"[saturnian/LocalData] Saturnian data: {saturnian_data}")
    except Exception as e:
        return log_print(f"[saturnian/LocalData] Error reading Saturnian data file: {e}")

    def read_datafile_attribute(attribute):
        """
        [From DraggieTools] Reads the datafile and returns the value of the attribute.
        """
        try:
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
                saturnian_data = json.load(f)
                log_print(f"[saturnian/datafile] Read datafile: {saturnian_data}")
            return saturnian_data[attribute]
        except Exception as e:
            if attribute == "install_dir":
                log_print(f"[saturnian/datafile] Error reading datafile for attribute {attribute}, returning default value: {saturnian_appdir}")
                return saturnian_appdir
            return log_print(f"[saturnian/datafile] Error reading Saturnian data file: {e}")

    def write_datafile_attribute(attribute, value):
        """
        Writes the value of the attribute to the datafile.
        """
        try:
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "r") as f:
                saturnian_data = json.load(f)
                log_print(f"[saturnian/datafile] Read datafile: {saturnian_data}")
            saturnian_data[attribute] = value
            with open(f"{saturnian_appdir}\\Saturnian_data.json", "w") as f:
                json.dump(saturnian_data, f, indent=4)
                log_print(f"[saturnian/datafile] Wrote attribute {attribute} to datafile: {value}")
        except Exception as e:
            log_print(f"[saturnian/datafile] Error writing attribute {attribute} to datafile: {e}")

    # Now, if the server version is different from the local version, we need to update Saturnian.

    def download_saturnian_build():
        download_url = server_json_response["downloadUrl"]
        preferred_download_directory = read_datafile_attribute("install_dir")
        if preferred_download_directory is None:
            preferred_download_directory = saturnian_appdir
        preferred_download_directory = os.path.normpath(preferred_download_directory)
        print(f"[saturnian/updater] Downloading Saturnian build from {download_url}")
        # save_dir = f"{preferred_download_directory}\\Saturnian.bin"
        save_dir = os.path.join(preferred_download_directory, "Saturnian.bin")

        try:
            bytes_downloaded = 0
            if not os.path.exists(preferred_download_directory):
                os.makedirs(preferred_download_directory, exist_ok=True)
                log_print(f"[saturnian/updater] Created directory {preferred_download_directory}", False, 2)
            with dash_get(download_url, allow_redirects=True, stream=True) as download:
                with open(save_dir, "wb") as f:
                    # download 8 MiB at a time
                    for chunk in download.iter_content(chunk_size=20971520): # 20 MiB
                        f.write(chunk)
                        bytes_downloaded += len(chunk)
                        log_print(f"[saturnian/updater] DownloadInfo: Downloaded {bytes_downloaded} bytes ({bytes_downloaded / 1048576} MiB)")
            log_print(f"[saturnian/updater] DownloadInfo: Total bytes downloaded: {bytes_downloaded} ({bytes_downloaded / 1048576} MiB / {bytes_downloaded / 1073741824} GiB)")
            log_print(f"[saturnian/updater] DownloadInfo: Hash of downloaded file: {hashlib.sha256(open(save_dir, 'rb').read()).hexdigest()}")
            log_print(f"[saturnian/updater] DownloadInfo: Downloaded Saturnian build id {saturnian_current_version} to {save_dir}")
        except Exception as e:
            return log_print(f"[saturnian/updater] Error downloading Saturnian build id {saturnian_current_version}: {e}\n\n{traceback.format_exc()}", True, 3)

        try:
            log_print("[saturnian/updater] Download complete. Extracting...")
            with zipfile.ZipFile(f"{preferred_download_directory}\\Saturnian.bin", "r") as zip_ref:
                zip_ref.extractall(f"{preferred_download_directory}\\SaturnianGame")
            log_print("[saturnian/updater] Extraction successful.")
            w10_toast_notification("Draggie Games", f"Your installation of Project Saturnian [{read_datafile_attribute('tier').upper()} TESTING] has been updated to version: {saturnian_current_version}.", duration=10, icon_url="https://cdn.ibaguette.com/saturnian.ico")
        except Exception as e:
            return log_print(f"[download_saturnian_build] Error extracting Saturnian build: {e}\n\n{traceback.format_exc()}", True, 3)

    if saturnian_current_version != read_datafile_attribute("current_version"):
        try:
            log_print(f"[saturnian/updater] Version mismatch! Server version: {saturnian_current_version} Local version: {saturnian_data['current_version']}. Forcing update.", False, 2)
            download_saturnian_build()
            write_datafile_attribute("current_version", saturnian_current_version)
            write_datafile_attribute("tier", server_json_response["type"])
        except Exception as e:
            return log_print(f"[saturnian/updater] Update error: {e}\n\n{traceback.format_exc()}", True, 3)

    def find_exe():
        log_print("[saturnian/updater] Checking for executable files...", False, 2)
        if read_datafile_attribute("install_dir"):
            # Check for any exe files in the SaturnianGame folder
            if os.path.exists(f"{read_datafile_attribute('install_dir')}\\SaturnianGame"):
                for file in os.listdir(f"{read_datafile_attribute('install_dir')}\\SaturnianGame"):
                    if file.endswith(".exe"):
                        return log_print(f"[saturnian/updater] Found executable file {file} in SaturnianGame folder.", False, 2)

        # If no exes, download and extract new build
        download_saturnian_build()

    find_exe()

    log_print("[saturnian] Version check completed!", False, 2)


def upload_logs(file_path, delete: Optional[bool] = False, all_files: Optional[bool] = False, current: Optional[bool] = False) -> None:
    """
    [Network] The upload logfile function.\n
    This is fired on statup from the main function.\n
    ANY *existing* files in the log folder will be uploaded to the Client server on my Repl server\n
    Does not include files in /client/modulation\n
    There may be personally identifiable information in this, your userprofile name, defined on line 21\n
    This telemetry will be configurable later but default to ON
    """

    logic_to_use = 2

    if current:
        for filename in os.listdir(f"{Client_AppData_Directory}\\Logs"):
            if filename == current_log_name:
                log_print(f"[UploadLogs/current] Updating server records for current logfile '{filename}'", True, 2)
                files = None
                file_path = os.path.join(f"{Client_AppData_Directory}\\Logs", filename)
                if os.path.isfile(file_path):
                    with open(file_path, 'rb') as f:
                        files = {'file': (filename, f.read())}
                        f.close()

                if files:
                    response = dash_post("https://logs.draggie.games/client/api/v2", files=files)
                    return log_print(f"[UploadLogs/current] Uploaded current logfile '{filename}' to server: status code {response.status_code}", True, 2)
        return

    if all_files:
        if logic_to_use == 1: # This is an old version of the upload logic. There may be permission errors involved which looks bad in the logfile.
            for filename in os.listdir(f"{Client_AppData_Directory}\\Logs"):
                if not filename == current_log_name:
                    if os.path.isfile(os.path.join(f"{Client_AppData_Directory}\\Logs", filename)):
                        file_path = os.path.join(f"{Client_AppData_Directory}\\Logs", filename)
                        with open(file_path, 'rb') as f:
                            files = {'file': f}
                            log_print(f"[UploadLogs] Uploading '{filename}' to server", True)
                            response = dash_post("https://logs.draggie.games/client/api/v2", files=files)
                            log_print(f"[UploadLogs] Uploaded '{filename}' to server: status code {response.status_code}", True)
                        if delete:
                            try:
                                os.remove(file_path)
                                log_print(f"[UploadLogs] Deleted '{file_path}'")
                            except PermissionError:
                                log_print("[UploadLogs] Unable to delete the log file. It is being used by another process.")
                    else:
                        print(f"[UploadLogs] Item '{filename}' is not a file, skippinng...")

        else: # Use the new logic
            for filename in os.listdir(f"{Client_AppData_Directory}\\Logs"):
                if filename == current_log_name:
                    # log_print(f"[UploadLogs] Dodging current session logfile: {filename}")
                    # continue
                    log_print(f"[UploadLogs] Uploading current session logfile: {filename}")

                file_path = os.path.join(f"{Client_AppData_Directory}\\Logs", filename)
                if os.path.isfile(file_path):
                    with open(file_path, 'rb') as f:
                        files = {'file': (filename, f.read())}
                        f.close()

                    if delete:
                        try:
                            try:
                                log_print(f"[UploadLogs] Uploading '{filename}' to server", True)
                                response = dash_post("https://logs.draggie.games/client/api/v2", files=files)
                                log_print(f"[UploadLogs] Uploaded '{filename}' to server: status code {response.status_code}", True)
                                os.remove(file_path)
                                log_print(f"[UploadLogs] Deleted '{file_path}'")
                            except PermissionError:
                                log_print("[UploadLogs] Unable to delete the log file. It is being used by another process.")
                            except Exception as e:
                                log_print(f"[UploadLogs] An error occurred whilst trying to upload or delete the logfile '{filename}': {e}. Long details: {traceback.format_exc()}", True)
                                try:
                                    response = dash_post("https://logs.draggie.games/client/api/v2", files=files)
                                    os.remove(file_path)
                                    log_print(f"[UploadLogs] Deleted '{file_path}'")
                                except PermissionError:
                                    log_print("[UploadLogs] Unable to delete the log file. It is being used by another process.")
                                except Exception as e:
                                    log_print(f"[UploadLogs] An error occurred whilst trying to upload to the backup server or delete the logfile '{filename}': {e}. Long details: {traceback.format_exc()}", True)
                        except PermissionError:
                            log_print("[UploadLogs] Unable to delete the log file. It is being used by another process.")
                else:
                    log_print(f"[UploadLogs] Item '{filename}' is not a file, skipping...")
    else:
        if os.path.isfile(file_path):
            with open(file_path, 'rb') as f:
                files = {'file': f}
                log_print(f"[UploadLogsNonAll] Uploading file '{file_path}' to server...", False)
                try:
                    response = dash_post("https://logs.draggie.games/client/api/v2", files=files)
                    log_print(f"{response.status_code}", True)
                except Exception as e:
                    return log_print(f"Unable to upload file: {e}\n\n{traceback.format_exc()}")
        if delete:
            try:
                os.remove(file_path)
                log_print(f"[UploadLogsNonAll] Deleted: '{file_path}'")
            except PermissionError:
                log_print("[UploadLogsNonAll] PermissionError")


def startup():
    def statup_copy_ensure():
        if not os.path.isfile(target_path):
            log_print("[Startup] Target filepath 'client.exe' is not a file.", True)
        """Copies the current file to the startup location via a shortcut. This shortcut is unconditional so will always execute to the current exe file"""
        if not shortcut_path:
            return log_print("[Startup] Shortcut path is not defined.", True)
        shell = win32com.client.Dispatch("WScript.Shell")
        shortcut = shell.CreateShortCut(shortcut_path)
        shortcut.Targetpath = target_path
        shortcut.WorkingDirectory = os.path.dirname(target_path)
        shortcut.save()

    statup_copy_ensure()
    asyncio.run(main())


def get_build():
    """[Network] Checks the ClientAssets endpoint for the latest build. This is fired every 300 seconds."""
    log_print("[GetBuild] Getting current build version")
    try:
        current_build_version = int((dash_get('https://lily.draggie.games/latest_build.txt')).text)
        log_print(f"[GetBuild] Server current build int: {current_build_version}")
    except Exception as e:
        current_build_version = build
        log_print(f"[GetBuild] Unable to get current build version from the server, using local build version: {e}", level=4)
    log_print(f"[GetBuild] Server current build: {current_build_version}")
    return current_build_version


def download_new_version():
    """[Network] Downloads the new build from the ClientAssets server."""
    current_build_version = get_build()
    log_print("[downloadNewVersion] Downloading new build")
    if not os.path.exists(update_directory):
        os.makedirs(update_directory, exist_ok=True)
    try:
        log_print(f"[downloadNewVersion/SelfUpdate] Downloading new version of myself from https://lily.draggie.games/lily{current_build_version}.exe. This may take a while...", True, 2)
        download(f"https://lily.draggie.games/lily{current_build_version}.exe", f"{update_directory}\\lily-v{current_build_version}.exe", self_update=True)
    except Exception as e:
        return log_print(f"[downloadNewVersion] There was a big error downloading the new version! Unable to download it. {e}\n\n{traceback.format_exc()}", level=4)


# -*-*-*-*-* MAIN THREAD *-*-*-*-*-


async def main() -> None:
    """
    [Network] [Executor] This coroutine is responsible for the permanent background task as well as initiating web requests\n
    The most important part here is getting the latest build. If the server is unreachable then it won't work.\n
    If there is an update available, it will download the newest version and exit, after having started that downloaded file\n
    If there is NOT an update available, it will check where it's running from and compare it to the old version, in OldExecutableDir.txt\n
    It will then copy itself to %localappdata%/draggie/trimmed_client/client.exe and initiate a Windows Startup shortcut to this\n
    This will ensure that dependencies and user specified programs will be updated automatically.\n
    In the future, add multiple checks for files using the DraggieTools API
    """
    current_build_version = get_build()
    log_print("[Main] Checking if there's an update available")
    if build < current_build_version: 
        # if build is less than current version - so there's an update available.
        log_print(f"[Main] [HTTPRequestInfo] Got build of this app: {current_build_version}, this is less than the current build {build}")
        log_print("[Main] Analysing binaryfile")
        try:
            # Attempt to download and start a new build if one is available on the server.
            log_print("[Main] Attempting to download a new version")
            download_new_version()
            # modulation()
            log_print(f"[Main] Attempting to start a file at '{update_directory}\\lily-v{current_build_version}.exe'")
            try:
                os.startfile(f'{update_directory}\\lily-v{current_build_version}.exe')
                log_print("[Main] Exiting! Successful update.")
                return sys.exit()
            except Exception as e:
                log_print(f"[Main] Unable to start the file at '{update_directory}\\lily-v{current_build_version}.exe' - {e}. {traceback.format_exc()}", level=4)
                log_print("[Main] Not exiting! Update failed.", level=4)
        except Exception as e:
            log_print(f"[Main] Exception has been caught: {e}", level=4)
            pass

    else:
        # Triggered if there is no update available according to the server.
        if os.path.isfile(f"{Client_AppData_Directory}\\OldExecutableDir.txt"): # Check if the old version has left a 'cleanup file' - this should only be there on the first launch of a new version
            log_print(f"[Main] [OverwriteOldVersion] {Client_AppData_Directory}\\OldExecutableDir.txt exists!", False, level=2)
            with open(f"{Client_AppData_Directory}\\OldExecutableDir.txt", "r") as file:
                old_sys_exe = file.read() # Get the contents of the old file.
                file.close()
            if old_sys_exe == str(sys.executable):
                log_print(f"[Main] The old sys exe, {old_sys_exe}, is the same as {sys.executable}.", True, 4)
                pass
            else:
                log_print("[Main] [OverwriteOldVersion] Removing OldExeDir.txt")
                os.remove(f"{Client_AppData_Directory}\\OldExecutableDir.txt")
                log_print("[Main] [OverwriteOldVersion] Removing old exe")
                try:
                    os.remove(old_sys_exe)
                    log_print("[Main] [OverwriteOldVersion] Copying current exe to old sys exe")
                    shutil.copyfile(str(sys.executable), target_path)
                    log_print(f"[Main] [OverwriteOldVersion] Copied current exe ({sys.executable}) to old sys exe ({old_sys_exe})")
                    w10_toast_notification("Draggie Games", f"Your Draggie Client software has been updated automatically to version {current_build_version}!", duration=10, icon_url="https://ibaguette.com/favicon.ico")
                except PermissionError as e:
                    log_print(f"[Main] [OverwriteOldVersion] PermissionError: Unable to remove old exe. {e}", level=4)
        else:
            if str(sys.executable) != target_path:
                if "python.exe" in str(sys.executable):
                    print("this is python, not the exe. developer mode is probably on.")
                else:
                    log_print(f"[Main] Current executable directory \"{sys.executable}\" is not equal to the target path \"{target_path}\"")
                    try:
                        log_print(f"[Main] Attempting to copy {sys.executable} into {target_path}")
                        shutil.copyfile(str(sys.executable), target_path)
                        log_print(f"[Main] [OverwriteOldVersion] Copied current exe ({sys.executable}) to target path ({target_path})")
                        log_print(f"[Main] Starting file at {target_path}")
                        os.startfile(target_path)
                        log_print(f"[Main] Started file at path {target_path}")
                        log_print("[Main] Quitting...")
                        sys.exit()
                    except shutil.SameFileError as e:
                        log_print(f"[Main] File already exists: {e}", level=3)
                    except PermissionError as e:
                        log_print(f"[Main] PermissionError: {e}", level=3)
                    except Exception as e:
                        log_print(f"[Main] Error while unconditionally copying file. This may be a good thing - {e}. {traceback.format_exc()}", level=3)
                    upload_logs(None, True, True)
            else:
                log_print(f"[Main] The current path, \"{target_path}\" is the same as the executable directory. The file does not need to be copied into the preferred directory.", False)
        log_print(f"[Main] No updates are currently needed. Sitting as a background process, next update in {update_refresh_duration_sec} seconds. Current version: {build}")

        log_print(f"[Main] Trimmed version has no optional features, continuing. {e}\n{traceback.format_exc()}", level=4)

        try:
            draggietools_updater()
        except Exception as e:
            log_print(f"[Main] Error updating DraggieTools but continuing. {e}\n{traceback.format_exc()}", level=4)

        try:
            saturnian_updater()
        except Exception as e:
            log_print(f"[Main] Error updating Saturnian but continuing. {e}\n{traceback.format_exc()}", level=4)

        log_print(f"[Main] Sleeping for {update_refresh_duration_sec} seconds, or until {datetime.datetime.now() + datetime.timedelta(seconds=update_refresh_duration_sec)}", True, level=2)

        try:
            log_print("[Main] Syncing current log to server")
            upload_logs(file_path=None, current=True)
        except Exception as e:
            log_print(f"[Main] Error uploading current log but continuing. {e}\n{traceback.format_exc()}", level=4)

        log_print("[Main] Sleeping started.")

        await asyncio.sleep(update_refresh_duration_sec)
        await main()

# -*-*-*-*-* START *-*-*-*-*-


try:
    log_print("[init] Starting up...")
    upload_logs(None, True, True)
    startup()
except Exception as e:
    log_print(f"[init/ERROR] StartupError!: {e}\n{traceback.format_exc()}")
    time.sleep(10)
    startup()

upload_logs(None, True, True)
sys.exit()
