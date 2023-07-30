package chase.minecraft.ForgeWrapper.installer.actions;

import chase.minecraft.ForgeWrapper.installer.json.InstallV1;

import java.util.function.BiFunction;
import java.util.function.Supplier;

public enum Actions
{
	CLIENT("Install client", "Install a new profile to the Mojang client launcher", ClientInstall::new, () -> "Successfully installed client into launcher."), SERVER("Install server", "Create a new modded server installation", ServerInstall::new, () -> "The server installed successfully"), EXTRACT("Extract", "Extract the contained jar file", ExtractAction::new, () -> "All files successfully extract.");
	
	private String label;
	
	private String tooltip;
	
	private BiFunction<InstallV1, ProgressCallback, Action> action;
	
	private Supplier<String> success;
	
	Actions(String label, String tooltip, BiFunction<InstallV1, ProgressCallback, Action> action, Supplier<String> success)
	{
		this.label = label;
		this.tooltip = tooltip;
		this.success = success;
		this.action = action;
	}
	
	public String getButtonLabel()
	{
		return this.label;
	}
	
	public String getTooltip()
	{
		return this.tooltip;
	}
	
	public String getSuccess()
	{
		return this.success.get();
	}
	
	public Action getAction(InstallV1 profile, ProgressCallback monitor)
	{
		return this.action.apply(profile, monitor);
	}
}
