import os
import json



version = 1
compatible_target_version_str = "1.0.0"
ver_hash = "12715b2fddd9c05bdb8aefe2acce16fe95982daf"

base = {
    "version": version,
    "compatible_target_version_str": compatible_target_version_str,
    "h": ver_hash,
    "name": "saturnian cms assets",
    "description": "saturnian cms assets",
    "assets_base_url": "https://assets.draggie.games/saturnian-content/",
}

path = "D:\\Unity\\Saturnian\\CMS Assets"

files = []

overwrites = {  # Apply the default settings to all files, however, some may be changed.
    "Cats.png": {
        "cacheable": False
    },
    "Renu.png": {
        "cacheable": False,
        "displayName": "Renushree (Cute)"
    },
    "lily": {
        "cacheable": False,
        "displayName": "Lily [3D Model]",
        "fervour": "legendary",
    }
}


def generate_json():
    items = 0
    for file in os.listdir(f"{path}\\input"):

        if os.path.isdir(f"{path}\\input\\{file}"):
            print(f"\n\n[INFO] Skipping directory {file}")
            continue

        if not file.endswith(".py") or not file.endswith(".json"):
            asset_info = {
                "name": file,
                "cacheable": True,
                "displayName": file
            }

            if file in overwrites:
                for key, value in overwrites[file].items():
                    asset_info[key] = value

                print(f"[INFO] applied overwrites to {file} with settings {overwrites[file]}")

            files.append(asset_info)
            print(f"Added {file} to manifest.json with settings {asset_info}")
            items += 1

        base["num_items"] = items

    with open(f"{path}\\manifest.json", "w") as f:
        print(f"[INFO] Finished with {items} items, generating manifest.json")
        f.write(json.dumps({
            **base,
            "files": files
            }
        ))
        #indent=4)) # Harder for dataminers ;)

        print("[INFO] Finished writing manifest.json, ready to upload to cms server.")


generate_json()
