/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Chase.Minecraft.Instances;

/// <summary>
/// Represents a manager for handling Minecraft instances.
/// </summary>
public class InstanceManager
{
    private readonly string path;

    /// <summary>
    /// Gets the dictionary of instances with their unique identifiers as keys.
    /// </summary>
    public Dictionary<Guid, InstanceModel> Instances { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceManager"/> class.
    /// </summary>
    /// <param name="path">The path to the directory where Minecraft instances are stored.</param>
    public InstanceManager(string path)
    {
        this.path = Directory.CreateDirectory(path).FullName;
        Instances = new();
        Load();
    }

    /// <summary>
    /// Creates a new Minecraft instance.
    /// </summary>
    /// <param name="instance">
    /// The <see cref="InstanceModel"/> representing the new instance to be created.
    /// </param>
    /// <returns>The created <see cref="InstanceModel"/> instance.</returns>
    public InstanceModel Create(InstanceModel instance)
    {
        try
        {
            instance.InstanceManager = this;
            instance.Path = Directory.CreateDirectory(Path.Combine(path, GetUniqueInstanceDirectoryName(instance.Name))).FullName;
            Instances.Add(instance.Id, instance);
            Save(instance.Id, instance);
        }
        catch (Exception e)
        {
            Log.Error("Unable to create instance.", e);
        }
        return instance;
    }

    /// <summary>
    /// Saves changes to an existing Minecraft instance.
    /// </summary>
    /// <param name="id">The unique identifier of the instance to be saved.</param>
    /// <param name="instance">
    /// The updated <see cref="InstanceModel"/> representing the instance to be saved.
    /// </param>
    /// <returns>The saved <see cref="InstanceModel"/> instance.</returns>
    public InstanceModel Save(Guid id, InstanceModel instance)
    {
        try
        {
            Log.Debug("Saving instance to file: {PATH}", Path.Combine(instance.Path, "instance.json"));
            instance.InstanceManager = this;
            instance.LastModified = DateTime.Now;
            Instances[id] = instance;
            File.WriteAllText(Path.Combine(instance.Path, "instance.json"), JsonConvert.SerializeObject(instance));
        }
        catch (Exception e)
        {
            Log.Error("Unable to save instance: {ID}", id, e);
        }
        return instance;
    }

    /// <summary>
    /// Loads all Minecraft instances from the specified directory.
    /// </summary>
    public void Load()
    {
        Instances.Clear();
        string[] json = Directory.GetFiles(path, "instance.json", SearchOption.AllDirectories);
        foreach (string file in json)
        {
            Log.Debug("Attempting to load instance from file: {PATH}", file);
            string item = File.ReadAllText(file);
            InstanceModel? instance = JObject.Parse(item).ToObject<InstanceModel>();
            if (instance != null)
            {
                Log.Debug("Successfully loaded instance from file: {PATH}", file);
                instance.InstanceManager = this;
                Instances.Add(instance.Id, instance);
            }
            else
            {
                Log.Error("Failed to load instance file: {PATH}", file);
            }
        }
    }

    /// <summary>
    /// Loads a Minecraft instance from the specified path.
    /// </summary>
    /// <param name="path">The path to the directory containing the instance data.</param>
    /// <returns>The loaded <see cref="InstanceModel"/> instance, or null if loading failed.</returns>
    public InstanceModel? Load(string path)
    {
        string instanceFile = Path.Combine(path, "instance.json");
        InstanceModel? instance = JObject.Parse(File.ReadAllText(instanceFile)).ToObject<InstanceModel>();
        if (instance != null)
        {
            instance.InstanceManager = this;
            return Instances[instance.Id] = instance;
        }

        return null;
    }

    /// <summary>
    /// Adds a mod to a Minecraft instance.
    /// </summary>
    /// <param name="instance">
    /// The <see cref="InstanceModel"/> representing the instance to which the mod will be added.
    /// </param>
    /// <param name="mod">The <see cref="ModModel"/> representing the mod to be added.</param>
    public void AddMod(InstanceModel instance, ModModel mod)
    {
        List<ModModel> mods = new();
        mods.AddRange(instance.Mods);
        mods.Add(mod);
        instance.Mods = mods.ToArray();
        Save(instance.Id, instance);
    }

    /// <summary>
    /// Retrieves all instances with a specified name.
    /// </summary>
    /// <param name="name">The name of the instances to retrieve.</param>
    /// <returns>An array of <see cref="InstanceModel"/> instances with the specified name.</returns>
    public InstanceModel[] GetInstancesByName(string name) => Instances.Values.Where(i => i.Name == name).ToArray();

    /// <summary>
    /// Retrieves the first instance with a specified name.
    /// </summary>
    /// <param name="name">The name of the instance to retrieve.</param>
    /// <returns>The <see cref="InstanceModel"/> instance with the specified name.</returns>
    public InstanceModel GetFirstInstancesByName(string name) => Instances.Values.First(i => i.Name == name);

    /// <summary>
    /// Retrieves a specific instance by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the instance to retrieve.</param>
    /// <returns>The <see cref="InstanceModel"/> instance with the specified unique identifier.</returns>
    public InstanceModel GetInstanceById(Guid id) => Instances[id];

    /// <summary>
    /// Checks if an instance with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the instance to check.</param>
    /// <returns>True if an instance with the specified name exists; otherwise, false.</returns>
    public bool Exist(string name) => Instances.Values.Any(i => i.Name == name);

    private string GetUniqueInstanceDirectoryName(string name)
    {
        string dirname = name;

        foreach (char illegal in Path.GetInvalidFileNameChars())
        {
            dirname = dirname.Replace(illegal, '-');
        }

        string[] dirs = Directory.GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
        int index = 0;
        string originalDirname = dirname;

        while (Array.Exists(dirs, dir => dir.Equals(Path.Combine(path, dirname), StringComparison.OrdinalIgnoreCase)))
        {
            index++;
            dirname = $"{originalDirname} ({index})";
        }

        return dirname;
    }
}