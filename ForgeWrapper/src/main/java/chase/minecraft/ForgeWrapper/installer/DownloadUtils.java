package chase.minecraft.ForgeWrapper.installer;

import chase.minecraft.ForgeWrapper.Main;
import chase.minecraft.ForgeWrapper.installer.actions.ProgressCallback;
import chase.minecraft.ForgeWrapper.installer.json.*;

import javax.net.ssl.SSLHandshakeException;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.*;
import java.nio.file.CopyOption;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;
import java.util.Arrays;
import java.util.List;
import java.util.function.Predicate;
import java.util.stream.Collectors;

public class DownloadUtils
{
	public static boolean OFFLINE_MODE = false;
	
	public static boolean downloadLibrary(ProgressCallback monitor, Mirror mirror, Version.Library library, File root, Predicate<String> optional, List<Artifact> grabbed, List<File> additionalLibraryDirs)
	{
		Artifact artifact = library.getName();
		File target = artifact.getLocalPath(root);
		Version.LibraryDownload download = (library.getDownloads() == null) ? null : library.getDownloads().getArtifact();
		if (download == null)
		{
			download = new Version.LibraryDownload();
			download.setPath(artifact.getPath());
		}
		if (!optional.test(library.getName().getDescriptor()))
		{
			monitor.message(String.format("Considering library %s: Not Downloading {Disabled}", new Object[]{artifact.getDescriptor()}));
			return true;
		}
		monitor.message(String.format("Considering library %s", new Object[]{artifact.getDescriptor()}));
		if (target.exists())
			if (download.getSha1() != null)
			{
				String sha1 = getSha1(target);
				if (download.getSha1().equals(sha1))
				{
					monitor.message("  File exists: Checksum validated.");
					return true;
				}
				monitor.message("  File exists: Checksum invalid, deleting file:");
				monitor.message("    Expected: " + download.getSha1());
				monitor.message("    Actual:   " + sha1);
				if (!target.delete())
				{
					monitor.stage("    Failed to delete file, aborting.");
					return false;
				}
			} else
			{
				monitor.message("  File exists: No checksum, Assuming valid.");
				return true;
			}
		target.getParentFile().mkdirs();
		try (InputStream input = Main.getInstallerClassLoader().getResourceAsStream("maven/" + artifact.getPath()))
		{
			if (input != null)
			{
				monitor.message("  Extracting library from /maven/" + artifact.getPath());
				Files.copy(input, target.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
				if (download.getSha1() != null)
				{
					String sha1 = getSha1(target);
					if (download.getSha1().equals(sha1))
					{
						monitor.message("    Extraction completed: Checksum validated.");
						grabbed.add(artifact);
						return true;
					}
					monitor.message("    Extraction failed: Checksum invalid, deleting file:");
					monitor.message("      Expected: " + download.getSha1());
					monitor.message("      Actual:   " + sha1);
					if (!target.delete())
					{
						monitor.stage("      Failed to delete file, aborting.");
						return false;
					}
					return false;
				}
				monitor.message("    Extraction completed: No checksum, Assuming valid.");
				grabbed.add(artifact);
				return true;
			}
		} catch (IOException e)
		{
			e.printStackTrace();
			return false;
		}
		if (download.getSha1() != null)
		{
			String providedSha1 = download.getSha1();
			for (File libDir : additionalLibraryDirs)
			{
				File inLibDir = new File(libDir, artifact.getPath());
				if (inLibDir.exists())
				{
					monitor.message(String.format("  Found artifact in local folder %s", new Object[]{libDir.toString()}));
					String sha1 = getSha1(inLibDir);
					if (providedSha1.equals(sha1))
					{
						monitor.message("    Checksum validated");
					} else
					{
						monitor.message("    Invalid checksum. Not using.");
						continue;
					}
					try
					{
						Files.copy(inLibDir.toPath(), target.toPath(), new CopyOption[0]);
						monitor.message("    Successfully copied local file");
						grabbed.add(artifact);
						return true;
					} catch (IOException e)
					{
						e.printStackTrace();
						monitor.message(String.format("    Failed to copy from local folder: %s", new Object[]{e.toString()}));
						if (target.exists() &&
								!target.delete())
						{
							monitor.message("    Failed to delete failed copy, aborting");
							return false;
						}
					}
				}
			}
		}
		String url = download.getUrl();
		if (url == null || url.isEmpty())
		{
			monitor.message("  Invalid library, missing url");
			return false;
		}
		if (download(monitor, mirror, download, target))
		{
			grabbed.add(artifact);
			return true;
		}
		return false;
	}
	
	public static boolean download(ProgressCallback monitor, Mirror mirror, Version.LibraryDownload download, File target)
	{
		String url = download.getUrl();
		if (url.startsWith("http") && !url.startsWith("https://libraries.minecraft.net/") && mirror != null && url.endsWith(download.getPath()))
			if (download(monitor, mirror, (Version.Download) download, target, mirror.getUrl() + download.getPath()))
				return true;
		return download(monitor, mirror, (Version.Download) download, target, url);
	}
	
	public static boolean download(ProgressCallback monitor, Mirror mirror, Version.Download download, File target)
	{
		return download(monitor, mirror, download, target, download.getUrl());
	}
	
	private static boolean download(ProgressCallback monitor, Mirror mirror, Version.Download download, File target, String url)
	{
		monitor.message("  Downloading library from " + url);
		try
		{
			URLConnection connection = getConnection(url);
			if (connection != null)
			{
				Files.copy(connection.getInputStream(), target.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
				if (download.getSha1() != null)
				{
					String sha1 = getSha1(target);
					if (download.getSha1().equals(sha1))
					{
						monitor.message("    Download completed: Checksum validated.");
						return true;
					}
					monitor.message("    Download failed: Checksum invalid, deleting file:");
					monitor.message("      Expected: " + download.getSha1());
					monitor.message("      Actual:   " + sha1);
					if (!target.delete())
					{
						monitor.stage("      Failed to delete file, aborting.");
						return false;
					}
				}
				monitor.message("    Download completed: No checksum, Assuming valid.");
			}
		} catch (IOException e)
		{
			e.printStackTrace();
		}
		return false;
	}
	
	public static String getSha1(File target)
	{
		try
		{
			return HashFunction.SHA1.hash(Files.readAllBytes(target.toPath())).toString();
		} catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}
	
	private static boolean checksumValid(File target, String checksum)
	{
		if (checksum == null || checksum.isEmpty())
			return true;
		String sha1 = getSha1(target);
		return (sha1 != null && sha1.equals(checksum));
	}
	
	private static URLConnection getConnection(String address)
	{
		if (OFFLINE_MODE)
		{
			System.out.println("Offline Mode: Not downloading: " + address);
			return null;
		}
		URL url = null;
		try
		{
			url = new URL(address);
		} catch (MalformedURLException e)
		{
			e.printStackTrace();
			return null;
		}
		try
		{
			int MAX = 3;
			URLConnection connection = null;
			for (int x = 0; x < MAX; )
			{
				connection = url.openConnection();
				connection.setConnectTimeout(5000);
				connection.setReadTimeout(5000);
				if (connection instanceof HttpURLConnection)
				{
					HttpURLConnection hcon = (HttpURLConnection) connection;
					hcon.setInstanceFollowRedirects(false);
					int res = hcon.getResponseCode();
					if (res == 301 || res == 302)
					{
						String location = hcon.getHeaderField("Location");
						hcon.disconnect();
						if (x == MAX - 1)
						{
							System.out.println("Invalid number of redirects: " + location);
							return null;
						}
						System.out.println("Following redirect: " + location);
						url = new URL(url, location);
					}
					x++;
				}
			}
			return connection;
		} catch (SSLHandshakeException e)
		{
			System.out.println("Failed to establish connection to " + address);
			String host = url.getHost();
			System.out.println(" Host: " + host + " [" + (String) getIps(host).stream().collect(Collectors.joining(", ")) + "]");
			e.printStackTrace();
			return null;
		} catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}
	
	public static List<String> getIps(String host)
	{
		try
		{
			InetAddress[] addresses = InetAddress.getAllByName(host);
			return (List<String>) Arrays.<InetAddress>stream(addresses).map(InetAddress::getHostAddress).collect(Collectors.toList());
		} catch (UnknownHostException e1)
		{
			e1.printStackTrace();
			return null;
		}
	}
	
	public static Mirror[] downloadMirrors(String url)
	{
		try
		{
			URLConnection connection = getConnection(url);
			if (connection != null)
				try (InputStream stream = connection.getInputStream())
				{
					return Util.loadMirrorList(stream);
				}
		} catch (Throwable e)
		{
			e.printStackTrace();
		}
		return null;
	}
	
	public static Manifest downloadManifest()
	{
		try
		{
			URLConnection connection = getConnection("https://launchermeta.mojang.com/mc/game/version_manifest.json");
			if (connection != null)
				try (InputStream stream = connection.getInputStream())
				{
					return Util.loadManifest(stream);
				}
		} catch (IOException e)
		{
			e.printStackTrace();
		}
		return null;
	}
	
	public static boolean downloadFile(File target, String url)
	{
		try
		{
			URLConnection connection = getConnection(url);
			if (connection != null)
			{
				Files.copy(connection.getInputStream(), target.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
				return true;
			}
		} catch (IOException e)
		{
			e.printStackTrace();
		}
		return false;
	}
	
	public static boolean extractFile(Artifact art, File target, String checksum)
	{
		InputStream input = Main.getInstallerClassLoader().getResourceAsStream("maven/" + art.getPath());
		if (input == null)
		{
			System.out.println("File not found in installer archive: /maven/" + art.getPath());
			return false;
		}
		if (!target.getParentFile().exists())
			target.getParentFile().mkdirs();
		try
		{
			Files.copy(input, target.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
			return checksumValid(target, checksum);
		} catch (Exception e)
		{
			e.printStackTrace();
			return false;
		}
	}
	
	public static boolean extractFile(String name, File target)
	{
//		String path = (name.charAt(0) == '/') ? name : "/" + name;
		String path = (name.charAt(0) == '/') ? name.substring(1) : name;
		InputStream input = Main.getInstallerClassLoader().getResourceAsStream(path);
		if (input == null)
		{
			System.out.println("File not found in installer archive: " + path);
			return false;
		}
		if (!target.getParentFile().exists())
			target.getParentFile().mkdirs();
		try
		{
			Files.copy(input, target.toPath(), new CopyOption[]{StandardCopyOption.REPLACE_EXISTING});
			return true;
		} catch (Exception e)
		{
			e.printStackTrace();
			return false;
		}
	}
}
