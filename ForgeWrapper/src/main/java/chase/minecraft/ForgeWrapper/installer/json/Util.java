package chase.minecraft.ForgeWrapper.installer.json;

import chase.minecraft.ForgeWrapper.Main;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.Map;

public class Util
{
	public static Gson GSON = (new GsonBuilder()).setPrettyPrinting()
			.registerTypeAdapter(Artifact.class, new Artifact.Adapter())
			.create();
	
	public static InstallV1 loadInstallProfile(File installer)
	{
		byte[] data = null;
		try (InputStream stream = Main.getInstallerClassLoader().getResourceAsStream("install_profile.json"))
		{
			assert stream != null;
			data = readFully(stream);
		} catch (IOException e)
		{
			System.err.printf("Failed to extract install_profile.json from installer: %s", e.getMessage());
			e.printStackTrace();
		}
		Spec spec = (Spec) GSON.fromJson(new InputStreamReader(new ByteArrayInputStream(data), StandardCharsets.UTF_8), Spec.class);
		switch (spec.getSpec())
		{
			case 0 ->
			{
				return new InstallV1((Install) GSON.fromJson(new InputStreamReader(new ByteArrayInputStream(data), StandardCharsets.UTF_8), Install.class));
			}
			case 1 ->
			{
				return (InstallV1) GSON.fromJson(new InputStreamReader(new ByteArrayInputStream(data), StandardCharsets.UTF_8), InstallV1.class);
			}
		}
		throw new IllegalArgumentException("Invalid launcher profile spec: " + spec.getSpec() + " Only 0, and 1 are supported");
	}
	
	public static Mirror[] loadMirrorList(InputStream stream)
	{
		return (Mirror[]) GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), Mirror[].class);
	}
	
	public static Manifest loadManifest(InputStream stream)
	{
		return (Manifest) GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), Manifest.class);
	}
	
	public static Version loadVersion(Install profile)
	{
		String path = profile.getJson();
		path = path.substring(1);
		try (InputStream stream = Main.getInstallerClassLoader().getResourceAsStream(path))
		{
			return (Version) GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), Version.class);
		} catch (IOException e)
		{
			throw new RuntimeException(e);
		}
	}
	
	public static Version getVanillaVersion(String version, File target)
	{
		if (!target.exists())
		{
			Manifest manifest = DownloadUtils.downloadManifest();
			if (manifest == null)
				return null;
			String url = manifest.getUrl(version);
			if (url == null)
				return null;
			if (!DownloadUtils.downloadFile(target, url))
				return null;
		}
		try (InputStream stream = new FileInputStream(target))
		{
			return (Version) GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), Version.class);
		} catch (IOException e)
		{
			throw new RuntimeException(e);
		}
	}
	
	private static byte[] readFully(InputStream stream) throws IOException
	{
		int len;
		byte[] data = new byte[4096];
		ByteArrayOutputStream entryBuffer = new ByteArrayOutputStream();
		do
		{
			len = stream.read(data);
			if (len <= 0)
				continue;
			entryBuffer.write(data, 0, len);
		} while (len != -1);
		return entryBuffer.toByteArray();
	}
	
	public static String replaceTokens(Map<String, String> tokens, String value)
	{
		StringBuilder buf = new StringBuilder();
		for (int x = 0; x < value.length(); x++)
		{
			char c = value.charAt(x);
			if (c == '\\')
			{
				if (x == value.length() - 1)
					throw new IllegalArgumentException("Illegal pattern (Bad escape): " + value);
				buf.append(value.charAt(++x));
			} else if (c == '{' || c == '\'')
			{
				StringBuilder key = new StringBuilder();
				for (int y = x + 1; y <= value.length(); y++)
				{
					if (y == value.length())
						throw new IllegalArgumentException("Illegal pattern (Unclosed " + c + "): " + value);
					char d = value.charAt(y);
					if (d == '\\')
					{
						if (y == value.length() - 1)
							throw new IllegalArgumentException("Illegal pattern (Bad escape): " + value);
						key.append(value.charAt(++y));
					} else
					{
						if (c == '{' && d == '}')
						{
							x = y;
							break;
						}
						if (c == '\'' && d == '\'')
						{
							x = y;
							break;
						}
						key.append(d);
					}
				}
				if (c == '\'')
				{
					buf.append(key);
				} else
				{
					if (!tokens.containsKey(key.toString()))
						throw new IllegalArgumentException("Illegal pattern: " + value + " Missing Key: " + key);
					buf.append(tokens.get(key.toString()));
				}
			} else
			{
				buf.append(c);
			}
		}
		return buf.toString();
	}
}
