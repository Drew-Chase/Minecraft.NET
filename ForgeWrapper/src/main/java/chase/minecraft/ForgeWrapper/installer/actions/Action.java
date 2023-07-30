package chase.minecraft.ForgeWrapper.installer.actions;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.function.Predicate;
import javax.swing.JOptionPane;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.SimpleInstaller;
import chase.minecraft.ForgeWrapper.installer.json.Artifact;
import chase.minecraft.ForgeWrapper.installer.json.Install;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;
import chase.minecraft.ForgeWrapper.installer.json.Version;

public abstract class Action {
  protected final InstallV1 profile;
  
  protected final ProgressCallback monitor;
  
  protected final PostProcessors processors;
  
  protected final Version version;
  
  private List<Artifact> grabbed = new ArrayList<>();
  
  protected Action(InstallV1 profile, ProgressCallback monitor, boolean isClient) {
    this.profile = profile;
    this.monitor = monitor;
    this.processors = new PostProcessors(profile, isClient, monitor);
    this.version = Util.loadVersion((Install)profile);
  }
  
  protected void error(String message) {
    if (!SimpleInstaller.headless)
      JOptionPane.showMessageDialog(null, message, "Error", 0); 
    this.monitor.stage(message);
  }
  
  public abstract boolean run(File paramFile1, Predicate<String> paramPredicate, File paramFile2) throws ActionCanceledException;
  
  public abstract boolean isPathValid(File paramFile);
  
  public abstract String getFileError(File paramFile);
  
  public abstract String getSuccessMessage();
  
  public String getSponsorMessage() {
    return (this.profile.getMirror() != null) ? String.format(SimpleInstaller.headless ? "Data kindly mirrored by %2$s at %1$s" : "<html><a href='%s'>Data kindly mirrored by %s</a></html>", new Object[] { this.profile.getMirror().getHomepage(), this.profile.getMirror().getName() }) : null;
  }
  
  protected boolean downloadLibraries(File librariesDir, Predicate<String> optionals, List<File> additionalLibDirs) throws ActionCanceledException {
    this.monitor.start("Downloading libraries");
    String userHome = System.getProperty("user.home");
    if (userHome != null && !userHome.isEmpty()) {
      File mavenLocalHome = new File(userHome, ".m2/repository");
      if (mavenLocalHome.exists())
        additionalLibDirs.add(mavenLocalHome); 
    } 
    this.monitor.message(String.format("Found %d additional library directories", new Object[] { Integer.valueOf(additionalLibDirs.size()) }));
    List<Version.Library> libraries = new ArrayList<>();
    libraries.addAll(Arrays.asList(this.version.getLibraries()));
    libraries.addAll(Arrays.asList(this.processors.getLibraries()));
    StringBuilder output = new StringBuilder();
    double steps = libraries.size();
    int progress = 1;
    for (Version.Library lib : libraries) {
      checkCancel();
      this.monitor.progress(progress++ / steps);
      if (!DownloadUtils.downloadLibrary(this.monitor, this.profile.getMirror(), lib, librariesDir, optionals, this.grabbed, additionalLibDirs)) {
        Version.LibraryDownload download = (lib.getDownloads() == null) ? null : lib.getDownloads().getArtifact();
        if (download != null && !download.getUrl().isEmpty())
          output.append('\n').append(lib.getName()); 
      } 
    } 
    String bad = output.toString();
    if (!bad.isEmpty()) {
      error("These libraries failed to download. Try again.\n" + bad);
      return false;
    } 
    return true;
  }
  
  protected int downloadedCount() {
    return this.grabbed.size();
  }
  
  protected void checkCancel() throws ActionCanceledException {
    try {
      Thread.sleep(1L);
    } catch (InterruptedException e) {
      throw new ActionCanceledException(e);
    } 
  }
}
