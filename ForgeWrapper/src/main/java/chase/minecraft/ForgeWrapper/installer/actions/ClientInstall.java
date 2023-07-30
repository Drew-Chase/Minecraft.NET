package chase.minecraft.ForgeWrapper.installer.actions;

import chase.minecraft.ForgeWrapper.Main;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;
import chase.minecraft.ForgeWrapper.installer.json.Version;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import java.io.*;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.charset.StandardCharsets;
import java.nio.file.CopyOption;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;
import java.util.ArrayList;
import java.util.function.Predicate;

public class ClientInstall extends Action
{
	public ClientInstall(InstallV1 profile, ProgressCallback monitor)
	{
		super(profile, monitor, true);
	}
	
	public boolean run(File target, Predicate<String> optionals, File installer) throws ActionCanceledException
	{
		if (!target.exists())
		{
			error("There is no minecraft installation at: " + target);
			return false;
		}
		File launcherProfiles = new File(target, "launcher_profiles.json");
		File launcherProfilesMS = new File(target, "launcher_profiles_microsoft_store.json");
		if (!launcherProfiles.exists() && !launcherProfilesMS.exists())
		{
			error("There is no minecraft launcher profile in \"" + target + "\", you need to run the launcher first!");
			return false;
		}
		File versionRoot = new File(target, "versions");
		File librariesDir = new File(target, "libraries");
		librariesDir.mkdir();
		checkCancel();
		this.monitor.stage("Extracting json");
		try (URLClassLoader classLoader = URLClassLoader.newInstance(new URL[]{Main.installer.toURI().toURL()}))
		{
			String path = profile.getJson();
			path = path.substring(1);
			try (InputStream stream = classLoader.getResourceAsStream(path))
			{
				File json = new File(versionRoot, this.profile.getVersion() + '/' + this.profile.getVersion() + ".json");
				json.getParentFile().mkdirs();
				Files.copy(stream, json.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
			} catch (IOException e)
			{
				error("  Failed to extract");
				e.printStackTrace();
				return false;
			}
		} catch (Exception e)
		{
			e.printStackTrace();
		}
		checkCancel();
		this.monitor.stage("Considering minecraft client jar");
		File versionVanilla = new File(versionRoot, this.profile.getMinecraft());
		if (!versionVanilla.mkdirs() && !versionVanilla.isDirectory())
		{
			if (!versionVanilla.delete())
			{
				error("There was a problem with the launcher version data. You will need to clear " + versionVanilla + " manually.");
				return false;
			}
			versionVanilla.mkdirs();
		}
		checkCancel();
		File clientTarget = new File(versionVanilla, this.profile.getMinecraft() + ".jar");
		if (!clientTarget.exists())
		{
			File versionJson = new File(versionVanilla, this.profile.getMinecraft() + ".json");
			Version vanilla = Util.getVanillaVersion(this.profile.getMinecraft(), versionJson);
			if (vanilla == null)
			{
				error("Failed to download version manifest, can not find client jar URL.");
				return false;
			}
			Version.Download client = vanilla.getDownload("client");
			if (client == null)
			{
				error("Failed to download minecraft client, info missing from manifest: " + versionJson);
				return false;
			}
			if (!DownloadUtils.download(this.monitor, this.profile.getMirror(), client, clientTarget))
			{
				clientTarget.delete();
				error("Downloading minecraft client failed, invalid checksum.\nTry again, or use the vanilla launcher to install the vanilla version.");
				return false;
			}
		}
		if (!downloadLibraries(librariesDir, optionals, new ArrayList<>()))
			return false;
		checkCancel();
		if (!this.processors.process(librariesDir, clientTarget, target, installer))
			return false;
		checkCancel();
		this.monitor.stage("Injecting profile");
		if (launcherProfiles.exists() && !injectProfile(launcherProfiles))
			return false;
		if (launcherProfilesMS.exists() && !injectProfile(launcherProfilesMS))
			return false;
		return true;
	}
	
	private boolean injectProfile(File target)
	{
		try
		{
			JsonObject json = null;
			try (InputStream stream = new FileInputStream(target))
			{
				json = JsonParser.parseReader(new InputStreamReader(stream, StandardCharsets.UTF_8)).getAsJsonObject();
			} catch (IOException e)
			{
				error("Failed to read " + target);
				e.printStackTrace();
				return false;
			}
			JsonObject _profiles = json.getAsJsonObject("profiles");
			if (_profiles == null)
			{
				_profiles = new JsonObject();
				json.add("profiles", (JsonElement) _profiles);
			}
			JsonObject _profile = _profiles.getAsJsonObject(this.profile.getProfile());
			if (_profile == null)
			{
				_profile = new JsonObject();
				_profile.addProperty("name", this.profile.getProfile());
				_profile.addProperty("type", "custom");
				_profiles.add(this.profile.getProfile(), (JsonElement) _profile);
			}
			_profile.addProperty("lastVersionId", this.profile.getVersion());
			String icon = this.profile.getIcon();
			if (icon != null)
				_profile.addProperty("icon", icon);
			String jstring = Util.GSON.toJson((JsonElement) json);
			Files.write(target.toPath(), jstring.getBytes(StandardCharsets.UTF_8), new java.nio.file.OpenOption[0]);
		} catch (IOException e)
		{
			error("There was a problem writing the launch profile,  is it write protected?");
			return false;
		}
		return true;
	}
	
	public boolean isPathValid(File targetDir)
	{
		return (targetDir.exists() && ((new File(targetDir, "launcher_profiles.json"))
				.exists() || (new File(targetDir, "launcher_profiles_microsoft_store.json"))
				.exists()));
	}
	
	public String getFileError(File targetDir)
	{
		if (targetDir.exists())
			return "The directory is missing a launcher profile. Please run the minecraft launcher first";
		return "There is no minecraft directory set up. Either choose an alternative, or run the minecraft launcher to create one";
	}
	
	public String getSuccessMessage()
	{
		if (downloadedCount() > 0)
			return String.format("Successfully installed client profile %s for version %s into launcher, and downloaded %d libraries", new Object[]{this.profile.getProfile(), this.profile.getVersion(), Integer.valueOf(downloadedCount())});
		return String.format("Successfully installed client profile %s for version %s into launcher", new Object[]{this.profile.getProfile(), this.profile.getVersion()});
	}
}
