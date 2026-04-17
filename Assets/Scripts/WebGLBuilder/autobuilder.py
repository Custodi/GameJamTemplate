import os
import subprocess
import time
import zipfile

# Configuration
UNITY_PATH = "D:/Unity Editors/6000.0.58f2/Editor/Unity.exe"  # Path to unity editor .exe
PROJECT_PATH = "E:/Unity Projects/Game Jam Template"  # Path to unity project
BUILD_PATH = os.path.join(PROJECT_PATH, "Build/WebGL")  # Path to web build folder 
ZIP_PATH = os.path.join(PROJECT_PATH, "Build/WebGL.zip")  # Path to archive
ITCH_IO_CHANNEL = "ink-fox/gamejamtemplatetest:web"  # Itch's user name and game's link name username/game:web
BUTLER_PATH = "butler"  # If butler in PATH - use just 'butler'
CHECK_INTERVAL = 60  # Inverval how ofter stript checks new commits
BUILD_TAG = "[build]" # You can change tag here

def pause():
    input("Нажмите Enter, чтобы продолжить...")

def get_latest_commit_message():
    try:
        commit_message = subprocess.check_output(["git", "log", "-1", "--pretty=%B"]).strip().decode("utf-8")
        return commit_message
    except subprocess.CalledProcessError as e:
        print(f"Error when get commit: {e}")
        pause()
        return None

def get_latest_commit_hash():
    try:
        return subprocess.check_output(["git", "rev-parse", "HEAD"]).strip().decode("utf-8")
    except subprocess.CalledProcessError as e:
        print(f"Error getting commit hash: {e}")
        pause()
        return None

def pull_latest_changes():
    try:
        subprocess.check_call(["git", "pull"])
        print("Pulled changes")
    except subprocess.CalledProcessError as e:
        print(f"Error git pull: {e}")
        pause()

def build_webgl():
    log_file = os.path.join(PROJECT_PATH, "unity_build.log")

    try:
        result = subprocess.run(
            [
                UNITY_PATH,
                "-quit", "-batchmode",
                "-projectPath", PROJECT_PATH,
                "-executeMethod", "WebGLBuilder.PerformWebGLBuild",  # путь к твоему статическому методу
                "-buildTarget", "WebGL",
                "-logFile", log_file
            ],
            capture_output=True,
            text=True
        )

        print("Unity exited with code:", result.returncode)

        # Читаем лог Unity
        if os.path.exists(log_file):
            with open(log_file, "r", encoding="utf-8", errors="ignore") as f:
                build_log = f.read()
            print("\n===== Unity Build Log Start =====")
            print(build_log[-2000:])  # выводим последние ~2000 символов, чтобы не захламить
            print("===== Unity Build Log End =====\n")
        else:
            print("⚠️ unity_build.log не найден")

        if result.returncode == 0:
            print("✅ Build WebGL finished without subprocess errors")
            # Проверим наличие папки билда
            if not os.path.exists(BUILD_PATH) or not os.listdir(BUILD_PATH):
                print("⚠️ Build folder is empty, Unity likely failed internally.")
                return False
            return True
        else:
            print("❌ Build failed. See log above.")
            return False

    except subprocess.CalledProcessError as e:
        print(f"Build Error: {e}")
        return False


def zip_build_folder():
    # Creating a folder if it not
    os.makedirs(os.path.dirname(ZIP_PATH), exist_ok=True)

    if os.path.exists(ZIP_PATH):
        os.remove(ZIP_PATH)

    with zipfile.ZipFile(ZIP_PATH, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, _, files in os.walk(BUILD_PATH):
            for file in files:
                full_path = os.path.join(root, file)
                relative_path = os.path.relpath(full_path, BUILD_PATH)
                zipf.write(full_path, relative_path)
    print("Archive finished.")

def upload_to_itch():
    try:
        subprocess.check_call([BUTLER_PATH, "push", ZIP_PATH, ITCH_IO_CHANNEL])
        print("Upload to itch.io finished.")
    except subprocess.CalledProcessError as e:
        print(f"Error during upload to itch.io: {e}")
        pause()

def main():
    last_commit_message = ""
    last_commit_hash = ""
    
    while True:
        pull_latest_changes()
        
        commit_message = get_latest_commit_message()
        commit_hash = get_latest_commit_hash()

        if last_commit_hash != commit_hash:
            if commit_message and BUILD_TAG.lower() in commit_message.lower(): 
                print("Tag found, starting build...")
                last_commit_hash = commit_hash

                if build_webgl():
                    zip_build_folder()
                    upload_to_itch()
                else:
                    print("Build failed")
            else:
                print("Last commit doesn't contain [build] tag. Skip build.")
        else:
            print("Same commit. Skip build")
        
        time.sleep(CHECK_INTERVAL)
        pause()

if __name__ == "__main__":
    main()
