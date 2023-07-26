/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chase.Minecraft.Instances;

public class InstanceManager
{
    private readonly string path;
    public Dictionary<Guid, InstanceModel> Instances { get; private set; }

    public InstanceManager(string path)
    {
        this.path = Directory.CreateDirectory(path).FullName;
        Instances = new();
        Load();
    }

    public InstanceModel Create(InstanceModel instance)
    {
        instance.Path = Directory.CreateDirectory(GetInstancePath(instance)).FullName;
        Instances.Add(instance.Id, instance);
        Save(instance.Id, instance);
        return instance;
    }

    public InstanceModel Save(Guid id, InstanceModel instance)
    {
        instance.LastModified = DateTime.Now;
        Instances[id] = instance;
        File.WriteAllText(Path.Combine(instance.Path, "instance.json"), JsonConvert.SerializeObject(instance));
        return instance;
    }

    public void Load()
    {
        Instances.Clear();
        string[] json = Directory.GetFiles(path, "instance.json", SearchOption.AllDirectories);
        foreach (string file in json)
        {
            string item = File.ReadAllText(file);
            InstanceModel? instance = JObject.Parse(item).ToObject<InstanceModel>();
            if (instance != null)
            {
                Instances.Add(instance.Id, instance);
            }
        }
    }

    public InstanceModel? Load(Guid id)
    {
        string instanceFile = Path.Combine(path, id.ToString(), "instance.json");
        InstanceModel? instance = JObject.Parse(File.ReadAllText(instanceFile)).ToObject<InstanceModel>();
        if (instance != null)
        {
            return Instances[id] = instance;
        }

        return null;
    }

    public void AddMod(InstanceModel instance, ModModel mod)
    {
        List<ModModel> mods = new();
        mods.AddRange(instance.Mods);
        mods.Add(mod);
        instance.Mods = mods.ToArray();
        Save(instance.Id, instance);
    }

    public string GetInstancePath(InstanceModel instance) => Path.Combine(path, instance.Id.ToString());

    public InstanceModel[] GetInstancesByName(string name) => Instances.Values.Where(i => i.Name == name).ToArray();

    public InstanceModel GetFirstInstancesByName(string name) => Instances.Values.First(i => i.Name == name);

    public InstanceModel GetInstanceById(string name) => Instances.Values.First(i => i.Name == name);

    public bool Exist(string name) => Instances.Values.Any(i => i.Name == name);
}