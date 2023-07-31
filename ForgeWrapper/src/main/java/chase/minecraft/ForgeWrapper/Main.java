package chase.minecraft.ForgeWrapper;

import org.apache.commons.cli.*;

import java.io.File;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Path;

public class Main
{
	public static File installer;
	
	public static void main(String[] args)
	{
		CommandLineParser parser = new DefaultParser();
		
		Options options = new Options();
		options.addOption("h", "help", false, "Display's the help");
		options.addOption("i", "installer", true, "The path to the forge installer jar");
		options.addOption("o", "output", true, "The path to the output");
		
		if (args.length == 0)
		{
			printHelp(options, args);
		}
		try
		{
			CommandLine cmd = parser.parse(options, args);
			if (cmd.hasOption('h'))
			{
				printHelp(options, args);
			} else if (cmd.hasOption('i') && cmd.hasOption('o'))
			{
				boolean verbose = cmd.hasOption('v');
				Main.installer = Path.of(cmd.getOptionValue('i')).toFile();
				File output = Path.of(cmd.getOptionValue('o')).toFile();
				if (output.mkdirs())
				{
					System.out.printf("Creating output directory: %s\n", output);
				}
				if (!Main.installer.exists())
				{
					System.err.printf("Installer file not found: %s\n", Main.installer);
					System.exit(1);
				}
				Installer installer = new Installer(Main.installer, output, verbose);
				installer.install();
			}
		} catch (ParseException e)
		{
			printHelp(options, args);
		}
		
	}
	
	public static URLClassLoader getInstallerClassLoader()
	{
		try
		{
			return URLClassLoader.newInstance(new URL[]{installer.toURI().toURL()});
		} catch (MalformedURLException e)
		{
			throw new RuntimeException(e);
		}
	}
	
	private static void printHelp(Options options, String[] args)
	{
		HelpFormatter formatter = new HelpFormatter();
		formatter.printHelp("ForgeWrapper", options);
		System.exit(0);
	}
}
