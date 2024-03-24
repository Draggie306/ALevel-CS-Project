import subprocess, os

script_directory = r"D:\rclone-v1.62.2-windows-amd64\rclone.exe"
og_copy_to = "r2demo:draggiegames-library/"


# auto detect build number
folder_with_builds = r"D:\OneDrive - Notre Dame High School\Unity Projects\Saturnian"

# allow for non-integer build numbers
#auto_build_number = max([int(x) for x in os.listdir(folder_with_builds) if os.path.isdir(os.path.join(folder_with_builds, x))])

auto_build_number = 0
for x in os.listdir(folder_with_builds):
    if os.path.isdir(os.path.join(folder_with_builds, x)):
        try:
            if int(x) > auto_build_number:
                auto_build_number = int(x)
        except ValueError:
            print(f"Skipping folder \"{x}\" as it is not an integer.")
print(f"\n\nDetected largest build number: {auto_build_number}")


# build_number = input("If this is correct, press enter. Otherwise, enter the correct build number:\n\n>>> ")
# if build_number == "":
build_number = auto_build_number
print(f"\n[auto] Set build number: {build_number}")

build_folder_dir = os.path.join(folder_with_builds, str(build_number))

# default_build_dir = input(f"\n\nThis will zip the contents of \"{folder_with_builds}\\{build_number}\".\nIf this is correct, press enter. Otherwise, enter the correct path:\n\n>>> ")
# if default_build_dir == "":
#     default_build_dir = build_folder_dir
default_build_dir = build_folder_dir

print(f"\n[auto] Set default bild dir to: {default_build_dir}")

# Package the directory contents into a zip file
# Output to D:\OneDrive - Notre Dame High School\Unity Projects
zipfile_name = f"D:\\OneDrive - Notre Dame High School\\Unity Projects\\saturnian\\{build_number}\\build\\build.zip"
shard_path = f"D:\\OneDrive - Notre Dame High School\\Unity Projects\\saturnian\\{build_number}\\build\\shards"

if not os.path.exists(shard_path):
    os.makedirs(shard_path)

if os.path.exists(zipfile_name):
    print("\nThe zip file already exists. Overwriting...")

print(f"Zipping the build directory to {zipfile_name}...")

os.chdir(default_build_dir)
subprocess.run(["powershell.exe", f"Compress-Archive -Path * -DestinationPath '{zipfile_name}' -Force"])

copy_to = f"{og_copy_to}saturnian/builds/{build_number}"  # TODO: change build number to uuid to prevent reverse engineering and copying of builds

print(f"\nUploading {zipfile_name} to {copy_to} on Cloudflare R2...")
ps_script = r"D:\rclone-v1.62.2-windows-amd64\rclone.exe copy -P " + "'" + zipfile_name + "'" + " " + copy_to # todo; show progress but cant from subprocess.run

# if filesize is bigger than 500 give warning
if os.path.getsize(zipfile_name) > 512000000:
    print("WARNING: The file size is larger than 512MB, the Cloudflare CDN will not cache this file, and it will be served from the raw R2 bucket. Consider splitting the build into shards.")

use_sharding = input("Press 1 to shard the build\n\n>>> ")


# Shardify the build!

def shardsplit2(file, size, output_dir):
    print(f"Splitting {file} into shards of {size}MB...")

    filesizebytes = os.path.getsize(file)

    for i in range(0, filesizebytes, size * 1000000):
        with open(file, "rb") as f:
            f.seek(i)
            print(f"Reading {size}MB from {i}...")
            shard = f.read(size * 1000000)
            with open(f"{output_dir}\\shards\\{i // (size * 1000000)}", "wb") as shardfile:
                shardfile.write(shard)
                print(f"Wrote shard {i // (size * 1000000)}")

    recombinethem = input("Recombine the shards? (1/0)\n\n>>> ")
    if recombinethem == "1":
        recombine_shards(f"{output_dir}\\shards", f"{output_dir}\\recombined.zip")


def recombine_shards(shard_dir, output_file):
    print("let's recombine them")

    bytes_written = 0
    for shard in os.listdir(shard_dir):
        with open(f"{shard_dir}\\{shard}", "rb") as f:
            sharddata = f.read()
            print(f"Read shard {shard + 1}")
            bytes_written += len(sharddata)
            with open(output_file, "ab") as output:
                output.write(sharddata)
                print(f"Wrote shard {shard + 1} to {output_file}")
                print(f"Written {bytes_written} bytes sofar")


def shardsplit(file, size, output_dir):
    print(f"Splitting {file} into shards of {size}MB...")
    subprocess.run(["powershell.exe", f"split -b {size}MB -d {file} {output_dir}\\shards"])

    # Upload the shards to the cloud
    print(f"Uploading shards to {copy_to} on Cloudflare R2...")
    ps_script = r"D:\rclone-v1.62.2-windows-amd64\rclone.exe copy " + f"{output_dir}\\shards* " + copy_to

    completed_process = subprocess.run([
        "powershell.exe",
        f"Set-Location -Path 'D:\\rclone-v1.62.2-windows-amd64'; {ps_script}"
    ], capture_output=True, text=True)

    if completed_process.returncode == 0:
        print("Shards uploaded successfully.")
    else:
        print("Shard upload failed.")
        print("Error Output:")
        print(completed_process.stderr)

    print("Done.")


if use_sharding == "1":
    shardsplit2(zipfile_name, 100, f"D:\\OneDrive - Notre Dame High School\\Unity Projects\\saturnian\\{build_number}\\build")

else:

    print(f"Executing:\n\n{ps_script}\n\n")
    # This step is important to ensure that relative paths in the PowerShell script work correctly

    print("\nUploading to 09b65a1a66a15b67892e49451e44dbde.r2.cloudflarestorage.com. Do not close this window until the upload is complete, or the upload will be corrupted.")
    completed_process = subprocess.run([
        "powershell.exe",
        f"Set-Location -Path 'D:\\rclone-v1.62.2-windows-amd64'; {ps_script}"
    ], capture_output=True, text=True)

    if completed_process.returncode == 0:
        print("PowerShell script execution successful.")
    else:
        print("PowerShell script execution failed.")
        print("Error Output:")
        print(completed_process.stderr)

    print("Done.")


    # Now generate and upload text file with build number
    build_number_file = "D:\\OneDrive - Notre Dame High School\\Unity Projects\\saturnian\\alpha_build_number.txt"

    print(f"Generating build number file at {build_number_file}...")
    with open(build_number_file, "w") as file:
        file.write(str(build_number))

    copy_to = f"{og_copy_to}builds/"

    print(f"Uploading the build number, {build_number}, to {copy_to} on Cloudflare R2...")
    ps_script = r"D:\rclone-v1.62.2-windows-amd64\rclone.exe copy -P " + "'" + build_number_file + "'" + " " + copy_to

    completed_process = subprocess.run([
        "powershell.exe",
        f"Set-Location -Path 'D:\\rclone-v1.62.2-windows-amd64'; {ps_script}"
    ], text=True)

    if completed_process.returncode == 0:
        print(f"Updated build number file to {build_number} successfully.")
    else:
        print("Failed to update build number file.")

print("\nLast step! Go to the raspi and set the env variable to the build number. Then run sudo systemctl restart draggiegames to restart the server.")
print(f"\nBuild number: {build_number}\nURL: https://draggiegames.library.content.euwest0002.prod.draggie.games/saturnian/builds/{build_number}/build.zip")