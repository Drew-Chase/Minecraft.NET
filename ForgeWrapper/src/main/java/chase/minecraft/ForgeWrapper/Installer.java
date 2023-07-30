package chase.minecraft.ForgeWrapper;

import chase.minecraft.ForgeWrapper.installer.actions.Action;
import chase.minecraft.ForgeWrapper.installer.actions.ActionCanceledException;
import chase.minecraft.ForgeWrapper.installer.actions.Actions;
import chase.minecraft.ForgeWrapper.installer.actions.ProgressCallback;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Path;
import java.util.*;
import java.util.function.Function;
import java.util.function.Predicate;

public class Installer
{
	private final File installer;
	private final File instance;
	private final File launcherProfile;
	
	public Installer(File installer, File instance, boolean verbose)
	{
		this.installer = installer;
		this.instance = instance;
		this.launcherProfile = Path.of(instance.getPath(), "launcher_profiles.json").toFile();
	}
	
	public boolean install()
	{
		createTempLauncherJson();
		
		InstallV1 profile = Util.loadInstallProfile(installer);
		Map<String, Function<ProgressCallback, Action>> actions = new HashMap<>();
		List<OptionalListEntry> optionals = new ArrayList<>();
		
		Predicate<String> optPred = input ->
		{
			Optional<OptionalListEntry> ent = optionals.stream().filter(OptionalListEntry::isEnabled).findFirst();
			return (ent.isEmpty() || ent.get().isEnabled());
		};
		
		Action action = Actions.CLIENT.getAction(profile, ProgressCallback.withOutputs(System.out));
		try
		{
			if (action.run(this.instance, optPred, this.installer))
			{
				return true;
			}
		} catch (ActionCanceledException e)
		{
			System.err.printf("Installation Canceled: %s", e.getMessage());
			e.printStackTrace();
		} catch (Exception e)
		{
			System.err.printf("There was an exception running task: %s", e.getMessage());
			e.printStackTrace();
		}
		return false;
	}
	
	
	private void createTempLauncherJson()
	{
		try (BufferedWriter writer = new BufferedWriter(new FileWriter(launcherProfile)))
		{
			writer.write("{\"profiles\": {}}");
			writer.flush();
			writer.close();
			launcherProfile.deleteOnExit();
		} catch (IOException e)
		{
			throw new RuntimeException(e);
		}
	}
	
	private static class OptionalListEntry
	{
		OptionalLibrary lib;
		
		private boolean enabled;
		
		public boolean isEnabled()
		{
			return this.enabled;
		}
	}
	
	private class OptionalLibrary
	{
		private String artifact;
		
		public String getArtifact()
		{
			return this.artifact;
		}
	}
}
