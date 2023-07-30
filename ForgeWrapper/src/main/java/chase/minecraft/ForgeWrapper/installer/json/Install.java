package chase.minecraft.ForgeWrapper.installer.json;

import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.SimpleInstaller;

import java.io.File;
import java.util.*;
import java.util.stream.Collectors;

public class Install extends Spec
{
	protected String profile;
	
	protected String version;
	
	protected String icon;
	
	protected String minecraft;
	
	protected String json;
	
	protected String logo;
	
	protected Artifact path;
	
	protected String urlIcon;
	
	protected String welcome;
	
	protected String mirrorList;
	
	protected boolean hideClient;
	
	protected boolean hideServer;
	
	protected boolean hideExtract = false;
	
	protected Version.Library[] libraries;
	
	protected List<Processor> processors;
	
	protected Map<String, DataFile> data;
	
	private Mirror mirror;
	
	private boolean triedMirrors = false;
	
	public String getProfile()
	{
		return this.profile;
	}
	
	public String getVersion()
	{
		return this.version;
	}
	
	public String getIcon()
	{
		return this.icon;
	}
	
	public String getMinecraft()
	{
		return this.minecraft;
	}
	
	public String getJson()
	{
		return this.json;
	}
	
	public String getLogo()
	{
		return this.logo;
	}
	
	public Artifact getPath()
	{
		return this.path;
	}
	
	public String getWelcome()
	{
		return (this.welcome == null) ? "" : this.welcome;
	}
	
	public String getMirrorList()
	{
		return this.mirrorList;
	}
	
	public Mirror getMirror()
	{
		if (this.mirror != null)
			return this.mirror;
		if (SimpleInstaller.mirror != null)
		{
			this.mirror = new Mirror("Mirror", "", "", SimpleInstaller.mirror.toString());
			return this.mirror;
		}
		if (getMirrorList() == null)
			return null;
		if (!this.triedMirrors)
		{
			this.triedMirrors = true;
			Mirror[] list = DownloadUtils.downloadMirrors(getMirrorList());
			this.mirror = (list == null || list.length == 0) ? null : list[(new Random()).nextInt(list.length)];
		}
		return this.mirror;
	}
	
	public boolean hideClient()
	{
		return this.hideClient;
	}
	
	public boolean hideServer()
	{
		return this.hideServer;
	}
	
	public boolean hideExtract()
	{
		return this.hideExtract;
	}
	
	public Version.Library[] getLibraries()
	{
		return (this.libraries == null) ? new Version.Library[0] : this.libraries;
	}
	
	public List<Processor> getProcessors(String side)
	{
		if (this.processors == null)
			return Collections.emptyList();
		return (List<Processor>) this.processors.stream().filter(p -> p.isSide(side)).collect(Collectors.toList());
	}
	
	public Map<String, String> getData(boolean client)
	{
		if (this.data == null)
			return new HashMap<>();
		return (Map<String, String>) this.data.entrySet().stream().collect(Collectors.toMap(Map.Entry::getKey, e -> client ? ((DataFile) e.getValue()).client : ((DataFile) e.getValue()).server));
	}
	
	public static class Processor
	{
		private List<String> sides;
		
		private Artifact jar;
		
		private Artifact[] classpath;
		
		private String[] args;
		
		private Map<String, String> outputs;
		
		public boolean isSide(String side)
		{
			return (this.sides == null || this.sides.contains(side));
		}
		
		public Artifact getJar()
		{
			return this.jar;
		}
		
		public Artifact[] getClasspath()
		{
			return (this.classpath == null) ? new Artifact[0] : this.classpath;
		}
		
		public String[] getArgs()
		{
			return (this.args == null) ? new String[0] : this.args;
		}
		
		public Map<String, String> getOutputs()
		{
			return (this.outputs == null) ? Collections.<String, String>emptyMap() : this.outputs;
		}
	}
	
	public static class DataFile
	{
		private String client;
		
		private String server;
	}
}
