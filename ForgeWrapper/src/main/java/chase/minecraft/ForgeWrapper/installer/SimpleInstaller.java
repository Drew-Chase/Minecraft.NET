package chase.minecraft.ForgeWrapper.installer;

import chase.minecraft.ForgeWrapper.installer.actions.ProgressCallback;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;

import javax.swing.*;
import java.io.*;
import java.net.URL;
import java.util.Locale;
import java.util.regex.Pattern;

public class SimpleInstaller
{
	public static boolean headless = false;
	
	public static boolean debug = false;
	
	public static URL mirror = null;
	
	public static File getMCDir()
	{
		String userHomeDir = System.getProperty("user.home", ".");
		String osType = System.getProperty("os.name").toLowerCase(Locale.ENGLISH);
		String mcDir = ".minecraft";
		if (osType.contains("win") && System.getenv("APPDATA") != null)
			return new File(System.getenv("APPDATA"), mcDir);
		if (osType.contains("mac"))
			return new File(new File(new File(userHomeDir, "Library"), "Application Support"), "minecraft");
		return new File(userHomeDir, mcDir);
	}
	
	public static void launchGui(ProgressCallback monitor, File installer)
	{
		try
		{
			UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
		} catch (Exception ignored)
		{
		}
		try
		{
			InstallV1 profile = Util.loadInstallProfile(installer);
//			InstallerPanel panel = new InstallerPanel(getMCDir(), profile, installer);
//			panel.run(monitor);
		
		
		
		} catch (Throwable e)
		{
			JOptionPane.showMessageDialog(null,
					"Something went wrong while installing.<br />Check log for more details:<br/>" + e.toString(), "Error", JOptionPane.ERROR_MESSAGE);
		}
	}
	
	public static OutputStream getLog() throws FileNotFoundException
	{
		File output, f = new File(SimpleInstaller.class.getProtectionDomain().getCodeSource().getLocation().getFile());
		if (f.isFile())
		{
			output = new File(f.getName() + ".log");
		} else
		{
			output = new File("installer.log");
		}
		return new BufferedOutputStream(new FileOutputStream(output));
	}
	
	static void hookStdOut(final ProgressCallback monitor)
	{
		final Pattern endingWhitespace = Pattern.compile("\\r?\\n$");
		OutputStream monitorStream = new OutputStream()
		{
			public void write(byte[] buf, int off, int len)
			{
				byte[] toWrite = new byte[len];
				System.arraycopy(buf, off, toWrite, 0, len);
				write(toWrite);
			}
			
			public void write(byte[] b)
			{
				String toWrite = new String(b);
				toWrite = endingWhitespace.matcher(toWrite).replaceAll("");
				if (!toWrite.isEmpty())
					monitor.message(toWrite);
			}
			
			public void write(int b)
			{
				write(new byte[]{(byte) b});
			}
		};
		System.setOut(new PrintStream(monitorStream));
		System.setErr(new PrintStream(monitorStream));
	}
}
