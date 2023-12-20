import getpass
import sys
import os
from os import environ, listdir, mkdir, path, remove, startfile, system
from subprocess import Popen
from datetime import datetime
from time import sleep, time
from uuid import uuid4
from tqdm import tqdm
from shutil import copyfile
import pathlib
from typing import Optional
import random
import traceback
import logging
import zipfile
from threading import Event, Thread
import base64
from math import ceil
from cryptography.fernet import Fernet
import json
import hashlib
import shutil
import psutil
import requests
import winshell

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
client = None

phrases = {
    'english': {
        'menu_options': '\n[0] Quit\n[1] Open Project Menu\n[2] Check for update\n[3] Change language\n[4] View source code\n[5] Project Saturnian\n[6] DraggieClient\n[7] Dev Menu\n[8] Upload logs\n\n>>> ',
        'key_error': 'Key error occurred: ',
        'backup': '\n\nResorting to backup',
        'downloading': 'Downloading.',
        'done_speed': 'done, average speed',
        'check_update': 'Checking for update...',
        'download_update': 'Downloading update...',
        'run_from': '\nRunning from',
        'menu_prompt': f'\n{green_colour}What would you like to do, my friend?\n',
        'server_says': 'The server says the newest build is',
        'running_version': '\nRunning version',
        'at': '@',
        'build': 'build',
        'update_available': '\nUpdate available!',
        'on_version': 'You are on version',
        'newest_version_build': 'The newest version is build',
        'press_enter_update': 'Press enter to download the update!',
        'downloading_opening': 'Downloading and opening up the source Python file in Explorer. To view it, open it in Notepad or you could upload it to an IDE online.',
        'which_build': 'which is build',
        'quitting': 'Quitting...',
        'newer_version': '\n\nHey, you\'re running on a version newer than the public build. That means you\'re very special UwU\n',
        'secret_menu': 'Welcome to the secret menu.',
        'skipping_file': 'Skipping file',
        'unsupported_extension': 'as it does not have a supported extension or it will not work.',
        'log_notice': 'Would you like to upload log files before deleting them? (Y/N)',
        'interacting_allowed': "Interacting allowed for",
        'file': 'file.',
        'input_threads': "Input the amount of threads to use to download files with:\n\n>>> ",
        'select_options': "Select options:\n\n1) See basic info and fingerprint hash\n2) Compare music to old version and extract additions\n3) Compare files to another version\n4) Download all background music files\n5) Download all files containing a string\n6) Open this archive's downloaded file folder\n0) Go back   \n\n>>> ",
        'read_info_from_file': "Read the following information from file",
        'amount_of_files': "Amount of files",
        'fingerprint_hash': "Fingerprint hash",
        'found_music_in_file': 'Found "music/background" in file: ',
        'search_term_input': "Enter the term to search all files for and it will be downloaded:\n\n>>> ",
        'search_all_archives': "[1] to search through ALL archives located in the DownloadedBuilds directory. Only select if you want to download multiple versions' assets.\n[Enter] Search for",
        "only_in_current_version": "only in current version: ",
        'searching_archive': "Searching archive: ",
        'found': '\nFound',
        "in_file": 'in file:',           # {phrases[language]['']}
        'not_downloading_exists': 'Not downloading the file due to it already being present. Please remove ',
        'not_downloading_exists_2': ' if you want it to be redownloaded.',
        'unsupported_extension_1': "Skipping file",
        'unsupported_extension_2': 'as it does not have a supported extension or it will not work.',
        'matching_files': 'matching files across',
        "total_files_in": 'total files in',
        "available_archives": 'available archives.',
        "S_as": 'Extracting Supercell game assets',
        "S_as_1": "files searched, ",
        "S_as_2": "downloaded.",
        "files_already_exist": 'files already exist.\n',
        'read_asset': "Read the following asset version from APK file: ",
        'apk_name_warning': "[IMPORTANT] As this is an APK file, the name has been set to default to Brawl Stars. ",
        'detected_architecture': "Detected Architecture: ",
        'unresolved_error': "\n[WARNING] An error has occured which cannot be resolved: ",
        'encrypted_csv_path_input': "Enter the path to the encoded file:\n\n>>> ",
        'processing': "Processing...",
        'unpacked_file_success': "\n\nSuccessfully unpacked the file. You can now view it at ",
        'supercell_archive_location': r"Enter the location of your Supercell archive file, e.g D:\Downloads\brawl.ipa. IPA files are preferred.",
        'select_option_supercell_archive': "\nUse an .ipa file or .apk file (for iOS and Android decices, respectively). Must not be unzipped.\n[0] Go back.\n[1] Search for all downloadable versions.\n[2] Decode a file with LZMA.",
        'select_downloaded_file': "\n[Enter] Select one of the",
        'select_downloaded_file_2': 'downloaded files.',
        'fetching_trusted_versions': "Fetching a list of all trusted versions from GitHub...",
        'download_all_builds_warning': "You have chosen to download ALL builds. If you would like to stop it, you will need to press Ctrl+C.",
        'downloading_build': "Downloading the build",
        'cumms_from': "comes from",
        'download_new_files': "Would you like to download new files?",
    },
    'french': {
        'menu_options': '\n[0] Quitter\n[1] Ouvrir le menu du projet\n[2] Vérifier les mises à jour\n[3] Changer de langue\n[4] Voir le code source\n[5] Project Saturnian\n[6] DraggieClient\n[7] Menu secret\n[8] Téléverser les journaux\n\n>>> ',
        'key_error': 'Erreur de clé: ',
        'backup': '\n\nRecourir à la sauvegarde',
        'downloading': 'Téléchargement.',
        'done_speed': 'terminé, vitesse moyenne',
        'check_update': 'Vérification des mises à jour...',
        'download_update': 'Téléchargement de la mise à jour...',
        'run_from': '\nLancement à partir de',
        'menu_prompt': 'Que souhaitez-vous faire, mon ami?',
        'server_says': 'Le serveur indique que la dernière version est',
        'running_version': '\nVersion en cours d\'exécution',
        'at': '@',
        'build': 'build',
        'update_available': '\nMise à jour disponible !',
        'on_version': 'Vous êtes sur la version',
        'newest_version_build': 'La dernière version est la build',
        'press_enter_update': 'Appuyez sur Entrée pour télécharger la mise à jour !',
        'downloading_opening': 'Téléchargement et ouverture du fichier Python source dans Windows Explorer. Pour le voir, ouvrez-le dans le Bloc-notes ou vous pouvez le télécharger dans une IDE en ligne.',
        'which_build': 'qui est la build',
        'quitting': 'Quitter...',
        'newer_version': '\nHé, vous utilisez une version plus récente que la version publique. Cela signifie que vous êtes très spécial, selon moi.\n',
        'secret_menu': 'Bienvenue dans le menu secret.',
        'skipping_file': 'Ignorer le fichier',
        'unsupported_extension': 'car il n\'a pas d\'extension prise en charge ou ne fonctionnera pas.',
        'log_notice': 'Voulez-vous téléverser les fichiers avant de les supprimer ? (O/N)',
        'menu_options': '\n\n[0] Quitter\n[1] Installer sur le bureau\n[2] Installer dans un répertoire personnalisé\n[3] Actualiser les mises à jour\n[4] Changer de langue\n[5] Voir le code source\n[6] Modifier les paramètres de Fortnite\n[7] ProjectSaturnian\n[8] Téléchargeur de torrents\n[9] Extracteur AutoBrawl\n\n>>> ',
        'download_new_files': "Voulez-vous télécharger de nouveaux fichiers ?",
    }
}


def l10n_text(key: str) -> str:
    try:
        if not language:
            value = key
        else:
            value = phrases[language][f'{key}']
        return value
    except Exception as e:
        log(f"[LocalisedText] Error occurred when getting localised string: {e}\n{traceback.format_exc()}", 4)
        return key


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
        if log_level <= 2:
            print(f"{text}{reset_colour}")
        elif log_level == 3:
            print(f"{yellow_colour}{text}{reset_colour}")
        else:
            print(f"{red_colour}{text}{reset_colour}")
    else:
        if log_level == 1:
            # print(f"{text}{cyan_colour} [debug] {reset_colour}")
            pass


global stop_event, thread


def start_anim_loading(text):
    global stop_event, thread
    stop_event = Event()
    thread = Thread(target=loading_icon, args=(stop_event, text))
    thread.start()


def stop_anim_loading():
    global stop_event, thread
    stop_event.set()
    thread.join()


def loading_icon(stop_event, text):
    while not stop_event.is_set():
        for i in ["|", "/", "-", "\\"]:
            print(f"\r{text} {i}", end="")
            sleep(0.1)


def refresh():
    with open(__file__) as fo:
        source_code = fo.read()
        byte_code = compile(source_code, __file__, "exec")
        exec(byte_code)


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


def download_chunk(url, start, end, save_dir, pbar):
    """
    Downloads a chunk of a file.
    """
    headers = {'Range': f'bytes={start}-{end}'}
    response = dash_get(url, headers=headers, stream=True)
    with open(f"{save_dir}.part{start}", "wb") as f:
        for data in response.iter_content(1024):
            f.write(data)
            pbar.update(len(data))


def change_language() -> str:
    global language, language_chosen
    language = None

    def write_language_file(language):
        with open(f"{DraggieTools_AppData_Directory}\\Language_Preference.txt", "w") as x:
            x.close()
        with open(f"{DraggieTools_AppData_Directory}\\Language_Preference.txt", "w") as x:
            x.write(language)
            x.close()
    while language is None:
        x = input("Choose language\nChoisissez la langue\nEnglish = 1, French = 2\n\n>>> ")
        if x == "2":
            log("La langue est maintenant francais.")
            write_language_file("french")
            log(f"({datetime.now()}.strftime('%Y-%m-%d %H:%M:%S'): File at path '{DraggieTools_AppData_Directory}\\Language_Preference.txt' written with 'French'")
            language_chosen = "french"
        else:
            log("Language updated to English.")
            write_language_file("english")
            log(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}: File at path '{DraggieTools_AppData_Directory}\\Language_Preference.txt' written with 'English'")
            language_chosen = "english"
    if dev_mode:
        log(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}: Language successfully changed to {language_chosen}")
    return language_chosen


def get_language() -> str:
    """Returns the user's language. Currently `english` or `french`.\nIf the language file doesn't exist, it will be created."""
    if path.exists(f"{DraggieTools_AppData_Directory}\\Language_Preference.txt"):
        try:
            with open(f"{DraggieTools_AppData_Directory}\\Language_Preference.txt", encoding="UTF-8") as x:
                language_read = x.read()

            if language_read == "french":
                language = 'french'
                log("Langue mise à jour : français.")
            else:
                language = 'english'
                log(f"{clear_above_line_overwrite}Language set to English.")
        except Exception:
            language = change_language()
    else:
        log("[get_language] Language file doesn't exist when checking, using blocking change_language function", 3, False)
        language = change_language()
    return language


language = get_language()
log(f"[MainInit] Language set to {language}", 2, False)


def download_update(current_build_version):
    try:
        if not path.exists(f'{DraggieTools_AppData_Directory}\\UpdatedBuilds'):
            mkdir(f'{DraggieTools_AppData_Directory}\\UpdatedBuilds')

        tqdm_download("https://github.com/Draggie306/ALevel-CS-Project/blob/main/python/tools/dist/DraggieTools_Trimmed.exe?raw=true", f"{DraggieTools_AppData_Directory}\\UpdatedBuilds\\DraggieTools-{current_build_version}.exe")

        with open(f"{Draggie_AppData_Directory}\\OldExecutableDir.txt", "w") as file:
            file.write(f"{sys.executable}")
            file.close()

    except KeyError as e:
        log(f"Some error occured: {e}\n\nResorting to fallback method. Preferences will not be used, saving to default directory.")
        log(f"{language[0]}{e}{language[1]}")
        r = dash_get('https://github.com/Draggie306/ALevel-CS-Project/blob/main/python/tools/dist/DraggieTools_Trimmed.exe?raw=true')
        with open(f'{current_directory}\\DraggieTools-{current_build_version}.exe', 'wb') as f:
            f.write(r)


def view_source():
    log(phrases[language]['downloading_opening'])
    x = time()
    r = dash_get('https://raw.githubusercontent.com/Draggie306/DraggieTools/main/DraggieTools.py')
    with open(f'{DraggieTools_AppData_Directory}\\SourceCode\\DraggieTools-v{version}-{build}-{x}.py', 'wb') as f:
        f.write(r.content)
    Popen(f'explorer /select,"{DraggieTools_AppData_Directory}\\SourceCode\\DraggieTools-v{version}-{build}-{x}.py"')


def check_for_update():
    try:
        log(phrases[language]['check_update'], 2, True)
    except Exception:
        change_language()
        check_for_update()
    try:
        if path.isfile(f"{Draggie_AppData_Directory}\\OldExecutableDir.txt"):  # OverwriteOldVersion
            log(f"[OverwriteOldVersion] {Draggie_AppData_Directory}\\OldExecutableDir.txt exists!", 1, False)
            with open(f"{Draggie_AppData_Directory}\\OldExecutableDir.txt", "r") as file:
                old_sys_exe = file.read()
                file.close()
            if old_sys_exe == str(sys.executable):
                log("[WARNING] The update cannot be applied to the current directory as you are running the file in the same place! Please download the update and wait for it to be closed.", 4)
            else:
                log("[OverwriteOldVersion] Removing OldExeDir.txt")
                remove(f"{Draggie_AppData_Directory}\\OldExecutableDir.txt")
                log("[OverwriteOldVersion] Removing old exe")
                remove(old_sys_exe)
                log("[OverwriteOldVersion] Copying current exe to old sys exe")
                copyfile(str(sys.executable), old_sys_exe)
                log(f"[OverwriteOldVersion] Copying current exe ({sys.executable}) to old sys exe ({old_sys_exe})")
        else:
            log("[OverwriteOldVersion] OldExeFile does not exist!", 3, False)
    except Exception as e:
        log(f"Unable to overwrite older version. {e}", 4)

    try:
        current_build_version = int((dash_get('https://raw.githubusercontent.com/Draggie306/ALevel-CS-Project/master/python/tools_build.txt')).text)
    except Exception as e:
        log(f"\nUnable to check for update. {e}\n\nIt looks like the GitHub update servers might be blocked by your network! I'll still work, but some features might be limited.", 4)
        current_build_version = build
    if build < current_build_version: # if build is less than current version - so there's an update available.
        release_notes = str((dash_get(f"https://raw.githubusercontent.com/Draggie306/ALevel-CS-Project/master/python/tools_releasenotes/{current_build_version}.txt")).text)
        log(f"\n{phrases[language]['update_available']} {phrases[language]['on_version']} {version} {phrases[language]['which_build']} {build}.\n{phrases[language]['newest_version_build']} {current_build_version}\n\n", event="success")
        if language == "english":
            versions_to_get = current_build_version - build
            if versions_to_get == 1:
                log(f"You're {versions_to_get} build behind latest")
            else:
                log(f"You're {versions_to_get} builds behind latest")

            string = (f"Latest release notes (v{current_build_version}):\n\n{release_notes}\n")

            while current_build_version != (build + 1):
                current_build_version = current_build_version - 1
                version_patch = str((dash_get(f"https://raw.githubusercontent.com/Draggie306/ALevel-CS-Project/master/python/tools_releasenotes/{(current_build_version)}.txt")).text)
                string = (string + f"\nv{current_build_version}:\n{version_patch}\n\n")
            log(f"\n{string}\n")

        update_choice = input(f"{phrases[language]['press_enter_update']}\n\n>>> ")
        if update_choice != "":
            log("Skipping update.")
            return

        log(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}: {language[6]} - {DraggieTools_AppData_Directory}")
        download_update(current_build_version)
        log("Update downloaded. Launching new version...")
        if not desktop_install_path:
            startfile(f'{DraggieTools_AppData_Directory}\\UpdatedBuilds\\DraggieTools-{current_build_version}.exe')
        else:
            desktop_dir = pathlib.Path.home() / 'Desktop'
            startfile(f'{desktop_dir}\\DraggieTools-{current_build_version}.exe')
        sys.exit()

    if current_build_version < build:
        log(phrases[language]['newer_version'])
    else:
        log(f"{phrases[language]['running_version']} {version} - {phrases[language]['build']} {build} - {phrases[language]['at']} {datetime.fromtimestamp(build_date).strftime('%Y-%m-%d %H:%M:%S')}. {phrases[language]['server_says']} {current_build_version}.")


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
        encrypted_binary_token = fernet_key.encrypt(token.encode())
        log(f"[encrypt_token] encrypted_binary_token: {encrypted_binary_token}")
        # convert the binary token to a string
        encrypted_token = encrypted_binary_token.decode()
        log(f"[encrypt_token] encrypted_token: {encrypted_token}")
        return encrypted_token

    def decrypt_token(encrypted_token):
        log("[decrypt_token] decrypting token")
        token = bytes(encrypted_token, encoding="utf-8")
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

    cached_token = read_tokenfile_contents()

    if not cached_token:
        log(f"\n\n[ProjectSaturnian] {red_colour}You must log in to download builds from the gameserver, and to validate your license.{reset_colour}", log_level=3)
        log(f"[ProjectSaturnian] {magenta_colour}If you do not have an account, you can create one at:{cyan_colour} https://alpha.draggiegames.com/register{reset_colour}", log_level=3)
        log(f"\n\n{yellow_colour}Please enter your Draggie Games login credentials below.{reset_colour}", log_level=3)
        email = input("\nDraggie Games email: ")
        password = getpass.getpass("\nPassword (will not be shown): ")
        log(f"{blue_colour}Logging in...")
        login = dash_post("https://client.draggie.games/login", json={"email": email, "password": password, "from": "SaturnianUpdater/DraggieTools"})

        if login.status_code == 200:
            log(f"\n\n{green_colour}Login successful.\n")
            server_token = login.json()["auth_token"]
            log(f"Server returned token: {server_token}", log_level=1)
            newly_encrypted_token = encrypt_token(server_token)
            log(f"newly_encrypted_token: {newly_encrypted_token}", log_level=1)
            write_token(newly_encrypted_token)
            log("Token written to file.", log_level=1)
            preferred_install_location = input("\n\nWould you like to install the required files to the default location [1] or a custom location [2]?\n\n>>> ")
            if preferred_install_location == "1":
                write_datafile_attribute("install_dir", saturnian_appdir)
            elif preferred_install_location == "2":
                custom_install_location = input("Please enter the full path to the directory you would like to install the game to.\n\n>>> ")
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
    except Exception:
        clear_token = input("There was an error reading the token file. Would you like to clear the cached token and try again? (y/n)\n\n>>> ")
        match clear_token:
            case "y":
                if not os.path.isdir(saturnian_appdir):
                    os.makedirs(saturnian_appdir, exist_ok=True)
                    log(f"[write_token] Created directory: {saturnian_appdir}")
                if os.path.isfile(f"{saturnian_appdir}\\token.bin"):
                    os.remove(f"{saturnian_appdir}\\token.bin")
                    log(f"[write_token] Deleted file: {saturnian_appdir}\\token.bin")
                else:
                    log(f"[write_token] File not found: {saturnian_appdir}\\token.bin", log_level=3)
                projectsaturnian()
            case _:
                log("Exiting...")
                return choice1()
        return choice1()

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
                choice1()
        else:
            log(f"[saturnian/errors.account] ERROR: Received Saturnian version status code: {x.status_code}", log_level=4)
            error_message = json.loads(x.content)
            log(f"[saturnian/errors.account] ERROR: {error_message['message']}", log_level=4)
            sleep(4)
            choice1()

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
        start_anim_loading("Decompressing...")
        with zipfile.ZipFile(f"{preferred_install_location}\\Saturnian.bin", "r") as zip_ref:
            zip_ref.extractall(f"{preferred_install_location}\\SaturnianGame")
        stop_anim_loading()
        sys.stdout.flush()
        sys.stdout.write("\r") # TODO: make it clear the line above
        sys.stdout.flush()
        log(f"\n{green_colour}[saturnian/buildDL] Extraction complete.")
        write_datafile_attribute("current_version", saturnian_current_version)

    if saturnian_current_version != read_datafile_attribute("current_version"):
        log(f"{yellow_colour}[saturnian/Updater] Local game version is different from server version! Input 1 to download and install the new version.")
        choice = input("\n\n>>> ")
        match choice:
            case "1":
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
            case "0":
                return choice1()
            case _:
                try:
                    download_saturnian_build()
                    log(f"\n{green_colour}[saturnian/Updater] Update completed!")
                except Exception as e:
                    return log(f"[saturnian/errors] Error downloading Saturnian build: {e}", log_level=4, event="error")

    to_open = input(f"\n\n{cyan_colour}Manage your installation of the project!{reset_colour}\n\n[0] Back to main menu\n[1] Open the game\n[2] Uninstall the project\n[3] Open the game folder\n[4] Change installation directory\n\n>>> ")
    match to_open.lower():
        case "0":
            return choice1()
        case "1":
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
        case "3":
            preferred_install_location = read_datafile_attribute("install_dir")
            Popen(f'explorer /select,"{preferred_install_location}\\SaturnianGame\\Saturnian.exe"')
        case "4":
            log("[saturnian/Updater] Changing installation directory...")
            start_time = time()
            try:
                new_saturnian_install_dir = input("Enter the new installation directory:\n\n>>> ")
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


def upload_log_file(file_path, delete_after_upload: Optional[bool] = False):
    url = "https://logs.draggie.games/tools"

    filename = path.basename(file_path)
    username = getpass.getuser()
    new_filename = f"[{username}]-{filename}"

    with open(file_path, 'rb') as f:
        contents = f.read()

    files = {
        'file': (new_filename, contents),
    }

    response = dash_post(url, files=files)
    if response.status_code == 429:
        log(f"{red_colour}Hit ratelimit while uploading the logfile {file_path}. Status code: {response.status_code}", 2)
        log(f"{red_colour}Waiting 20 seconds before trying again...", 2)
        sleep(20)
        upload_log_file(file_path)
    elif response.status_code != 200:
        log(f"{red_colour}Failed to upload the logfile {file_path}. Status code: {response.status_code}", 2)
    else:
        log(f"{green_colour}Uploaded the logfile {file_path}. Status code: {response.status_code}", 2)
        if delete_after_upload:
            os.remove(file_path)
            log(f"{green_colour}Deleted the logfile {file_path}", 2)


def upload_logs(most_recent: Optional[int] = None, no_confirm: Optional[bool] = True):
    logging_dir = f"{DraggieTools_AppData_Directory}\\Logs"
    files = listdir(logging_dir)
    files = [path.join(logging_dir, file) for file in files]

    # sort the files based on their creation time
    files.sort(key=path.getctime)

    # select the most recent x files
    if most_recent:
        files = files[-most_recent:]

    log(f"Files to upload: {files}", 1, False)

    if len(files) > 20:
        log(f"{yellow_colour}[WARNING] That's a lot of files to upload, you will probably hit the rate limit. Please confirm that you want to upload {len(files)} files.", 2)
        if no_confirm:
            confirm = input(f"{yellow_colour}Do you want to continue? (y/n)\n\n>>> ")
            if confirm.lower() == "y":
                pass
            else:
                return log(f"{yellow_colour}Cancelled upload.", 2)
        else:
            log(f"{yellow_colour}Continuing upload.", 2)
    # upload each file
    for file in files:
        upload_log_file(file)


def dev_menu():
    global build
    x = input("\n\n[1] Set build\n[2] Set version\n[3] Set unix time\n[4] Reload entire code (Dangerous)\n>>> ")
    match x:
        case "1":
            build = int(input("Enter the build number: "))
            choice1()
        case "2":
            version = str(input("Enter version number: "))
            choice1()
        case "3":
            build_date = int(input("Enter unix seconds (int): "))
            choice1()
        case "4":
            refresh()
        case _:
            choice1()


def draggieclient():
    # Project Lily: aka DraggieClient
    lily_initial_choice = input("\nWhat do you want to do?\n[0] Go back\n[1] Install\n[2] Uninstall\n[3] View Logs\n[4] Manage Settings\n\n>>> ")

    try:
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

    match lily_initial_choice:
        case "1":
            if not os.path.exists(f"{lily_ensure_appdata_dir}\\Logs"):
                os.makedirs(f"{lily_ensure_appdata_dir}\\Logs", exist_ok=True)
                log(f"{green_colour}[ProjectLily] Prerequisite directory made: {lily_ensure_appdata_dir}\\Logs")

            log("Downloading AutoUpdateClient build v45...")
            try:
                tqdm_download("https://lily.draggie.games/lily_trimmed.exe", target_path, return_exceptions=True)
                os.startfile(target_path)
                save_json()
                log(f"{green_colour}Your system now has DraggieClient installed! Running in the background, it will automatically keep all of your files by me up to date! Enjoy.")
            except PermissionError as e:
                log(f"[ProjectLily] {e}\nThe client is likely running! We don't need to install it again.", log_level=3)
            except Exception as e:
                log(f"[ProjectLily] An error occured: {e}: {traceback.format_exc()}", log_level=4)
        case "2":
            startup_path = os.path.join(winshell.startup(), "Client.lnk")
            if os.path.exists(startup_path):
                os.remove(startup_path)
            try:
                os.remove(os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client\\trimmed_lily.exe"))
            except Exception as e:
                log(f"[ProjectLily] Unable to remove client.exe - it is likely that it is running: {e}", log_level=3)

            for proc in psutil.process_iter():
                try:
                    target_path = os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client\\trimmed_lily.exe")
                    process = psutil.Process(proc.pid)
                    procname = "client.exe"
                    if proc.name() == procname:
                        process_exe = process.exe()
                        log(f"Found client.exe running at {process_exe}\nAttempting to kill process...")
                        if process_exe.lower() == target_path:
                            proc.kill()
                            log(f"{green_colour}Client has been killed.")
                        os.remove(os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client\\trimmed_lily.exe"))
                        log(f"{green_colour}[ProjectLily] Client has been uninstalled and removed from startup successfully.")
                except Exception as e:
                    log(f"[ProjectLily] Unable to kill and completely remove client.exe: {e}", log_level=3)
        case "3":
            log_subdir = os.path.expanduser("~\\AppData\\Local\\Draggie\\Trimmed_Client\\Logs")
            if not os.path.exists(log_subdir):
                os.makedirs(log_subdir, exist_ok=True)
            os.startfile(log_subdir)
        case "4":
            # return log("This is not yet implemented.")
            json_settings_dir = os.path.join(lily_ensure_appdata_dir, "Client_Settings.json")
            if not os.path.exists(json_settings_dir):
                with open(json_settings_dir, 'w') as f:
                    json.dump({
                        "startup": True,
                        "autoupdate": True,
                        "autoupdate_interval": "default",
                        "autoupdate_interval_unit": "default",
                    }, f)
            with open(json_settings_dir, 'r') as f:
                json_settings = json.load(f)
                for key, value in json_settings.items():
                    log(f"{key}: {value}", output=False)
                choice_client_settings = input("What do you want to do?\n[1] Change startup\n[2] Change autoupdate\n>>> ")
                match choice_client_settings:
                    case "1":
                        choice_client_settings_startup = input("Do you want to enable or disable startup?\n[1] Enable\n[2] Disable\n>>> ")
                        match choice_client_settings_startup:
                            case "1":
                                json_settings["startup"] = True
                                with open(json_settings_dir, 'w') as f:
                                    json.dump(json_settings, f)
                                log(f"{green_colour}[ProjectLily] Startup enabled.")
                            case "2":
                                json_settings["startup"] = False
                                with open(json_settings_dir, 'w') as f:
                                    json.dump(json_settings, f)
                                log(f"{red_colour}[ProjectLily] Startup disabled.")
                                draggieclient()
                    case "2":
                        choice_client_settings_autoupdate = input("Do you want to enable or disable autoupdate?\n[1] Enable\n[2] Disable\n>>> ")
                        match choice_client_settings_autoupdate:
                            case "1":
                                json_settings["autoupdate"] = True
                                with open(json_settings_dir, 'w') as f:
                                    json.dump(json_settings, f)
                                log(f"{green_colour}[ProjectLily] Autoupdate enabled.")
                            case "2":
                                json_settings["autoupdate"] = False
                                with open(json_settings_dir, 'w') as f:
                                    json.dump(json_settings, f)
                                log(f"{red_colour}[ProjectLily] Autoupdate disabled.")
                                draggieclient()
        case _:
            log(f"{red_colour}[ProjectLily] Invalid choice. Please try again.", log_level=3)
    sleep(1)
    return choice1()


def save_json():
    try:
        with open(os.path.join(DraggieTools_AppData_Directory, 'Client_AutoUpdateInfo.txt'), 'w') as f:
            json.dump({
                "build": build,
                "version": version,
                "buildTime": build_date,
                "tools_installation_directory": directory,
            }, f)
    except Exception as e:
        log(f"Unable to write current information: {e}")


def choice1():
    try:
        x = input(phrases[language]['menu_options'])
        print(reset_colour)
        if x == "0":
            log(f"Goodbye! {time()}\nHope you found me useful.", 2, False)
            log("\n\n\n\n\n\nQuitting...")
            if client:
                client.close()
            sys.exit()

        match x:
            case "1":
                print("Coming soon!")
            case "2":
                check_for_update()
            case "3":
                change_language()
            case "4":
                view_source()
            case "5":
                projectsaturnian()
            case "6":
                draggieclient()
            case "dev":
                dev_menu()
            case "log":
                upload_logs()
            case _:
                choice1()
        choice1()
    except KeyboardInterrupt:
        log("Abandoned by user request.", )
        try:
            sleep(1)
        except KeyboardInterrupt:
            log("Press Ctrl+C 3 more times to exit.")
            try:
                sleep(2)
            except KeyboardInterrupt:
                log("Press Ctrl+C 2 more times to exit..")
                try:
                    sleep(3)
                except KeyboardInterrupt:
                    log("Press Ctrl+C 1 more time to exit...")
                    try:
                        sleep(4)
                    except KeyboardInterrupt:
                        log("Goodbye!")
                        return sys.exit(0)

        log("Okay, I'll stay.")
        return choice1()
    except Exception as e:
        log(f"\n[WARNING] An unknown exception has occured: {e}\n\n{traceback.format_exc()}", 4)
        beans = input("Type 1 to upload your logs!\n\n>>> ")
        if beans == "1" or beans == "":
            upload_logs(5)
        return choice1()


def main():
    try:
        log(f"{phrases[language]['run_from']} {current_directory}")
    except Exception:
        log("First time run detected.")
        change_language()
        log(f"{phrases[language]['run_from']}", 2)
    log(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}: main() subroutine executed", 1, False)
    log(phrases[language]['menu_prompt'])
    save_json()

    choice1()


sys.stdout.write("\r")
sys.stdout.flush()
log(f"{clear_above_line_overwrite}Done! Loading base data...")

desktop_install_path = False

if path.exists(f"{DraggieTools_AppData_Directory}\\InstallDir_Pref.txt"):
    log("[MainInit] InstallDir_Pref exists.", 2, False)
    desktop_dir = pathlib.Path.home() / 'Desktop'
    with open(f"{DraggieTools_AppData_Directory}\\InstallDir_Pref.txt", "w+") as e:
        install_dir = e.read()
        if install_dir == str(desktop_dir):
            log(f"Determined install_dir to be desktop @ {desktop_dir}")
            desktop_install_path = True
        log(f"[MainInit] Determined install_dir {install_dir}. Read from file InstallDir_Pref", 2, False)
else:
    log("[MainInit] Setting new file preference exists.", 2, False)
    with open(f"{DraggieTools_AppData_Directory}\\InstallDir_Pref.txt", 'w+') as f:
        f.close()
    with open(f"{DraggieTools_AppData_Directory}\\InstallDir_Pref.txt", 'w') as f:
        f.write(f"{DraggieTools_AppData_Directory}\n{build}")

log("[MainInit] Checking for update", 2, False)
check_for_update()

current_directory = path.dirname(path.realpath(__file__))
if dev_mode:
    log(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}: variable currrent_directory assigned with value {current_directory}")


try:
    log("Done! Entering program...")
    main()
except Exception as e:
    log(f"An unhandled exception was encountered.\nShort: {e}\n\nLong:\n{traceback.format_exc()}\n\nIt would be appreciated if you generate a logfile and DM it Draggie#3060. Thanks!\n")
    logging.error(traceback.format_exc())
    beans = input("Type 1 to upload your logs!\n\n>>> ")
    if beans == "1" or beans == "":
        upload_logs()
    main()
