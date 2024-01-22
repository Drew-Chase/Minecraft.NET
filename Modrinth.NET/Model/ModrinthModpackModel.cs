// Ignore Spelling: Modrinth Modpack

using Newtonsoft.Json;

namespace Chase.Minecraft.Modrinth.Model
{
    /// <summary>
    /// Represents a Modrinth modpack model.
    /// </summary>
    public struct ModrinthModpackModel
    {
        /// <summary>
        /// Gets or sets the format version of the modpack.
        /// </summary>
        [JsonProperty("formatVersion")]
        public int FormatVersion { get; set; }

        /// <summary>
        /// Gets or sets the game associated with the modpack.
        /// </summary>
        [JsonProperty("game")]
        public string Game { get; set; }

        /// <summary>
        /// Gets or sets the version ID of the modpack.
        /// </summary>
        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the modpack.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a summary describing the modpack.
        /// </summary>
        [JsonProperty("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets an array of files in the modpack.
        /// </summary>
        [JsonProperty("files")]
        public ModpackFile[] Files { get; set; }

        /// <summary>
        /// Gets or sets the dependencies of the modpack.
        /// </summary>
        [JsonProperty("dependencies")]
        public ModpackDependency Dependencies { get; set; }
    }

    /// <summary>
    /// Represents a file in the modpack.
    /// </summary>
    public struct ModpackFile
    {
        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the hashes of the file.
        /// </summary>
        [JsonProperty("hashes")]
        public Hashes Hashes { get; set; }

        /// <summary>
        /// Gets or sets an array of download URLs for the file.
        /// </summary>
        [JsonProperty("downloads")]
        public string[] Downloads { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
    }

    /// <summary>
    /// Represents the dependencies of the modpack.
    /// </summary>
    public struct ModpackDependency
    {
        /// <summary>
        /// Gets or sets the Minecraft version dependency.
        /// </summary>
        [JsonProperty("minecraft")]
        public string Minecraft { get; set; }

        /// <summary>
        /// Gets or sets the Fabric Loader version dependency.
        /// </summary>
        [JsonProperty("fabric-loader")]
        public string FabricLoader { get; set; }


        /// <summary>
        /// Gets or sets the Forge Loader version dependency.
        /// </summary>
        [JsonProperty("forge")]
        public string ForgeLoader { get; set; }
    }
}
