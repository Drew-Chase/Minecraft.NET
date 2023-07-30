package chase.minecraft.ForgeWrapper.installer.actions;

import java.io.File;
import java.io.IOException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.attribute.FileAttribute;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.jar.Attributes;
import java.util.jar.JarFile;
import java.util.stream.Collectors;
import javax.swing.JOptionPane;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.SimpleInstaller;
import chase.minecraft.ForgeWrapper.installer.json.Artifact;
import chase.minecraft.ForgeWrapper.installer.json.Install;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;
import chase.minecraft.ForgeWrapper.installer.json.Version;

public class PostProcessors {
  private final InstallV1 profile;
  
  private final boolean isClient;
  
  private final ProgressCallback monitor;
  
  private final boolean hasTasks;
  
  private final Map<String, String> data;
  
  private final List<Install.Processor> processors;
  
  public PostProcessors(InstallV1 profile, boolean isClient, ProgressCallback monitor) {
    this.profile = profile;
    this.isClient = isClient;
    this.monitor = monitor;
    this.processors = profile.getProcessors(isClient ? "client" : "server");
    this.hasTasks = !this.processors.isEmpty();
    this.data = profile.getData(isClient);
  }
  
  public Version.Library[] getLibraries() {
    return this.hasTasks ? this.profile.getLibraries() : new Version.Library[0];
  }
  
  public boolean process(File librariesDir, File minecraft, File root, File installer) {
    try {
      if (!this.data.isEmpty()) {
        StringBuilder err = new StringBuilder();
        Path temp = Files.createTempDirectory("forge_installer", (FileAttribute<?>[])new FileAttribute[0]);
        this.monitor.start("Created Temporary Directory: " + temp);
        double steps = this.data.size();
        int i = 1;
        for (String key : this.data.keySet()) {
          this.monitor.progress(i++ / steps);
          String value = this.data.get(key);
          if (value.charAt(0) == '[' && value.charAt(value.length() - 1) == ']') {
            this.data.put(key, Artifact.from(value.substring(1, value.length() - 1)).getLocalPath(librariesDir).getAbsolutePath());
            continue;
          } 
          if (value.charAt(0) == '\'' && value.charAt(value.length() - 1) == '\'') {
            this.data.put(key, value.substring(1, value.length() - 1));
            continue;
          } 
          File target = Paths.get(temp.toString(), new String[] { value }).toFile();
          this.monitor.message("  Extracting: " + value);
          if (!DownloadUtils.extractFile(value, target))
            err.append("\n  ").append(value); 
          this.data.put(key, target.getAbsolutePath());
        } 
        if (err.length() > 0) {
          error("Failed to extract files from archive: " + err.toString());
          return false;
        } 
      } 
      this.data.put("SIDE", this.isClient ? "client" : "server");
      this.data.put("MINECRAFT_JAR", minecraft.getAbsolutePath());
      this.data.put("MINECRAFT_VERSION", this.profile.getMinecraft());
      this.data.put("ROOT", root.getAbsolutePath());
      this.data.put("INSTALLER", installer.getAbsolutePath());
      this.data.put("LIBRARY_DIR", librariesDir.getAbsolutePath());
      int progress = 1;
      if (this.processors.size() == 1) {
        this.monitor.stage("Building Processor");
      } else {
        this.monitor.start("Building Processors");
      } 
      for (Install.Processor proc : this.processors) {
        this.monitor.progress(progress++ / this.processors.size());
        log("===============================================================================");
        Map<String, String> outputs = new HashMap<>();
        if (!proc.getOutputs().isEmpty()) {
          boolean miss = false;
          log("  Cache: ");
          for (Map.Entry<String, String> e : (Iterable<Map.Entry<String, String>>)proc.getOutputs().entrySet()) {
            String key = e.getKey();
            if (key.charAt(0) == '[' && key.charAt(key.length() - 1) == ']') {
              key = Artifact.from(key.substring(1, key.length() - 1)).getLocalPath(librariesDir).getAbsolutePath();
            } else {
              key = Util.replaceTokens(this.data, key);
            } 
            String value = e.getValue();
            if (value != null)
              value = Util.replaceTokens(this.data, value); 
            if (key == null || value == null) {
              error("  Invalid configuration, bad output config: [" + (String)e.getKey() + ": " + (String)e.getValue() + "]");
              return false;
            } 
            outputs.put(key, value);
            File artifact = new File(key);
            if (!artifact.exists()) {
              log("    " + key + " Missing");
              miss = true;
              continue;
            } 
            String sha = DownloadUtils.getSha1(artifact);
            if (sha.equals(value)) {
              log("    " + key + " Validated: " + value);
              continue;
            } 
            log("    " + key);
            log("      Expected: " + value);
            log("      Actual:   " + sha);
            miss = true;
            artifact.delete();
          } 
          if (!miss) {
            log("  Cache Hit!");
            continue;
          } 
        } 
        File jar = proc.getJar().getLocalPath(librariesDir);
        if (!jar.exists() || !jar.isFile()) {
          error("  Missing Jar for processor: " + jar.getAbsolutePath());
          return false;
        } 
        JarFile jarFile = new JarFile(jar);
        String mainClass = jarFile.getManifest().getMainAttributes().getValue(Attributes.Name.MAIN_CLASS);
        jarFile.close();
        if (mainClass == null || mainClass.isEmpty()) {
          error("  Jar does not have main class: " + jar.getAbsolutePath());
          return false;
        } 
        this.monitor.message("  MainClass: " + mainClass, ProgressCallback.MessagePriority.LOW);
        List<URL> classpath = new ArrayList<>();
        StringBuilder err = new StringBuilder();
        this.monitor.message("  Classpath:", ProgressCallback.MessagePriority.LOW);
        this.monitor.message("    " + jar.getAbsolutePath(), ProgressCallback.MessagePriority.LOW);
        classpath.add(jar.toURI().toURL());
        for (Artifact dep : proc.getClasspath()) {
          File lib = dep.getLocalPath(librariesDir);
          if (!lib.exists() || !lib.isFile())
            err.append("\n  ").append(dep.getDescriptor()); 
          classpath.add(lib.toURI().toURL());
          this.monitor.message("    " + lib.getAbsolutePath(), ProgressCallback.MessagePriority.LOW);
        } 
        if (err.length() > 0) {
          error("  Missing Processor Dependencies: " + err.toString());
          return false;
        } 
        List<String> args = new ArrayList<>();
        for (String arg : proc.getArgs()) {
          char start = arg.charAt(0);
          char end = arg.charAt(arg.length() - 1);
          if (start == '[' && end == ']') {
            args.add(Artifact.from(arg.substring(1, arg.length() - 1)).getLocalPath(librariesDir).getAbsolutePath());
          } else {
            args.add(Util.replaceTokens(this.data, arg));
          } 
        } 
        if (err.length() > 0) {
          error("  Missing Processor data values: " + err.toString());
          return false;
        } 
        this.monitor.message("  Args: " + (String)args.stream().map(a -> (a.indexOf(' ') != -1 || a.indexOf(',') != -1) ? ('"' + a + '"') : a).collect(Collectors.joining(", ")), ProgressCallback.MessagePriority.LOW);
        ClassLoader cl = new URLClassLoader(classpath.<URL>toArray(new URL[classpath.size()]), getParentClassloader());
        Thread currentThread = Thread.currentThread();
        ClassLoader threadClassloader = currentThread.getContextClassLoader();
        currentThread.setContextClassLoader(cl);
        try {
          Class<?> cls = Class.forName(mainClass, true, cl);
          Method main = cls.getDeclaredMethod("main", new Class[] { String[].class });
          main.invoke(null, new Object[] { args.toArray(new String[args.size()]) });
        } catch (InvocationTargetException ite) {
          Throwable e = ite.getCause();
          e.printStackTrace();
          if (e.getMessage() == null) {
            error("Failed to run processor: " + e.getClass().getName() + "\nSee log for more details.");
          } else {
            error("Failed to run processor: " + e.getClass().getName() + ":" + e.getMessage() + "\nSee log for more details.");
          } 
          return false;
        } catch (Throwable e) {
          e.printStackTrace();
          if (e.getMessage() == null) {
            error("Failed to run processor: " + e.getClass().getName() + "\nSee log for more details.");
          } else {
            error("Failed to run processor: " + e.getClass().getName() + ":" + e.getMessage() + "\nSee log for more details.");
          } 
          return false;
        } finally {
          currentThread.setContextClassLoader(threadClassloader);
        } 
        if (!outputs.isEmpty()) {
          for (Map.Entry<String, String> e : outputs.entrySet()) {
            File artifact = new File(e.getKey());
            if (!artifact.exists()) {
              err.append("\n    ").append(e.getKey()).append(" missing");
              continue;
            } 
            String sha = DownloadUtils.getSha1(artifact);
            if (sha.equals(e.getValue())) {
              log("  Output: " + (String)e.getKey() + " Checksum Validated: " + sha);
              continue;
            } 
            err.append("\n    ").append(e.getKey())
              .append("\n      Expected: ").append(e.getValue())
              .append("\n      Actual:   ").append(sha);
            if (!SimpleInstaller.debug && !artifact.delete())
              err.append("\n      Could not delete file"); 
          } 
          if (err.length() > 0) {
            error("  Processor failed, invalid outputs:" + err.toString());
            return false;
          } 
        } 
      } 
      return true;
    } catch (IOException e) {
      e.printStackTrace();
      return false;
    } 
  }
  
  private void error(String message) {
    if (!SimpleInstaller.headless)
      JOptionPane.showMessageDialog(null, message, "Error", 0); 
    for (String line : message.split("\n"))
      this.monitor.message(line); 
  }
  
  private void log(String message) {
    for (String line : message.split("\n"))
      this.monitor.message(line); 
  }
  
  private static boolean clChecked = false;
  
  private static ClassLoader parentClassLoader = null;
  
  private synchronized ClassLoader getParentClassloader() {
    if (!clChecked) {
      clChecked = true;
      if (!System.getProperty("java.version").startsWith("1."))
        try {
          Method getPlatform = ClassLoader.class.getDeclaredMethod("getPlatformClassLoader", new Class[0]);
          parentClassLoader = (ClassLoader)getPlatform.invoke(null, new Object[0]);
        } catch (NoSuchMethodException|IllegalAccessException|IllegalArgumentException|InvocationTargetException e) {
          log("No platform classloader: " + System.getProperty("java.version"));
        }  
    } 
    return parentClassLoader;
  }
}
