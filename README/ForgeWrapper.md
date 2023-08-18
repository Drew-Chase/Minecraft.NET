# Forge Wrapper

Forge Wrapper is a java wrapper for the forge installer allowing you to install forge client using the cli. This also does _**not**_ require the `launcher_profiles.json`

## Build

Build and package the jar file

```powershell
./gradlew shadowJar
```

## Usage

| Short | Long          | Parameters | Description                         |
| ----- | ------------- | ---------- | ----------------------------------- |
| `-h`  | `--help`      | NONE       | Display's the help                  |
| `-i`  | `--installer` | `<arg>`    | The path to the forge installer jar |
| `-o`  | `--output`    | `<arg>`    | The path to the output              |

#### Example

```powershell
java -jar .\ForgeWrapper-0.0.1-all.jar -i "/path/to/forge-installer.jar" -o "/path/to/minecraft/installation"
```
